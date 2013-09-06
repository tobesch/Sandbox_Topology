Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types
Imports Rhino.Geometry


Public Class TopologyBrepVertex
    Inherits GH_Component

    ''' <summary>
    ''' Each implementation of GH_Component must provide a public 
    ''' constructor without any arguments.
    ''' Category represents the Tab in which the component will appear, 
    ''' Subcategory the panel. If you use non-existing tab or panel names, 
    ''' new tabs/panels will automatically be created.
    ''' </summary>
    Public Sub New()
        MyBase.New("Brep Topology Vertex", "Brep Topo Vertex", _
           "Analyses the vertex topology of a Brep", _
           "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddBrepParameter("Brep", "B", "Brep to analyse", GH_ParamAccess.item)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddIntegerParameter("Face-Vertex structure", "FV", "For each face list vertex indices belonging to face", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Vertex-Face structure", "VF", "For each vertex list adjacent face indices", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object can be used to retrieve data from input parameters and 
    ''' to store data in output parameters.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _brep As Brep = Nothing

        '2. Retrieve input data.
        If (Not DA.GetData(0, _brep)) Then Return

        '3. Abort on invalid inputs.
        If (Not _brep.IsValid) Then Return

        '4. Check for non-manifold Breps
        If (Not _brep.IsManifold) Then Return

        '5. Check if the topology is valid
        Dim log As String = String.Empty
        If (Not _brep.IsValidTopology(log)) Then Return

        '6. Now do something productive
        Dim _polyList As New List(Of Polyline)

        For Each _loop As BrepLoop In _brep.Loops
            Dim _poly As Polyline = Nothing
            If Not _loop.To3dCurve.TryGetPolyline(_poly) Then Return
            _polyList.Add(_poly)
        Next

        '4.2. get topology
        Dim _T As Double = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance
        Dim _ptList As New List(Of PointTopological)
        For Each _vertex As BrepVertex In _brep.Vertices
            _ptList.Add(New PointTopological(_vertex.Location, _ptList.Count))
        Next
        'Dim _ptList As List(Of PointTopological) = getPointTopo(_polyList, _T)
        Dim _fList As List(Of PLineTopological) = getPLineTopo(_polyList, _ptList, _T)
        Call setPointPLineTopo(_fList, _ptList)

        ' 4.3: return results
        Dim _FV As New Grasshopper.DataTree(Of Int32)
        For Each _lineTopo As PLineTopological In _fList
            Dim _path As New GH_Path(_FV.BranchCount)
            For Each _index As Int32 In _lineTopo.PointIndices
                _FV.Add(_index, _path)
            Next
        Next

        Dim _VF As New Grasshopper.DataTree(Of Int32)
        For Each _ptTopo As PointTopological In _ptList
            Dim _path As New GH_Path(_VF.BranchCount)
            For Each _lineTopo As PLineTopological In _ptTopo.PLines
                _VF.Add(_lineTopo.Index, _path)
            Next
        Next

        DA.SetDataTree(0, _FV)
        DA.SetDataTree(1, _VF)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.TopologyBrepPoint
            'Return Nothing
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.tertiary
        End Get
    End Property

    ''' <summary>
    ''' Each component must have a unique Guid to identify it. 
    ''' It is vital this Guid doesn't change otherwise old ghx files 
    ''' that use the old ID will partially fail during loading.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{b585dbc3-37eb-4387-b6d2-7bd220dc2470}")
        End Get
    End Property
End Class