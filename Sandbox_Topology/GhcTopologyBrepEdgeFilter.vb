﻿Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Rhino.Geometry
Imports Grasshopper.Kernel.Types
Imports Grasshopper.Kernel.Data


Public Class GhcTopologyBrepEdgeFilter
    Inherits GH_Component
    ''' <summary>
    ''' Initializes a new instance of the NakedPolygonVertices class.
    ''' </summary>
    Public Sub New()
        MyBase.New("Brep Topology Edge Filter", "Brep Topo Edge Filter",
           "Filter the edges of a brep based on their valency",
           "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddLineParameter("Edge list", "E", "Ordered list of edges", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Edge-Face structure", "EF", "For each edge the list of adjacent face indices", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Valency filter", "V", "Filter edges with the specified number of adjacent faces", GH_ParamAccess.item, 1)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddTextParameter("List of edge IDs", "I", "List of edge indices matching the valency criteria", GH_ParamAccess.list)
        pManager.AddLineParameter("List of edges", "E", "List of edges matching the valency criteria", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _E As New List(Of GH_Line)
        Dim _EF As GH_Structure(Of GH_Integer) = Nothing
        Dim _C As Int32 = 0

        '2. Retrieve input data.
        If (Not DA.GetDataList(0, _E)) Then Return
        If (Not DA.GetDataTree(1, _EF)) Then Return
        If (Not DA.GetData(2, _C)) Then Return

        '3. Abort on invalid inputs.
        '3.1. get the number of branches in the trees
        If (Not _E.Count > 0) Then Return
        If (Not _EF.PathCount > 0) Then Return
        If (Not _C > 0) Then Return

        '4. Do something useful.

        Dim _idList As New List(Of Int32)
        For i As Int32 = 0 To _EF.Branches.Count - 1
            Dim _branch As List(Of GH_Integer) = _EF.Branches(i)
            If _branch.Count = _C Then
                _idList.Add(i)
            End If
        Next

        Dim _eList As New List(Of Line)
        For Each _id As Int32 In _idList
            _eList.Add(_E.Item(_id).Value)
        Next

        DA.SetDataList(0, _idList)
        DA.SetDataList(1, _eList)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.TopologyBrepEdgeFilter
        End Get
    End Property

    ''' <summary>
    ''' Gets the unique ID for this component. Do not change this ID after release.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{7ca5a286-2357-4e49-bc8b-38682c40b558}")
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property
End Class