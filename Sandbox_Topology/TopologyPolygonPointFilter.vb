Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Types
Imports Grasshopper.Kernel.Data


Public Class TopologyPolygonPointFilter
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the NakedPolygonVertices class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Polygon Topology Point Filter", "Poly Topo Point Filter", _
           "Filter the points in a polygon network based on their connectivity", _
           "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddPointParameter("Point list", "P", "Ordered list of points", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Point-Loop structure", "PL", "Ordered structure listing the polylines adjacent to each point", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Valency filter", "V", "Filter points with the specified number of adjacent polylines", GH_ParamAccess.item, 1)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddTextParameter("List of point IDs", "I", "List of point indices matching the valency criteria", GH_ParamAccess.list)
        pManager.AddPointParameter("List of points", "P", "List of points matching the valency criteria", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _P As New List(Of GH_Point)
        Dim _PF As GH_Structure(Of GH_Integer) = Nothing
        Dim _C As Int32 = 0

        '2. Retrieve input data.
        If (Not DA.GetDataList(0, _P)) Then Return
        If (Not DA.GetDataTree(1, _PF)) Then Return
        If (Not DA.GetData(2, _C)) Then Return

        '3. Abort on invalid inputs.
        '3.1. get the number of branches in the trees
        If (Not _P.Count > 0) Then Return
        If (Not _PF.PathCount > 0) Then Return
        If (Not _C > 0) Then Return

        '4. Do something useful.
        'Dim _ptList As List(Of Point3d) = _P
        Dim _pfTree As GH_Structure(Of GH_Integer) = _PF

        Dim _idList As New List(Of Int32)
        For Each _path As GH_Path In _pfTree.Paths
            If _pfTree.Branch(_path).Count = _C Then
                _idList.Add(_path.ToString.Trim(New Char() {"{", "}"}))
            End If
        Next

        Dim _ptList As New List(Of Point3d)
        For Each _id As Int32 In _idList

            _ptList.Add(_P.Item(_id).Value)

        Next

        DA.SetDataList(0, _idList)
        DA.SetDataList(1, _ptList)


    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.TopologyPolyFilterPoints
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