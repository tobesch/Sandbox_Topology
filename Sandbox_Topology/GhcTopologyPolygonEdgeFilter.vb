Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Types
Imports Grasshopper.Kernel.Data
Imports System.IO


Public Class GhcTopologyPolygonEdgeFilter
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the NakedPolygonVertices class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Polygon Topology Edge Filter", "Poly Topo Edge Filter",
           "Filter the edges in a polygon network based on their valency",
           "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddLineParameter("Edge list", "E", "Ordered list of edges", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Edge-Loop structure", "EL", "Ordered structure listing the polylines adjacent to each edge", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Valency filter", "V", "Filter edges with the specified number of adjacent polylines", GH_ParamAccess.item, 1)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddTextParameter("List of edge IDs", "I", "List of edge indices matching the valency criteria", GH_ParamAccess.tree)
        pManager.AddLineParameter("List of edges", "E", "List of edges matching the valency criteria", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _E As New GH_Structure(Of GH_Line)
        Dim _EL As GH_Structure(Of GH_Integer) = Nothing
        Dim _V As Int32 = 0

        '2. Retrieve input data.
        If (Not DA.GetDataTree(0, _E)) Then Return
        If (Not DA.GetDataTree(1, _EL)) Then Return
        If (Not DA.GetData(2, _V)) Then Return

        '3. Abort on invalid inputs.
        '3.1. get the number of branches in the trees
        If (Not _E.PathCount > 0) Then Return
        If (Not _EL.PathCount > 0) Then Return
        If (Not _V > 0) Then Return

        '4. Do something useful.

        Dim _idTree As New Grasshopper.DataTree(Of Int32)
        Dim _edgeTree As New Grasshopper.DataTree(Of Line)

        For i As Int32 = 0 To _E.Branches.Count - 1

            Dim branch As List(Of GH_Line) = _E.Branch(i)
            Dim mainpath As New GH_Path(i)

            For j As Int32 = 0 To branch.Count - 1
                Dim args As Integer() = New Integer() {i, j}
                Dim path As New GH_Path(args)
                If _EL.Branch(path).Count = _V Then
                    _idTree.Add(j, mainpath)
                    _edgeTree.Add(branch.Item(j).Value, mainpath)
                End If
            Next

            'Dim _idList As New List(Of Int32)

            'For Each edgeList As List(Of GH_Integer) In _EL.Branches
            '    If branch.Count = _V Then
            '        _idList.Add(_path.ToString.Trim(New Char() {"{", "}"}))
            '    End If
            'Next

            'For Each edgeList As List(Of GH_Integer) In _EL.Branches
            '    If branch.Count = _V Then
            '        _idList.Add(_path.ToString.Trim(New Char() {"{", "}"}))
            '    End If
            'Next

            'For Each _id As Int32 In _idList

            '    _edgeTree.Add(_E.Item(_id).Value)

            'Next
        Next


        DA.SetDataTree(0, _idTree)
        DA.SetDataTree(1, _edgeTree)


    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.TopologyPolyEdgeFilter
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{d9534bba-01a3-48c2-b989-4884c03421d9}")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.secondary
        End Get
    End Property
End Class