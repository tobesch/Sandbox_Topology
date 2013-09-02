Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types
Imports Rhino.Geometry


Public Class EdgeDispatch
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the CurveDispatch class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Edge Dispatch", "Edge Disp", _
           "Returns edge segments for outlines, miters, and joints", _
           "Echinoid", "2 Toolpaths")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddLineParameter("Edges", "E", "List of edges in topological order", GH_ParamAccess.tree)
        pManager.AddBrepParameter("Plates", "B", "List of planar brep pairs in topological order", GH_ParamAccess.tree)
        pManager.AddNumberParameter("Joint width", "W", "Joint width in mm", GH_ParamAccess.item)
        pManager.AddNumberParameter("Miter length", "L", "Minimum miter length in mm", GH_ParamAccess.item)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddLineParameter("Miters", "M", "Edge segment for miter toolpath", GH_ParamAccess.tree)
        pManager.AddLineParameter("Joints", "J", "Edge segment for joint toolpath", GH_ParamAccess.tree)
        pManager.AddLineParameter("Outlines", "O", "Edge segment for outline toolpath", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim edges As New GH_Structure(Of GH_Line)
        Dim breps As New GH_Structure(Of GH_Brep)
        Dim dblJWidth As Double = Double.MinValue
        Dim dblMLength As Double = Double.MinValue

        '2. Retrieve input data.
        If (Not DA.GetDataTree(0, edges)) Then Return
        If (Not DA.GetDataTree(1, breps)) Then Return
        If (Not DA.GetData(2, dblJWidth)) Then Return
        If (Not DA.GetData(3, dblMLength)) Then Return

        '3. Abort on invalid inputs.
        If (edges.IsEmpty) Then Return
        If (breps.IsEmpty) Then Return
        If (Not Rhino.RhinoMath.IsValidDouble(dblJWidth)) Then Return
        If (Not Rhino.RhinoMath.IsValidDouble(dblMLength)) Then Return

        '4. Do something useful.
        '4.1 Decide if line must be flipped
        edges = decideIfLineMustBeFlipped(edges, breps)

        '4.2 Filter open edges
        Dim _outlines As GH_Structure(Of GH_Line) = filterOutlines(edges, breps)

        '4.3 Filter embedded edges
        Dim _embedded As GH_Structure(Of GH_Line) = filterEmbedded(edges, breps)

        '4.4 Calculate segment lengths
        Dim _mTree As New Grasshopper.DataTree(Of Line)
        Dim _jTree As New Grasshopper.DataTree(Of Line)

        For i As Int32 = 0 To _embedded.Branches.Count - 1
            Dim _path As GH_Path = _embedded.Path(i)
            Dim _edge As Line = _embedded.Branch(i).Item(0).Value
            Dim lenCrv As Double = _edge.Length

            'check if the is space on the edge for 2 miters and minimum 2 joints
            If lenCrv < 2 * dblMLength + 2 * dblJWidth Then
                'there is not enough space
                _mTree.Add(_edge, _path)
            Else
                Dim numSeg As Int32 = Math.Floor((lenCrv - 2 * dblMLength) / dblJWidth) - Math.Floor((lenCrv - 2 * dblMLength) / dblJWidth) Mod 2
                Dim distParam As Double = 0.5 * (lenCrv - numSeg * dblJWidth) / lenCrv

                '4.3 Create and assign edge segments
                Dim startMiter As New Interval
                startMiter.T0 = 0
                startMiter.T1 = distParam
                Dim lineStart As New Line(_edge.PointAt(startMiter.T0), _edge.PointAt(startMiter.T1))
                If lineStart.IsValid And lineStart.Length > 0 Then _mTree.Add(lineStart, _path)

                Dim endMiter As New Interval
                endMiter.T0 = 1 - distParam
                endMiter.T1 = 1

                Dim lineEnd As New Line(_edge.PointAt(endMiter.T0), _edge.PointAt(endMiter.T1))
                If lineEnd.IsValid And lineEnd.Length > 0 Then _mTree.Add(lineEnd, _path)

                Dim jointEdge As New Interval
                jointEdge.T0 = distParam
                jointEdge.T1 = 1 - distParam

                Dim lineJoint As New Line(_edge.PointAt(jointEdge.T0), _edge.PointAt(jointEdge.T1))
                If lineJoint.IsValid And lineJoint.Length > 0 Then _jTree.Add(lineJoint, _path)

            End If
            
        Next

        '5. Set data to output
        DA.SetDataTree(0, _mTree)
        DA.SetDataTree(1, _jTree)
        DA.SetDataTree(2, _outlines)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.Echinoid_2_EdgeDispatch
            'Return Nothing
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{2e2c709d-4b51-496a-8c63-305176a52772}")
        End Get
    End Property

    Private Function decideIfLineMustBeFlipped(ByVal C As GH_Structure(Of GH_Line), ByVal S As GH_Structure(Of GH_Brep)) As GH_Structure(Of GH_Line)

        Dim flipped As New GH_Structure(Of GH_Line)

        For i As Int32 = 0 To C.Branches.Count - 1

            Dim _path As GH_Path = C.Path(i)
            Dim _brep As Brep = S.Branch(i).Item(0).Value
            Dim myLoop As Curve = _brep.Loops.Item(0).To3dCurve
            Dim _line As Line = C.Branch(i).Item(0).Value
            Dim lineDir As Vector3d = _line.Direction
            lineDir.Unitize()
            Dim t As Double = Double.MinValue
            myLoop.ClosestPoint(_line.PointAt(0.5), t)
            Dim myDir As Vector3d = myLoop.TangentAt(t)
            myDir.Unitize()


            If Not ((Math.Round(lineDir.X, 3) = Math.Round(myDir.X, 3)) And (Math.Round(lineDir.Y, 3) = Math.Round(myDir.Y, 3)) And (Math.Round(lineDir.Z, 3) = Math.Round(myDir.Z, 3))) Then
                _line.Flip()
                Dim _ghLine As New GH_Line(_line)
                flipped.Append(_ghLine, _path)
            Else
                flipped.Append(C.Branch(i).Item(0), _path)
            End If

        Next

        Return flipped

    End Function

    Private Function filterOutlines(ByVal edges As GH_Structure(Of GH_Line), ByVal breps As GH_Structure(Of GH_Brep)) As GH_Structure(Of GH_Line)

        Dim _sorted As New GH_Structure(Of GH_Line)

        For i As Int32 = 0 To edges.Branches.Count - 1
            Dim _path As GH_Path = edges.Path(i)
            If Not breps.Branch(i).Count = 2 Then
                _sorted.Append(edges.Branch(i).Item(0), _path)
            End If
        Next

        Return _sorted

    End Function

    Private Function filterEmbedded(ByVal edges As GH_Structure(Of GH_Line), ByVal breps As GH_Structure(Of GH_Brep)) As GH_Structure(Of GH_Line)

        Dim _sorted As New GH_Structure(Of GH_Line)

        For i As Int32 = 0 To edges.Branches.Count - 1
            Dim _path As GH_Path = edges.Path(i)
            If breps.Branch(i).Count = 2 Then
                _sorted.Append(edges.Branch(i).Item(0), _path)
            End If
        Next

        Return _sorted

    End Function

End Class