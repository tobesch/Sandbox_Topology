Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry


Public Class CreateOutline
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the CreateOutline class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Create Outline", "Outline", _
                 "Creates outline toolpath from open edge", _
                 "Echinoid", "2 Toolpaths")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddCurveParameter("Edge", "E", "Current edge to work on", GH_ParamAccess.item)
        pManager.AddVectorParameter("Normals", "N", "Normal vector of the plates to which the edge belongs", GH_ParamAccess.item)
        pManager.AddNumberParameter("Thickness", "T", "Material thickness (mm)", GH_ParamAccess.item)
        pManager.AddNumberParameter("Radius", "R", "Tool radius (mm)", GH_ParamAccess.item)
        pManager.AddNumberParameter("Safety", "S", "Safety distance (mm)", GH_ParamAccess.item)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddCurveParameter("Outline", "O", "Outline toolpath for current edge", GH_ParamAccess.item)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _crv As Curve = Nothing
        Dim normal As Vector3d = Vector3d.Unset
        Dim dblThick As Double = Double.MinValue
        Dim dblRadius As Double = Double.MinValue
        Dim dblSafety As Double = Double.MinValue

        '2. Retrieve input data.
        If (Not DA.GetData(0, _crv)) Then Return
        If (Not DA.GetData(1, normal)) Then Return
        If (Not DA.GetData(2, dblThick)) Then Return
        If (Not DA.GetData(3, dblRadius)) Then Return
        If (Not DA.GetData(4, dblSafety)) Then Return

        '3. Abort on invalid inputs.
        If (Not _crv.IsValid) Then Return
        If (Not normal.IsValid) Then Return
        If (Not Rhino.RhinoMath.IsValidDouble(dblThick)) Then Return
        If (Not Rhino.RhinoMath.IsValidDouble(dblRadius)) Then Return
        If (Not Rhino.RhinoMath.IsValidDouble(dblSafety)) Then Return

        '4. Do something useful.


        '4.1 Translation based on material thickness
        normal.Unitize()
        _crv.Transform(Rhino.Geometry.Transform.Translation(-dblThick * normal))

        '4.2 Translation based on tool thickness
        Dim perpVec As Vector3d = normal
        If _crv.IsLinear Then
            perpVec.Rotate(0.5 * Math.PI, _crv.TangentAt(_crv.Domain.Mid))
            edge.Transform(Rhino.Geometry.Transform.Translation(dblRadius * perpVec))
        End If

        Dim points(5) As Point3d
        points(2) = edge.From
        points(3) = edge.To

        '4.3 Define lead-in and lead-out points
        points(1) = points(2)
        points(1).Transform(Rhino.Geometry.Transform.Translation(dblRadius * perpVec))
        points(0) = points(1)
        points(0).Transform(Rhino.Geometry.Transform.Translation(dblSafety * normal))

        points(4) = points(3)
        points(4).Transform(Rhino.Geometry.Transform.Translation(dblRadius * perpVec))
        points(5) = points(4)
        points(5).Transform(Rhino.Geometry.Transform.Translation(dblSafety * normal))

        '4.N Create polyline
        Dim myPoly As New Polyline(points)
        'Print(Err.Description)

        '5. Set data to output
        DA.SetData(0, myPoly)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.Echinoid_2_Outline
            'Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{ed81aaa1-0ae9-49aa-a0fd-086a1622f429}")
        End Get
    End Property
End Class