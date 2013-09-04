Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry

Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types

Public Class TopologyLine
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the PolygonEdgeTopology class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Line Topology", "Line Topo", _
     "Analyses the topology of a network consisting of lines", _
     "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddLineParameter("List of lines", "L", "Line network", GH_ParamAccess.list)
        pManager.AddNumberParameter("Tolerance", "T", "Tolerance value", GH_ParamAccess.item, 0.001)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddPointParameter("List of points", "P", "Ordered list of unique points", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Line-Point structure", "LP", "For each line list both end points indices", GH_ParamAccess.tree)
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
        Dim _L As New List(Of Line)
        Dim _T As Double = 0

        '2. Retrieve input data.
        If (Not DA.GetDataList(0, _L)) Then Return
        If (Not DA.GetData(1, _T)) Then Return

        '3. Abort on invalid inputs.
        If (Not _L.Count > 0) Then Return
        If (Not _T > 0) Then Return

        '4. Do something useful.

        '4.1. get topology
        Dim _ptList As List(Of PointTopological) = getPointTopo(_L, _T)
        Dim _lineList As List(Of LineTopological) = getLineTopo(_L, _ptList, _T)
        Call setPointLineTopo(_lineList, _ptList)

        ' 4.2: return results
        Dim _PValues As New List(Of Point3d)
        For Each _ptTopo As PointTopological In _ptList
            _PValues.Add(_ptTopo.Point)
        Next

        Dim _LPValues As New Grasshopper.DataTree(Of Int32)
        For Each _lineTopo As LineTopological In _lineList
            Dim _path As New GH_Path(_LPValues.BranchCount)
            _LPValues.Add(_lineTopo.Startindex, _path)
            _LPValues.Add(_lineTopo.Endindex, _path)
        Next

        Dim _PLValues As New Grasshopper.DataTree(Of Line)
        For Each _ptTopo As PointTopological In _ptList
            Dim _path As New GH_Path(_PLValues.BranchCount)
            For Each _lineTopo As LineTopological In _ptTopo.Lines
                _PLValues.Add(_L.Item(_lineTopo.Index), _path)
            Next
        Next

        Dim _PPValues As New Grasshopper.DataTree(Of Int32)
        For Each _ptTopo As PointTopological In _ptList
            Dim _path As New GH_Path(_PPValues.BranchCount)
            For Each _lineTopo As LineTopological In _ptTopo.Lines
                If _ptTopo.Index = _lineTopo.Startindex Then
                    _PPValues.Add(_lineTopo.Endindex, _path)
                ElseIf _ptTopo.Index = _lineTopo.Endindex Then
                    _PPValues.Add(_lineTopo.Startindex, _path)
                End If
            Next
        Next


        DA.SetDataList(0, _PValues)
        DA.SetDataTree(1, _LPValues)
        DA.SetDataTree(2, _PPValues)
        DA.SetDataTree(3, _PLValues)


    End Sub


    Private Function containsValue(ByVal _edges As Dictionary(Of String, Line), ByVal _check As Line, ByVal _T As Double) As Boolean


        For Each _edge As Line In _edges.Values
            'If _edge.GetHashCode.DistanceTo(_check) < _T Then
            'consider it the same point
            'Return True
            'End If
        Next

        Return False

    End Function

    Private Function containsPoint(ByVal _points As List(Of PointTopological), ByVal _check As Point3d, ByVal _T As Double) As Boolean

        For Each _item As PointTopological In _points
            If _item.Point.DistanceTo(_check) < _T Then
                'consider it the same point
                Return True
            End If
        Next

        Return False

    End Function

    Private Sub setPointLineTopo(ByVal _lineList As List(Of LineTopological), ByVal _pointList As List(Of PointTopological))

        For Each _pt As PointTopological In _pointList

            Dim _lList As New List(Of LineTopological)

            For Each _l As LineTopological In _lineList

                If _pt.Index = _l.Startindex Then
                    _lList.Add(_l)
                ElseIf _pt.Index = _l.Endindex Then
                    _lList.Add(_l)
                End If

            Next

            _pt.Lines = _lList

        Next

    End Sub

    Private Function getLineTopo(ByVal L As List(Of Line), ByVal _ptDict As List(Of PointTopological), ByVal _T As Double) As List(Of LineTopological)

        Dim _lDict As New List(Of LineTopological)

        Dim _count As Int32 = 0
        For Each _line As Line In L

            Dim _Lkey As String = "L" & _count

            Dim _points As Point3d() = New Point3d(1) {}
            _points(0) = _line.PointAt(0)
            _points(1) = _line.PointAt(1)

            Dim _indices As New List(Of String)

            For i As Int32 = 0 To 1

                For Each _item As PointTopological In _ptDict
                    If _item.Point.DistanceTo(_points(i)) < _T Then
                        _indices.Add(_item.Index)
                        Exit For
                    End If
                Next

            Next

            _lDict.Add(New LineTopological(_indices.Item(0), _indices.Item(1), _count))

            _count = _count + 1

        Next

        Return _lDict

    End Function

    Private Function getPointTopo(ByVal L As List(Of Line), ByVal _T As Double) As List(Of PointTopological)

        Dim _ptList As New List(Of PointTopological)

        Dim _count As Int32 = 0
        For Each _line As Line In L

            Dim _points As Point3d() = New Point3d(1) {}
            _points(0) = _line.PointAt(0)
            _points(1) = _line.PointAt(1)

            For i As Int32 = 0 To UBound(_points)

                ' check if point exists in _ptDict already
                If Not containsPoint(_ptList, _points(i), _T) Then
                    Dim _key As String = "P" & _count
                    Dim _value As Point3d = _points(i)
                    _ptList.Add(New PointTopological(_points(i), _count))
                    _count = _count + 1
                End If

            Next

        Next

        Return _ptList

    End Function


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