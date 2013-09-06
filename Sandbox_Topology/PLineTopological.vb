Public Class PLineTopological

    Private _v As List(Of Int32)
    Private _i As Int32     'internal indexing of the lines

    Public Sub New(ByVal V As List(Of Int32), ByVal I As Int32)

        _v = V
        _i = I

    End Sub

    '##### PROPERTIES #####

    Public ReadOnly Property Index As Int32
        Get
            Return _i
        End Get
    End Property

    Public ReadOnly Property PointIndices As List(Of Int32)
        Get
            Return _v
        End Get
    End Property

End Class
