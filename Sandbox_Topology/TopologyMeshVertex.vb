Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types
Imports Rhino.Geometry


Public Class TopologyMeshVertex
    Inherits GH_Component

    ''' <summary>
    ''' Each implementation of GH_Component must provide a public 
    ''' constructor without any arguments.
    ''' Category represents the Tab in which the component will appear, 
    ''' Subcategory the panel. If you use non-existing tab or panel names, 
    ''' new tabs/panels will automatically be created.
    ''' </summary>
    Public Sub New()
        MyBase.New("Mesh Topology Vertex", "Mesh Topo Vertex", _
           "Analyses the vertex topology of a Mesh", _
           "Sandbox", "Topology")
    End Sub

    ''' <summary>
    ''' Registers all the input parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterInputParams(ByVal pManager As GH_Component.GH_InputParamManager)
        pManager.AddMeshParameter("Mesh", "M", "Mesh to analyse", GH_ParamAccess.item)
    End Sub

    ''' <summary>
    ''' Registers all the output parameters for this component.
    ''' </summary>
    Protected Overrides Sub RegisterOutputParams(ByVal pManager As GH_Component.GH_OutputParamManager)
        pManager.AddPointParameter("List of mesh vertices", "V", "Ordered list of mesh vertices", GH_ParamAccess.list)
        pManager.AddIntegerParameter("Vertex-Vertex structure", "VV", "For each vertex the list of adjacent vertex indices", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Vertex-Face structure", "VF", "For each vertex the list of adjacent face indices", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object can be used to retrieve data from input parameters and 
    ''' to store data in output parameters.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim _mesh As Mesh = Nothing

        '2. Retrieve input data.
        If (Not DA.GetData(0, _mesh)) Then Return

        '3. Abort on invalid inputs.
        If (Not _mesh.IsValid) Then Return

        '4. Check for non-manifold Mesh
        Dim _isOriented As Boolean
        Dim _hasBoundary As Boolean
        If (Not _mesh.IsManifold(True, _isOriented, _hasBoundary)) Then Return

        '5. Check if the topology is valid
        Dim log As String = String.Empty
        If (Not _mesh.IsValidWithLog(log)) Then Return

        '6. Now do something productive
        Dim _VVValues As New Grasshopper.DataTree(Of Int32)
        Dim _VFValues As New Grasshopper.DataTree(Of Int32)

        Dim _vertexList As Collections.MeshVertexList = _mesh.Vertices()
        For _vIndex As Int32 = 0 To _vertexList.Count - 1
            Dim _indices As Int32() = _vertexList.GetConnectedVertices(_vIndex)
            Dim vv_path As New GH_Path(_VVValues.BranchCount)
            For Each _vertex As Int32 In _indices
                If _vertex <> _vIndex Then _VVValues.Add(_vertex, vv_path)
            Next
        Next

        For _vIndex As Int32 = 0 To _vertexList.Count - 1
            Dim _faces As Int32() = _vertexList.GetVertexFaces(_vIndex)
            Dim vf_path As New GH_Path(_VFValues.BranchCount)
            _VFValues.AddRange(_faces, vf_path)
        Next

        Dim _VList As New List(Of Point3d)
        Dim _vertices As Point3d() = _vertexList.ToPoint3dArray
        _VList.AddRange(_vertices)

        DA.SetDataList(0, _VList)
        DA.SetDataTree(1, _VVValues)
        DA.SetDataTree(2, _VFValues)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.TopologyMeshPoint
        End Get
    End Property

    Public Overrides ReadOnly Property Exposure As Grasshopper.Kernel.GH_Exposure
        Get
            Return GH_Exposure.quarternary
        End Get
    End Property

    ''' <summary>
    ''' Each component must have a unique Guid to identify it. 
    ''' It is vital this Guid doesn't change otherwise old ghx files 
    ''' that use the old ID will partially fail during loading.
    ''' </summary>
    Public Overrides ReadOnly Property ComponentGuid() As Guid
        Get
            Return New Guid("{412f2c84-4675-4282-8b55-8a8c8d325e6d}")
        End Get
    End Property
End Class