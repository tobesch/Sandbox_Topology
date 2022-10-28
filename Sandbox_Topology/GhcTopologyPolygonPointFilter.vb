Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Types
Imports Grasshopper.Kernel.Data


Public Class GhcTopologyPolygonPointFilter
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the NakedPolygonVertices class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Polygon Topology Point Filter", "Poly Topo Point Filter",
           "Filter the points in a polygon network based on their connectivity",
           "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddPointParameter("Point list", "P", "Ordered list of points", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Point-Loop structure", "PL", "Ordered structure listing the polylines adjacent to each point", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Valency filter", "V", "Filter points with the specified number of adjacent polylines", GH_ParamAccess.item, 1)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddTextParameter("List of point IDs", "I", "List of point indices matching the valency criteria", GH_ParamAccess.tree)
        pManager.AddPointParameter("List of points", "P", "List of points matching the valency criteria", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _P As New GH_Structure(Of GH_Point)
        Dim _PF As New GH_Structure(Of GH_Integer)
        Dim _V As Int32 = 0

        '2. Retrieve input data.
        If (Not DA.GetDataTree(0, _P)) Then Return
        If (Not DA.GetDataTree(1, _PF)) Then Return
        If (Not DA.GetData(2, _V)) Then Return

        '3. Abort on invalid inputs.
        '3.1. get the number of branches in the trees
        If (Not _P.PathCount > 0) Then Return
        If (Not _PF.PathCount > 0) Then Return
        If (Not _V > 0) Then Return

        '4. Do something useful.
        'Dim _ptList As List(Of Point3d) = _P
        Dim _pfTree As GH_Structure(Of GH_Integer) = _PF

        Dim _idTree As New Grasshopper.DataTree(Of Int32)
        Dim _ptTree As New Grasshopper.DataTree(Of Point3d)

        For i As Int32 = 0 To _P.Branches.Count - 1

            Dim branch As List(Of GH_Point) = _P.Branch(i)
            Dim mainpath As New GH_Path(i)

            For j As Int32 = 0 To branch.Count - 1
                Dim args As Integer() = New Integer() {i, j}
                Dim path As New GH_Path(args)
                If _PF.Branch(path).Count = _V Then
                    _idTree.Add(j, mainpath)
                    _ptTree.Add(branch.Item(j).Value, mainpath)
                End If
            Next

        Next

        DA.SetDataTree(0, _idTree)
        DA.SetDataTree(1, _ptTree)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.TopologyPolyPointFilter
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{340977b2-12a6-4dbd-9dac-b2a6c058c5e8}")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property
End Class