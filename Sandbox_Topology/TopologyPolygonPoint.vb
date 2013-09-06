Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry

Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types


Public Class TopologyPolygonPoint
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the PolygonTopology class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Polygon Topology Point", "Poly Topo Point", _
     "Analyses the point topology of a network consisting of closed polylines", _
     "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddCurveParameter("List of polylines", "C", "Network of closed polylines", GH_ParamAccess.list)
        pManager.AddNumberParameter("Tolerance", "T", "Tolerance value", GH_ParamAccess.item, 0.001)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddPointParameter("List of points", "P", "Ordered list of unique points", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Loop-Point structure", "LP", "For each polyline lists all point indices", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Point-Loop structure", "PL", "For each point lists all adjacent polyline indices", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _C As New List(Of GH_Curve)
        Dim _T As Double = 0

        '2. Retrieve input data.
        If (Not DA.GetDataList(0, _C)) Then Return
        If (Not DA.GetData(1, _T)) Then Return

        '3. Abort on invalid inputs.
        If (Not _C.Count > 0) Then Return
        If (Not _T > 0) Then Return

        '4. Do something useful.
        Dim _polyList As New List(Of Polyline)

        '4.1. check inputs
        For Each _crv As GH_Curve In _C
            Dim _poly As Polyline = Nothing
            If Not _crv.Value.TryGetPolyline(_poly) Then Return
            _polyList.Add(_poly)
        Next

        '4.2. get topology
        Dim _ptList As List(Of PointTopological) = getPointTopo(_polyList, _T)
        Dim _fList As List(Of PLineTopological) = getPLineTopo(_polyList, _ptList, _T)
        Call setPointPLineTopo(_fList, _ptList)
        'Dim _ptFaceDict As Dictionary(Of String, List(Of String)) = getPointFaceDict(_fList, _ptList)

        ' 4.3: return results
        Dim _PValues As New List(Of Point3d)
        For Each _ptTopo As PointTopological In _ptList
            _PValues.Add(_ptTopo.Point)
        Next

        Dim _FPValues As New Grasshopper.DataTree(Of Int32)
        For Each _lineTopo As PLineTopological In _fList
            Dim _path As New GH_Path(_FPValues.BranchCount)
            For Each _index As Int32 In _lineTopo.PointIndices
                _FPValues.Add(_index, _path)
            Next
        Next

        Dim _PFValues As New Grasshopper.DataTree(Of Int32)
        For Each _ptTopo As PointTopological In _ptList
            Dim _path As New GH_Path(_PFValues.BranchCount)
            For Each _lineTopo As PLineTopological In _ptTopo.PLines
                _PFValues.Add(_lineTopo.Index, _path)
            Next
        Next

        DA.SetDataList(0, _PValues)
        DA.SetDataTree(1, _FPValues)
        DA.SetDataTree(2, _PFValues)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            ' return Resources.IconForThisComponent;
            Return My.Resources.TopologyPolyPoint
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{df9c3597-cbc2-4a05-b6e5-22eac1049469}")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property

End Class