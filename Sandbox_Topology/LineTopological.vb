Imports Rhino.Geometry

Public Class LineTopological

    Private _startIndex As Int32
    Private _endIndex As Int32
    Private _i As Int32     'internal indexing of the lines

    Public Sub New(ByVal P1 As Int32, ByVal P2 As Int32, ByVal I As Int32)

        _startIndex = P1
        _endIndex = P2
        _i = I

    End Sub

    '##### PROPERTIES #####

    Public ReadOnly Property Index As Int32
        Get
            Return _i
        End Get
    End Property

    Public ReadOnly Property Startindex As Int32
        Get
            Return _startIndex
        End Get
    End Property

    Public ReadOnly Property Endindex As Int32
        Get
            Return _endIndex
        End Get
    End Property

End Class
