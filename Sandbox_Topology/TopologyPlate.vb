Imports System.Collections.Generic

Imports Grasshopper.Kernel
Imports Grasshopper.Kernel.Data
Imports Grasshopper.Kernel.Types
Imports Rhino.Geometry


Public Class TopologyPlate
    Inherits GH_Component

    ''' <summary>
    ''' Each implementation of GH_Component must provide a public 
    ''' constructor without any arguments.
    ''' Category represents the Tab in which the component will appear, 
    ''' Subcategory the panel. If you use non-existing tab or panel names, 
    ''' new tabs/panels will automatically be created.
    ''' </summary>
    Public Sub New()
        MyBase.New("Plate Topology", "Topo", _
           "Analyses the topology of a Brep", _
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
        pManager.AddIntegerParameter("Faces", "F", "List of adjacent face indices by edge", GH_ParamAccess.tree)
        pManager.AddIntegerParameter("Edges", "E", "List of adjacent edge indices by face", GH_ParamAccess.tree)
    End Sub

    ''' <summary>
    ''' This is the method that actually does the work.
    ''' </summary>
    ''' <param name="DA">The DA object can be used to retrieve data from input parameters and 
    ''' to store data in output parameters.</param>
    Protected Overrides Sub SolveInstance(ByVal DA As IGH_DataAccess)

        '1. Declare placeholder variables and assign initial invalid data.
        '   This way, if the input parameters fail to supply valid data, we know when to abort.
        Dim brep As Rhino.Geometry.Brep = Nothing

        '2. Retrieve input data.
        If (Not DA.GetData(0, brep)) Then Return

        '3. Abort on invalid inputs.
        If (Not brep.IsValid) Then Return

        '4. Check for non-manifold Breps
        If (Not brep.IsManifold) Then Return

        '5. Check if the topology is valid
        Dim log As String = String.Empty
        If (Not brep.IsValidTopology(log)) Then Return

        '6. Now do something productive
        Dim face As BrepFace = Nothing
        Dim e_tree As New Grasshopper.DataTree(Of Int32)

        For i As Int32 = 0 To brep.Faces.Count - 1
            face = brep.Faces.Item(i)
            Dim edges As Int32() = face.AdjacentEdges()

            Dim e_path As New GH_Path(i)
            For j As Int32 = 0 To UBound(edges)
                e_tree.Add(edges(j), e_path)
            Next
        Next

        Dim edge As BrepEdge = Nothing
        Dim f_tree As New Grasshopper.DataTree(Of Int32)

        For i As Int32 = 0 To brep.Edges.Count - 1
            edge = brep.Edges.Item(i)
            Dim faces As Int32() = edge.AdjacentFaces()

            Dim path As New GH_Path(i)
            For j As Int32 = 0 To UBound(faces)
                f_tree.Add(faces(j), path)
            Next
        Next

        DA.SetDataTree(0, f_tree)
        DA.SetDataTree(1, e_tree)

    End Sub

    ''' <summary>
    ''' Provides an Icon for every component that will be visible in the User Interface.
    ''' Icons need to be 24x24 pixels.
    ''' </summary>
    Protected Overrides ReadOnly Property Icon() As System.Drawing.Bitmap
        Get
            'You can add image files to your project resources and access them like this:
            Return My.Resources.Echinoid_1_Topology
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