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
        pManager.AddIntegerParameter("Line-point structure", "LP", "For each line list both points indices connected to it", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Point-Line structure", "PL", "For each point list all line indices connected to it", GH_ParamAccess.tree)
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
        Dim _lineList As List(Of Line) = _L

        '4.1. get topology
        Dim _ptDict As Dictionary(Of String, Point3d) = getPointDict(_lineList, _T)
        Dim _lineDict As Dictionary(Of String, List(Of String)) = getLineDict(_lineList, _ptDict, _T)
        Dim _ptLineDict As Dictionary(Of String, List(Of String)) = getPointLineDict(_lineDict, _ptDict)

        ' 4.2: return results
        Dim _PValues As New List(Of Point3d)
        For Each _pair As KeyValuePair(Of String, Point3d) In _ptDict
            _PValues.Add(_pair.Value)
        Next

        Dim _LPValues As New Grasshopper.DataTree(Of Int32)
        For Each _ptList As List(Of String) In _lineDict.Values
            Dim _path As New GH_Path(_LPValues.BranchCount)
            For Each _item As String In _ptList
                _LPValues.Add(_item.Substring(1), _path)
            Next
        Next

        Dim _PLValues As New Grasshopper.DataTree(Of Int32)
        For Each _fList As List(Of String) In _ptLineDict.Values
            Dim _path As New GH_Path(_PLValues.BranchCount)
            For Each _item As String In _fList
                _PLValues.Add(_item.Substring(1), _path)
            Next
        Next


        DA.SetDataList(0, _PValues)
        DA.SetDataTree(1, _LPValues)
        DA.SetDataTree(2, _PLValues)

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

    Private Function containsPoint(ByVal _points As Dictionary(Of String, Point3d), ByVal _check As Point3d, ByVal _T As Double) As Boolean

        For Each _pt As Point3d In _points.Values
            If _pt.DistanceTo(_check) < _T Then
                'consider it the same point
                Return True
            End If
        Next

        Return False

    End Function

    Private Function getPointLineDict(ByVal _lineDict As Dictionary(Of String, List(Of String)), ByVal _ptDict As Dictionary(Of String, Point3d)) As Dictionary(Of String, List(Of String))

        Dim _ptLineDict As New Dictionary(Of String, List(Of String))

        For Each _ptID As String In _ptDict.Keys

            Dim _lList As New List(Of String)

            'For i As Int32 = 0 To _fDict.Values.Count - 1
            'For Each _values As List(Of String) In _fDict.Values
            For Each _key As String In _lineDict.Keys
                Dim _values As List(Of String) = _lineDict.Item(_key)

                For j As Int32 = 0 To _values.Count - 1
                    If _ptID = _values.Item(j) Then
                        _lList.Add(_key)
                        '_string = _string & (_fDict.Keys(i)) & ", "
                    End If
                Next

            Next

            _ptLineDict.Add(_ptID, _lList)

        Next

        Return _ptLineDict

    End Function

    Private Function getLineDict(ByVal L As List(Of Line), ByVal _ptDict As Dictionary(Of String, Point3d), ByVal _T As Double) As Dictionary(Of String, List(Of String))

        Dim _lDict As New Dictionary(Of String, List(Of String))

        Dim _count As Int32 = 0
        For Each _line As Line In L

            Dim _Lkey As String = "L" & _count

            Dim _points As Point3d() = New Point3d(1) {}
            _points(0) = _line.PointAt(0)
            _points(1) = _line.PointAt(1)

            Dim _values As New List(Of String)

            For i As Int32 = 0 To UBound(_points)

                For Each _key As String In _ptDict.Keys
                    If _ptDict.Item(_key).DistanceTo(_points(i)) < _T Then
                        _values.Add(_key)
                        Exit For
                    End If
                Next

            Next

            _lDict.Add(_Lkey, _values)

            _count = _count + 1

        Next

        Return _lDict

    End Function

    Private Function getPointDict(ByVal L As List(Of Line), ByVal _T As Double) As Dictionary(Of String, Point3d)

        Dim _ptDict As New Dictionary(Of String, Point3d)

        Dim _count As Int32 = 0
        For Each _line As Line In L

            Dim _points As Point3d() = New Point3d(1) {}
            _points(0) = _line.PointAt(0)
            _points(1) = _line.PointAt(1)

            For i As Int32 = 0 To UBound(_points)

                ' check if point exists in _ptDict already
                If Not containsPoint(_ptDict, _points(i), _T) Then
                    Dim _key As String = "P" & _count
                    Dim _value As Point3d = _points(i)
                    _ptDict.Add(_key, _value)
                    _count = _count + 1
                End If

            Next

        Next

        Return _ptDict

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
            ' return Resources.IconForThisComponent;
            Return Nothing
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