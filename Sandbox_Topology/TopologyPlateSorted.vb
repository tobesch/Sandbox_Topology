Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Rhino.Geometry


Public Class TopologyPlateSorted
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the PlateTopologySorted class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Plate Topology Sorted", "Topo Sort", _
           "Returns faces and edges in topological order", _
           "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddBrepParameter("Brep", "B", "Brep to analyse", GH_ParamAccess.item)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddCurveParameter("Edges", "E", "List of edges by face", GH_ParamAccess.tree)
        pManager.AddBrepParameter("Faces", "F", "List of adjacent faces by edge", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim oBrep As Rhino.Geometry.Brep = Nothing

        '2. Retrieve input data.
        If (Not DA.GetData(0, oBrep)) Then Return

        '3. Abort on invalid inputs.
        If (Not oBrep.IsValid) Then Return

        '4. Check for non-manifold Breps
        If (Not oBrep.IsManifold) Then Return

        '5. Check if the topology is valid
        Dim log As String = String.Empty
        If (Not oBrep.IsValidTopology(log)) Then Return

        '6. Now do something productive
        '6.1 Get the edge tree
        Dim ei_Tree As Grasshopper.DataTree(Of Int32) = getEdgeIndexTree(oBrep)

        '6.2 Get the face tree
        Dim fi_Tree As Grasshopper.DataTree(Of Int32) = getFaceIndexTree(oBrep)

        '6.3 Get surfaces pairs in edge order per face
        Dim brep_Tree As Grasshopper.DataTree(Of Brep) = getFaceTree(oBrep, fi_Tree, ei_Tree)

        '6.4 Get edges in edge order per face
        Dim edge_Tree As Grasshopper.DataTree(Of BrepEdge) = getEdgeTree(oBrep, fi_Tree, ei_Tree)

        '7. Set data to output
        DA.SetDataTree(0, edge_Tree)
        DA.SetDataTree(1, brep_Tree)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.Echinoid_1_TopologySorted
            'Return Nothing
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property
    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{0a139369-17cd-427f-be69-0be7f61a6466}")
        End Get
    End Property

    Private Function getEdgeIndexTree(ByVal oBrep As Brep) As Grasshopper.DataTree(Of Int32)
        Dim face As BrepFace = Nothing
        Dim e_tree As New Grasshopper.DataTree(Of Int32)

        For i As Int32 = 0 To oBrep.Faces.Count - 1
            face = oBrep.Faces.Item(i)
            Dim edges As Int32() = face.AdjacentEdges()

            Dim e_path As New GH_Path(i)
            For j As Int32 = 0 To UBound(edges)
                e_tree.Add(edges(j), e_path)
            Next
        Next
        Return e_tree
    End Function

    Private Function getFaceIndexTree(ByVal oBrep As Brep) As Grasshopper.DataTree(Of Int32)
        Dim edge As BrepEdge = Nothing
        Dim f_tree As New Grasshopper.DataTree(Of Int32)

        For i As Int32 = 0 To oBrep.Edges.Count - 1
            edge = oBrep.Edges.Item(i)
            Dim faces As Int32() = edge.AdjacentFaces()

            Dim path As New GH_Path(i)
            For j As Int32 = 0 To UBound(faces)
                f_tree.Add(faces(j), path)
            Next
        Next
        Return f_tree
    End Function

    Private Function getEdgeTree(ByVal oBrep As Brep, ByVal fi_Tree As Grasshopper.DataTree(Of Int32), ByRef ei_Tree As Grasshopper.DataTree(Of Int32)) As Grasshopper.DataTree(Of BrepEdge)

        Dim line_Tree As New Grasshopper.DataTree(Of BrepEdge)

        For i As Int32 = 0 To ei_Tree.Branches.Count - 1

            For j As Int32 = 0 To ei_Tree.Branch(i).Count - 1

                Dim path As New GH_Path(i, j)
                Dim getpath As New GH_Path(ei_Tree.Branch(i).Item(j))
                'Dim indices As List(Of Int32) = fi_Tree.Branch(getpath)

                'If indices.Count > 1 Then

                'A = L.item(E)
                Dim oEdge As BrepEdge = oBrep.Edges.Item(ei_Tree.Branch(i).Item(j))
                'If oEdge.IsLinear Then
                'Dim _line As New Line(oEdge.PointAtNormalizedLength(0), oEdge.PointAtNormalizedLength(1)) 'oLine.to()
                'line_Tree.Add(_line.ToNurbsCurve, path)
                'Else
                line_Tree.Add(oEdge, path)
                'End If


                'Else

                'B = L.item(E)
                'Btree.Add(L.item(E.Branch(i).item(j)), path)
                'End If

            Next

        Next

        Return line_Tree
    End Function

    Private Function getFaceTree(ByVal oBrep As Brep, ByVal fi_Tree As Grasshopper.DataTree(Of Int32), ByRef ei_Tree As Grasshopper.DataTree(Of Int32)) As Grasshopper.DataTree(Of Brep)

        Dim face_Tree As New Grasshopper.DataTree(Of Brep)
        Dim faces As New List(Of Brep)
        Dim copyBrep As Brep = oBrep.DuplicateBrep

        For faceindex As Int32 = 0 To oBrep.Faces.Count - 1
            faces.Add(copyBrep.Faces.ExtractFace(faceindex))
        Next


        For i As Int32 = 0 To ei_Tree.Branches.Count - 1

            For j As Int32 = 0 To ei_Tree.Branch(i).Count - 1

                Dim path As New GH_Path(i, j)
                Dim getPath As New GH_Path(ei_Tree.Branch(i).Item(j))
                Dim indices As List(Of Int32) = fi_Tree.Branch(getPath)

                'If indices.Count > 1 Then

                If indices.Item(0) <> i Then
                    indices.Reverse()
                End If

                For Each index As Int32 In indices
                    'Dim faceindex As Int32 = oBrep.Faces.Item(index).FaceIndex
                    'Dim oLoop As BrepLoop = oBrep.Faces(index).OuterLoop
                    'Dim face As Brep() = Rhino.Geometry.Brep.CreatePlanarBreps(oLoop)
                    'If face.IsValid Then
                    face_Tree.Add(faces(index), path)
                    'face_Tree.Add(oBrep.Faces.ExtractFace(faceindex), path)
                    'Else
                    'Print("Something went wrong")
                    'End If
                Next

            Next

        Next

        Return face_Tree
    End Function

End Class