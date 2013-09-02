Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry


Public Class CurveInterpreter
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the CurveInterpreter class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Curve Interpreter", "Interpreter", _
           "Converts polyline into toolpath instruction set", _
           "Sandbox", "Fabrication")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddPointParameter("Base Origin", "B", "Origin of the base", GH_ParamAccess.item)
        pManager.AddPointParameter("Sheet Origin", "O", "Origin of the sheet", GH_ParamAccess.item)
        pManager.AddCurveParameter("Polylines", "C", "List of polylines to interpret", GH_ParamAccess.item)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddPointParameter("Points", "P", "NC-Code target points", GH_ParamAccess.list)
        pManager.AddVectorParameter("Vectors", "V", "NC-Code tool vectors", GH_ParamAccess.list)
        pManager.AddTextParameter("Feed", "F", "NC-Code feed information", GH_ParamAccess.list)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim oB As Point3d = Point3d.Unset
        Dim oPt As Point3d = Point3d.Unset
        Dim crv As Curve = Nothing

        '2. Retrieve input data.
        If (Not DA.GetData(0, oB)) Then Return
        If (Not DA.GetData(1, oPt)) Then Return
        If (Not DA.GetData(2, crv)) Then Return

        '3. Abort on invalid inputs.
        If (Not oB.IsValid) Then Return
        If (Not oPt.IsValid) Then Return
        If (Not crv.IsValid) Then Return
        If (Not crv.IsPolyline) Then Return

        '4. Do something useful.
        '4.1 Try to get the polyline representation
        'Dim crvList As New List(Of NurbsCurve)
        'crv = crv.ToNurbsCurve()
        'crvList.Add(crv)
        'Dim points As Rhino.Geometry.Collections.NurbsCurvePointList = crvList.Item(0).Points
        Dim poly As Polyline = Nothing
        If (Not crv.TryGetPolyline(poly)) Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not get polyline!")
            Return
        End If

        Dim CX As Rhino.Collections.Point3dList.XAccess = poly.X
        Dim CY As Rhino.Collections.Point3dList.YAccess = poly.Y
        Dim CZ As Rhino.Collections.Point3dList.ZAccess = poly.Z

        Dim pts As New List(Of Point3d)
        Dim vts As New List(Of Vector3d)
        Dim fds As New List(Of String)

        ' points
        Dim pt As Point3d = Point3d.Unset
        For i As Int32 = 0 To poly.Count - 1
            pt.X = CX.Item(i) - oPt.X + oB.X
            pt.Y = CY.Item(i) - oPt.Y + oB.Y
            pt.Z = CZ.Item(i) - oPt.Z + oB.Z
            pts.Add(pt)
        Next

        ' vectors
        Dim x As Double = CX.Item(0) - CX.Item(1)
        Dim y As Double = CY.Item(0) - CY.Item(1)
        Dim z As Double = CZ.Item(0) - CZ.Item(1)
        Dim vt As New Vector3d(x, y, z)
        vt.Unitize()

        For i As Int32 = 0 To pts.Count - 1
            vts.Add(vt)
        Next

        'feed
        Dim fd As String = String.Empty
        For i As Int32 = 0 To pts.Count - 1
            If i = 0 Or i = pts.Count - 1 Then
                fd = "G0"
            Else
                fd = "G1"
            End If
            fds.Add(fd)
        Next

        DA.SetDataList(0, pts)
        DA.SetDataList(1, vts)
        DA.SetDataList(2, fds)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.Echinoid_3_Interpreter
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{ce83482d-9124-463b-962c-c6cf710f51b7}")
        End Get
    End Property
End Class