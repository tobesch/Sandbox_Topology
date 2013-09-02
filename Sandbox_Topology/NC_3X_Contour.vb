Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry


Public Class NC3XContour
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the NC_3X_Contour class.
    ''' </summary>
    Public Sub New()
        MyBase.New("3X Contour", "3X Contour", _
           "Generates tooling information for 3-axis contour cutting", _
           "Sandbox", "CNC")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(pManager As GH_Component.GH_InputParamManager)
        pManager.AddCurveParameter("Polyline", "Pl", "Polyline as input", GH_ParamAccess.item)
        pManager.AddNumberParameter("Safety offset", "S", "Safety plane offset", GH_ParamAccess.item)
        pManager.AddPlaneParameter("Base", "B", "Base plane", GH_ParamAccess.item, Plane.WorldXY)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(pManager As GH_Component.GH_OutputParamManager)
        pManager.AddPointParameter("Points", "P", "NC points", GH_ParamAccess.list)
        pManager.AddVectorParameter("Vectors", "V", "NC tool vectors", GH_ParamAccess.list)
        pManager.AddTextParameter("Feed", "F", "NC Feed data", GH_ParamAccess.list)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _Pl As Curve = Nothing
        Dim _S As Double = Double.MinValue
        Dim _B As Plane = Plane.Unset

        '2. Retrieve input data.
        If (Not DA.GetData(0, _PL)) Then Return
        If (Not DA.GetData(1, _S)) Then Return
        If (Not DA.GetData(2, _B)) Then Return

        '3. Abort on invalid inputs.
        If Not (_Pl.IsPolyline) Then Return
        If Not (_S > 0) Then Return
        If Not (_B.IsValid) Then Return

        '4. Do something useful.
        Dim poly As Polyline = Nothing
        If (Not _Pl.TryGetPolyline(poly)) Then
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not get polyline!")
            Return
        End If

        Dim _zAxis As Vector3d = Vector3d.ZAxis
        Dim _zDir As Vector3d = Vector3d.Multiply(_zAxis, _S)

        Dim pts As New List(Of Point3d)
        Dim vts As New List(Of Vector3d)
        Dim fds As New List(Of String)

        ' points
        For i As Int32 = 0 To poly.Count - 1
            pts.Add(poly(i) - _B.Origin)
        Next


        ' vectors
        _zAxis.Unitize()
        For i As Int32 = 0 To pts.Count - 1
            vts.Add(_zAxis)
        Next

        'feed
        Dim fd As String = "G1"
        For i As Int32 = 0 To pts.Count - 1
            fds.Add(fd)
        Next

        Dim pt1, pt0 As Point3d
        pt0 = pts.Item(0)
        pt0.Transform(Transform.Translation(_zDir))
        pts.Insert(0, pt0)
        vts.Insert(0, _zAxis)
        fds.Insert(0, "G0")

        pt1 = pts.Item(pts.Count - 1)
        pt1.Transform(Transform.Translation(_zDir))
        pts.Add(pt1)
        vts.Add(_zAxis)
        fds.Add("G0")

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
            ' return Resources.IconForThisComponent;
            Return Nothing
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
            Return New Guid("{321434ff-503a-4681-8c00-7ce4beb10e31}")
        End Get
    End Property
End Class