Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Types


Public Class NC3XDrilling
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the NC_3X_Drilling class.
    ''' </summary>
    Public Sub New()
        MyBase.New("3X Drilling", "3X Drilling", _
           "Generates tooling information for 3-axis drilling", _
           "Sandbox", "CNC")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(pManager As GH_Component.GH_InputParamManager)
        pManager.AddPointParameter("Points", "P", "Points as input", GH_ParamAccess.item)
        pManager.AddNumberParameter("Depth", "D", "Drilling depth", GH_ParamAccess.item, 0)
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
        Dim _P As GH_Point = Nothing
        Dim _D As Double = Double.NaN
        Dim _S As Double = Double.MinValue
        Dim _B As Plane = Plane.Unset

        '2. Retrieve input data.
        If (Not DA.GetData(0, _P)) Then Return
        If (Not DA.GetData(1, _D)) Then Return
        If (Not DA.GetData(2, _S)) Then Return
        If (Not DA.GetData(3, _B)) Then Return

        '3. Abort on invalid inputs.
        If Not (_P.IsValid) Then Return
        If Double.IsNaN(_D) Then Return
        If Not (_S > 0) Then Return
        If Not (_B.IsValid) Then Return

        '4. Do something useful.
        Dim zAxis As Vector3d = Vector3d.ZAxis
        Dim zDirD As Vector3d = Vector3d.Multiply(zAxis, _D)
        Dim zDirS As Vector3d = Vector3d.Multiply(zAxis, _S)

        Dim pt1, pt2 As Point3d
        Dim pts As New List(Of Point3d)
        Dim vts As New List(Of Vector3d)
        Dim fds As New List(Of String)

        pt1 = _P.Value - _B.Origin
        pt1.Transform(Transform.Translation(zDirS))

        pts.Add(pt1)
        vts.Add(zAxis)
        fds.Add("G0")

        pt2 = _P.Value - _B.Origin
        pt2.Transform(Transform.Translation(zDirD))
        pts.Add(pt2)
        vts.Add(zAxis)
        fds.Add("G1")

        ' last point is the same as first point
        pts.Add(pt1)
        vts.Add(zAxis)
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
            Return New Guid("{915b478b-bcb3-4a34-973e-951d9b63d8f3}")
        End Get
    End Property
End Class