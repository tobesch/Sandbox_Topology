Imports Grasshopper.Kernel

Public Class Sandbox_Topology_Info
    Inherits GH_AssemblyInfo

    Public Overrides ReadOnly Property AssemblyName() As String
        Get
            Return "Sandbox Topology"
        End Get
    End Property

    Public Overrides ReadOnly Property License As Grasshopper.Kernel.GH_LibraryLicense
        Get
            Return GH_LibraryLicense.opensource
        End Get
    End Property
    Public Overrides ReadOnly Property Description As String
        Get
            Return "Tools for experiments in computational architecture"
        End Get
    End Property
    Public Overrides ReadOnly Property Version As String
        Get
            Return "1.0.0.0"
        End Get
    End Property
    Public Overrides ReadOnly Property Name As String
        Get
            Return "Sandbox Topology"
        End Get
    End Property
    Public Overrides ReadOnly Property AuthorContact As String
        Get
            Return "tobias.schwinn@gmail.com"
        End Get
    End Property
    Public Overrides ReadOnly Property AuthorName As String
        Get
            Return "Tobias Schwinn"
        End Get
    End Property

    'Override here any more methods you see fit.
    'Start typing Public Overrides..., select a property and push Enter.

End Class