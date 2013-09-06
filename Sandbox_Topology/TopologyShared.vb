Imports Rhino.Geometry

Module TopologyShared

    Private Function containsPoint(ByVal _points As List(Of PointTopological), ByVal _check As Point3d, ByVal _T As Double) As Boolean

        For Each _item As PointTopological In _points
            If _item.Point.DistanceTo(_check) < _T Then
                'consider it the same point
                Return True
            End If
        Next

        Return False

    End Function

    Public Function getPointTopo(ByVal P As List(Of Polyline), ByVal _T As Double) As List(Of PointTopological)

        Dim _ptList As New List(Of PointTopological)

        Dim _count As Int32 = 0
        For Each _poly As Polyline In P

            Dim _points As Point3d() = _poly.ToArray

            For i As Int32 = 0 To UBound(_points)

                ' check if point exists in _ptList already
                If Not containsPoint(_ptList, _points(i), _T) Then
                    _ptList.Add(New PointTopological(_points(i), _count))
                    _count = _count + 1
                End If

            Next

        Next

        Return _ptList

    End Function

    Public Function getPLineTopo(ByVal P As List(Of Polyline), ByVal _ptDict As List(Of PointTopological), ByVal _T As Double) As List(Of PLineTopological)

        Dim _lDict As New List(Of PLineTopological)

        Dim _count As Int32 = 0
        For Each _poly As Polyline In P

            Dim _points As Point3d() = _poly.ToArray

            Dim _indices As New List(Of Int32)

            For i As Int32 = 0 To UBound(_points)

                For Each _item As PointTopological In _ptDict
                    If _item.Point.DistanceTo(_points(i)) < _T Then
                        _indices.Add(_item.Index)
                        Exit For
                    End If
                Next

            Next

            _lDict.Add(New PLineTopological(_indices, _count))

            _count = _count + 1

        Next

        Return _lDict

    End Function

    Public Sub setPointPLineTopo(ByVal _lineList As List(Of PLineTopological), ByVal _pointList As List(Of PointTopological))

        For Each _pt As PointTopological In _pointList

            Dim _lList As New List(Of PLineTopological)

            For Each _l As PLineTopological In _lineList

                For Each _index As Int32 In _l.PointIndices
                    If _index = _pt.Index Then
                        _lList.Add(_l)
                        Exit For
                    End If
                Next

            Next

            _pt.PLines = _lList

        Next

    End Sub
End Module
