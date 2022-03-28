Public Class PublicNamedIndex(Of T1, T2)

    Private _value1ToReturn As T1

    Public WriteOnly Property Value1ToReturn As T1
        Set
            _value1ToReturn = Value
        End Set
    End Property

    Public ReadOnly Property Value1(
        Optional indexOne As Integer = 1,
        Optional indexTwo As Integer = 2) As T1
        Get
            Return _value1ToReturn
        End Get
    End Property

    Public Property Value2SetValue As T2

    Public WriteOnly Property Value2(
        indexOne As Integer,
        indexTwo As Integer) As T2
        Set
            Value2SetValue = Value
        End Set
    End Property

    Public Property Value3SetValue As T2

    Public WriteOnly Property Value3(
         Optional indexOne As Integer = 1,
         Optional indexTwo As Nullable(Of Integer) = Nothing) As T2
        Set
            Value3SetValue = Value
        End Set
    End Property

End Class