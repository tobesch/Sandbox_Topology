Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Types
Imports Rhino.Geometry


Public Class NCSettings
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the NCSettings class.
    ''' </summary>
    Public Sub New()
        MyBase.New("NC Settings", "NC Settings", _
           "Provides the basic settings for NC fabrication", _
           "Sandbox", "CNC")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddIntegerParameter("Tool", "T", "Tool number", GH_ParamAccess.item, 1)
        pManager.AddIntegerParameter("Speed", "S", "Spindle speed", GH_ParamAccess.item, 1000)
        pManager.AddIntegerParameter("Feed", "F", "G1 speed", GH_ParamAccess.item, 100)
        pManager.AddTextParameter("Base", "B", "Base Origin in the Robot's World", GH_ParamAccess.item)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddGenericParameter("Settings", "S", "NC-code generation settings", GH_ParamAccess.list)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)
        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _T As Int32 = Int32.MinValue
        Dim _S As Int32 = Int32.MinValue
        Dim _F As Int32 = Int32.MinValue
        Dim _B As String = String.Empty

        '2. Retrieve input data.
        If (Not DA.GetData(0, _T)) Then Return
        If (Not DA.GetData(1, _S)) Then Return
        If (Not DA.GetData(2, _F)) Then Return
        If (Not DA.GetData(3, _B)) Then Return

        '3. Abort on invalid inputs.
        '

        '4. Do something useful.
        Dim _ncSettings As New List(Of String)
        'Dim _ncSettings As List(Of NCSettingType) = Nothing
        'Dim first As New NCSettingType("P9999")

        _ncSettings.Add("P9999")
        _ncSettings.Add("G40")
        _ncSettings.Add("G90G94")
        _ncSettings.Add("T" & _T.ToString & "M6")
        _ncSettings.Add("G90")
        _ncSettings.Add("S" & _S.ToString & "M3")
        _ncSettings.Add(_F.ToString)
        _ncSettings.Add(_B)
        
        '_ncSettings.Add(New NCSettingType("P9999"))
        '_ncSettings.Add(New NCSettingType("G40"))
        '_ncSettings.Add(New NCSettingType("G90G94"))
        '_ncSettings.Add(New NCSettingType("T" & _T.ToString & "M6"))
        '_ncSettings.Add(New NCSettingType("G90"))
        '_ncSettings.Add(New NCSettingType("S" & _S.ToString & "M3"))
        '_ncSettings.Add(New NCSettingType(_F.ToString))

        DA.SetData(0, _ncSettings)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.Echinoid_3_NCSettings
            'Return Nothing
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{e9fbf3b2-2455-4066-a8dc-8b5c76764488}")
        End Get
    End Property
End Class