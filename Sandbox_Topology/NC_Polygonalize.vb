Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry


Public Class NCPolygonalize
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the NC_Polygonalize class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Polygonalize Curve", "Polygonalize", _
           "Polygonalize curves based on deviation/chord height from input curve", _
           "Sandbox", "CNC")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(pManager As GH_Component.GH_InputParamManager)
        pManager.AddCurveParameter("Curve", "C", "Input curve for polygonalization", GH_ParamAccess.item)
        pManager.AddNumberParameter("Tolerance", "t", "Tolerance value defining chord height", GH_ParamAccess.item, 1.0)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(pManager As GH_Component.GH_OutputParamManager)
        pManager.AddCurveParameter("Polyline", "Pl", "Polyline output", GH_ParamAccess.item)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _C As Curve = Nothing
        Dim _t As Double = Double.MinValue

        '2. Retrieve input data.
        If (Not DA.GetData(0, _C)) Then Return
        If (Not DA.GetData(1, _t)) Then Return

        '3. Abort on invalid inputs.
        '3.1. get the number of branches in the trees
        If Not (_C.IsValid) Then Return
        If Not (_t > 0) Then Return

        '4. Do something useful.
        Dim _crv As PolylineCurve = _C.ToPolyline(0, 1, 0, 0, 0, _t, 0, 0, True)

        DA.SetData(0, _crv)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            ' return Resources.IconForThisComponent;
            Return My.Resources.Sandbox_Polygonalize
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
            Return New Guid("{efd04c72-64fe-448f-98eb-43c94af77677}")
        End Get
    End Property
End Class