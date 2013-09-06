Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types
Imports Rhino.Geometry


Public Class TopologyBrepEdge
    Inherits GH_Component

    ''' <summary>
    ''' Each implementation of GH_Component must provide a public 
    ''' constructor without any arguments.
    ''' Category represents the Tab in which the component will appear, 
    ''' Subcategory the panel. If you use non-existing tab or panel names, 
    ''' new tabs/panels will automatically be created.
    ''' </summary>
    Public Sub New()
        MyBase.New("Brep Topology Edge", "Brep Topo Edge", _
           "Analyses the edge topology of a Brep", _
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
        pManager.AddIntegerParameter("Face-Edge structure", "FE", "For each face list edge indices belonging to face", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Edge-Face structure", "EF", "For each edge lists adjacent face indices", GH_ParamAccess.tree)
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
        Dim e_tree As New Grasshopper.DataTree(Of Int32)

        For Each _face As BrepFace In _brep.Faces

            Dim _edges As Int32() = _face.AdjacentEdges()
            Dim e_path As New GH_Path(e_tree.BranchCount)
            e_tree.AddRange(_edges, e_path)

        Next


        Dim f_tree As New Grasshopper.DataTree(Of Int32)

        For Each _edge As BrepEdge In _brep.Edges

            Dim _faces As Int32() = _edge.AdjacentFaces()
            Dim f_path As New GH_Path(f_tree.BranchCount)
            f_tree.AddRange(_faces, f_path)
        Next

        DA.SetDataTree(0, e_tree)
        DA.SetDataTree(1, f_tree)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.TopologyBrep
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
            Return New Guid("{9a99f8d8-9f33-4e83-9d8e-562820438a28}")
        End Get
    End Property
End Class