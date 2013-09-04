Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Types
Imports Grasshopper.Kernel.Data


Public Class TopologyLineFilter
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the TopoplogyLineFilter class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Line Topology Filter", "Line Filter", _
     "Filters a network of lines based on connectivity", _
     "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddPointParameter("List of points", "P", "Ordered list of unique points", GH_ParamAccess.list)
        pManager.AddLineParameter("Point-Line structure", "PL", "Ordered structure listing the lines connected to each point", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Valency filter", "V", "Filter points with the specified number of lines connected to it", GH_ParamAccess.item, 1)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddPointParameter("List of Points", "P", "List of points matching the valency parameter", GH_ParamAccess.list)
        pManager.AddLineParameter("Line Structure", "L", "For each filtered point lists the lines connected to it", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _P As New List(Of GH_Point)
        Dim _PL As New GH_Structure(Of GH_Line)
        Dim _V As Int32 = 0

        '2. Retrieve input data.
        If (Not DA.GetDataList(0, _P)) Then Return
        If (Not DA.GetDataTree(1, _PL)) Then Return
        If (Not DA.GetData(2, _V)) Then Return

        '3. Abort on invalid inputs.
        If (Not _P.Count > 0) Then Return
        If (Not _PL.Branches.Count > 0) Then Return
        If (Not _V > 0) Then Return

        '4. Do something useful.
        '4.1 Filter based on Valency parameter
        Dim _ptList As New List(Of Point3d)
        Dim _lValues As New Grasshopper.DataTree(Of Line)

        For i As Int32 = 0 To _PL.Branches.Count - 1

            Dim _branch As List(Of GH_Line) = _PL.Branches(i)
            If _branch.Count = _V Then
                _ptList.Add(_P.Item(i).Value)

                Dim _path As New GH_Path(i)
                For Each _item As GH_Line In _branch
                    _lValues.Add(_item.Value, _path)
                Next

            End If
        Next

        ' 4.2: return results
        DA.SetDataList(0, _ptList)
        DA.SetDataTree(1, _lValues)

    End Sub

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
            Return New Guid("{251697aa-b454-4f30-8bf7-0f00ea707fc2}")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.primary
        End Get
    End Property
End Class