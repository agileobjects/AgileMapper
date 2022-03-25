Public Class PublicNamedIndex(Of T)

    Private _valueToReturn As T

    Public WriteOnly Property ValueToReturn As T
        Set
            _valueToReturn = Value
        End Set
    End Property

    Public ReadOnly Property Value(
        Optional indexOne As Integer = 1,
        Optional indexTwo As Integer = 2) As T
        Get
            Return _valueToReturn
        End Get
    End Property

End Class