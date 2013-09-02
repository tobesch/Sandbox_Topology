Public Class LineTopological

    Private _startIndex As Int32
    Private _endIndex As Int32

    Public Sub New(ByVal P1 As Int32, ByVal P2 As Int32)

        _startIndex = P1
        _endIndex = P2

    End Sub

    '##### PROPERTIES #####

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
