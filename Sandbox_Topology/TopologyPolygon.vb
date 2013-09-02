Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry

Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types


Public Class TopologyPolygon
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the PolygonTopology class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Polygon Topology", "Topology", _
     "Analyses the topology of a curve network consisting of closed polylines", _
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
        pManager.AddIntegerParameter("Face-Point collection", "FP", "Point indeces grouped by face index", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Point-Face collection", "PF", "Face indeces grouped by point index", GH_ParamAccess.tree)
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
        Dim _ptDict As Dictionary(Of String, Point3d) = getPointDict(_polyList, _T)
        Dim _fDict As Dictionary(Of String, List(Of String)) = getFaceDict(_polyList, _ptDict, _T)
        Dim _ptFaceDict As Dictionary(Of String, List(Of String)) = getPointFaceDict(_fDict, _ptDict)
        ''Dim _ptFaceList As List(Of String) = getPointFaceDict(_fDict, _ptDict)

        ' 4.3: return results
        Dim _PValues As New List(Of Point3d)
        For Each _pair As KeyValuePair(Of String, Point3d) In _ptDict
            _PValues.Add(_pair.Value)
        Next

        Dim _FPValues As New Grasshopper.DataTree(Of Int32)
        For Each _ptList As List(Of String) In _fDict.Values
            Dim _path As New GH_Path(_FPValues.BranchCount)
            For Each _item As String In _ptList
                _FPValues.Add(_item.Substring(1), _path)
            Next
        Next

        Dim _PFValues As New Grasshopper.DataTree(Of Int32)
        For Each _fList As List(Of String) In _ptFaceDict.Values
            Dim _path As New GH_Path(_PFValues.BranchCount)
            For Each _item As String In _fList
                _PFValues.Add(_item.Substring(1), _path)
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
            Return My.Resources.Sandbox_Topology
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

    Private Function getPointFaceDict(ByVal _fDict As Dictionary(Of String, List(Of String)), ByVal _ptDict As Dictionary(Of String, Point3d)) As Dictionary(Of String, List(Of String))

        Dim _ptFaceDict As New Dictionary(Of String, List(Of String))

        For Each _ptID As String In _ptDict.Keys

            Dim _fList As New List(Of String)

            'For i As Int32 = 0 To _fDict.Values.Count - 1
            'For Each _values As List(Of String) In _fDict.Values
            For Each _key As String In _fDict.Keys
                Dim _values As List(Of String) = _fDict.Item(_key)

                For j As Int32 = 0 To _values.Count - 1
                    If _ptID = _values.Item(j) Then
                        _fList.Add(_key)
                        '_string = _string & (_fDict.Keys(i)) & ", "
                    End If
                Next

            Next

            _ptFaceDict.Add(_ptID, _fList)

        Next

        Return _ptFaceDict

    End Function

    Private Function getFaceDict(ByVal _polyList As List(Of Polyline), ByVal _ptDict As Dictionary(Of String, Point3d), ByVal _T As Double) As Dictionary(Of String, List(Of String))

        Dim _fDict As New Dictionary(Of String, List(Of String))

        Dim _count As Int32 = 0
        For Each _poly As Polyline In _polyList

            Dim _Fkey As String = "F" & _count

            Dim _points As Point3d()
            _points = _poly.ToArray

            Dim _value As New List(Of String)

            For i As Int32 = 0 To UBound(_points) - 1

                For Each _key As String In _ptDict.Keys
                    If _ptDict.Item(_key).DistanceTo(_points(i)) < _T Then
                        _value.Add(_key)
                        Exit For
                    End If
                Next

            Next

            _fDict.Add(_Fkey, _value)

            _count = _count + 1

        Next

        Return _fDict

    End Function

    Private Function containsValue(ByVal _points As Dictionary(Of String, Point3d), ByVal _check As Point3d, ByVal _T As Double) As Boolean


        For Each _pt As Point3d In _points.Values
            If _pt.DistanceTo(_check) < _T Then
                'consider it the same point
                Return True
            End If
        Next

        Return False

    End Function

    Private Function getPointDict(ByVal _polyList As List(Of Polyline), ByVal _T As Double) As Dictionary(Of String, Point3d)

        Dim _ptDict As New Dictionary(Of String, Point3d)

        Dim _count As Int32 = 0
        For Each _poly As Polyline In _polyList

            Dim _points As Point3d()
            _points = _poly.ToArray

            For i As Int32 = 0 To UBound(_points)

                ' check if point exists in _ptDict already
                If Not containsValue(_ptDict, _points(i), _T) Then
                    Dim _key As String = "P" & _count
                    Dim _value As Point3d = _points(i)
                    _ptDict.Add(_key, _value)
                    _count = _count + 1
                End If

            Next

        Next

        Return _ptDict

    End Function
End Class