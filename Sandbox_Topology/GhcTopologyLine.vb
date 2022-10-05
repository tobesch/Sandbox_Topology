Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry

Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types

Public Class GhcTopologyLine
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the PolygonEdgeTopology class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Line Topology",
                   "Line Topo",
                   "Analyses the topology of a network consisting of lines",
                   "Sandbox",
                   "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddLineParameter("List of lines", "L", "Network of lines", GH_ParamAccess.tree)
        pManager.AddNumberParameter("Tolerance", "T", "Tolerance value", GH_ParamAccess.item, 0.001)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddPointParameter("List of points", "P", "Ordered list of unique points", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Line-Point structure", "LP", "For each line lists both end points indices", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Point-Point structure", "PP", "For each point list all point indices connected to it", GH_ParamAccess.tree)
        pManager.AddLineParameter("Point-Line structure", "PL", "For each point list all lines connected to it", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _L As New GH_Structure(Of GH_Line)
        Dim _T As Double = 0

        '2. Retrieve input data.
        If (Not DA.GetDataTree(0, _L)) Then Return
        If (Not DA.GetData(1, _T)) Then Return

        '3. Abort on invalid inputs.
        If (_L.PathCount < 1) Then Return
        If (Not _T > 0) Then Return

        '4. Do something useful.
        Dim _polyTree As New Grasshopper.DataTree(Of Polyline)

        '4.1 get inputs
        Dim count As Int32 = 0
        For Each _branch As List(Of GH_Line) In _L.Branches
            Dim path As New GH_Path(count)
            For Each _goo As GH_Line In _branch
                Dim _line As Line = _goo.Value
                Dim _poly As New Polyline(New Point3d() {_line.From(), _line.To()})
                _polyTree.Add(_poly, path)
            Next
            count += 1
        Next

        Dim _PValues As New Grasshopper.DataTree(Of Point3d)
        Dim _LPValues As New Grasshopper.DataTree(Of Int32)
        Dim _PPValues As New Grasshopper.DataTree(Of Int32)
        Dim _PLValues As New Grasshopper.DataTree(Of Line)

        For i As Int32 = 0 To _polyTree.Branches.Count - 1

            Dim branch As List(Of Polyline) = _polyTree.Branch(i)
            Dim mainpath As New GH_Path(i)

            '4.2 get topology
            Dim _ptList As List(Of PointTopological) = getPointTopo(branch, _T)
            Dim _lineList As List(Of PLineTopological) = getPLineTopo(branch, _ptList, _T)
            Call setPointPLineTopo(_lineList, _ptList)

            ' 4.3 return results
            For Each _ptTopo As PointTopological In _ptList
                _PValues.Add(_ptTopo.Point, mainpath)
            Next

            'For Each _lineTopo As PLineTopological In _lineList
            For j As Int32 = 0 To _lineList.Count - 1
                Dim _lineTopo As PLineTopological = _lineList.Item(j)
                Dim args = New Integer() {i, j}
                Dim _path As New GH_Path(args)
                _LPValues.Add(_lineTopo.PointIndices.Item(0), _path)
                _LPValues.Add(_lineTopo.PointIndices.Item(1), _path)
            Next

            'For Each _ptTopo As PointTopological In _ptList
            For j As Int32 = 0 To _ptList.Count - 1
                Dim _ptTopo As PointTopological = _ptList.Item(j)
                Dim args = New Integer() {i, j}
                Dim _path As New GH_Path(args)
                For Each _lineTopo As PLineTopological In _ptTopo.PLines
                    If _ptTopo.Index = _lineTopo.PointIndices.Item(0) Then
                        _PPValues.Add(_lineTopo.PointIndices.Item(1), _path)
                    ElseIf _ptTopo.Index = _lineTopo.PointIndices.Item(1) Then
                        _PPValues.Add(_lineTopo.PointIndices.Item(0), _path)
                    End If
                Next
            Next

            'For Each _ptTopo As PointTopological In _ptList
            For j As Int32 = 0 To _ptList.Count - 1
                Dim _ptTopo As PointTopological = _ptList.Item(j)
                Dim args = New Integer() {i, j}
                Dim _path As New GH_Path(args)
                For Each _lineTopo As PLineTopological In _ptTopo.PLines
                    _PLValues.Add(_L.Branch(i).Item(_lineTopo.Index), _path)
                Next
            Next

        Next

        DA.SetDataTree(0, _PValues)
        DA.SetDataTree(1, _LPValues)
        DA.SetDataTree(2, _PPValues)
        DA.SetDataTree(3, _PLValues)

    End Sub

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.TopologyLine
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{a09956f1-a616-4896-a242-eab3fc506087}")
        End Get
    End Property
End Class