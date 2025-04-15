using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Sandbox
{

    /// <summary>
    /// 
    /// </summary>
    public class TopologyMeshEdge : GH_Component
    {

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TopologyMeshEdge() : base("Mesh Topology Edge", "Mesh Topo Edge", "Analyses the edge topology of a Mesh", "Sandbox", "Topology")

        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to analyse", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("List of edges", "E", "Ordered list of mesh edges", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Face-Face structure", "FF", "For each face the list of adjacent face indices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Edge-Face structure", "EF", "For each edge the list of adjacent face indices", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Declare placeholder variables and assign initial invalid data.
            // This way, if the input parameters fail to supply valid data, we know when to abort.
            GH_Structure<GH_Mesh> _meshes; // It’s an out parameter so you don’t have to construct it ahead of time -- David Rutten
            
            // 2. Retrieve input data.
            if (!DA.GetDataTree(0, out _meshes))
                return;

            // 3. Abort on invalid inputs.
            if (!(_meshes.PathCount > 0))
                return;

            for (int i = 0; i < _meshes.Branches.Count; i++)
            {
                foreach (GH_Mesh _mesh in _meshes.Branches[i])
                {
                    if (!_mesh.Value.IsValid)
                        return;

                    // 4. Check for non-manifold Mesh
                    if (!_mesh.Value.IsManifold(true, out bool _isOriented, out bool _hasBoundary))
                        return;

                    // 5. Check if the topology is valid
                    string log = string.Empty;
                    if (!_mesh.Value.IsValidWithLog(out log))
                        return;
                }
            }

            // 6. Now do something productive
            var _E_tree = new Grasshopper.DataTree<Line>();
            var _FFValues = new Grasshopper.DataTree<int>();
            var _EFValues = new Grasshopper.DataTree<int>();

            for (int i = 0; i < _meshes.Branches.Count; i++)
            {
                var e_path = new GH_Path(i);

                foreach (GH_Mesh _mesh in _meshes.Branches[i])
                {
                    var _faceList = _mesh.Value.Faces;
                    //_E_tree.AddRange(_edges, e_path);

                    for (int j = 0; j < _faceList.Count; j++)
                    {
                        var _faces = _faceList.AdjacentFaces(j);
                        var ff_path = new GH_Path(i, j);
                        _FFValues.AddRange(_faces, ff_path);
                    }

                    var _edgeList = _mesh.Value.TopologyEdges;
                    for (int j = 0; j < _edgeList.Count; j++)
                    {
                        var _faces = _edgeList.GetConnectedFaces(j);
                        var ef_path = new GH_Path(i, j);
                        _EFValues.AddRange(_faces, ef_path);
                    }

                    var _edgeLines = new List<Line>();
                    for (int j = 0; j < _edgeList.Count; j++)
                        _edgeLines.Add(_edgeList.EdgeLine(j));

                    _E_tree.AddRange(_edgeLines, e_path);
                }
            }

            DA.SetDataTree(0, _E_tree);
            DA.SetDataTree(1, _FFValues);
            DA.SetDataTree(2, _EFValues);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return My.Resources.Resources.TopologyMeshEdge;
                // Return Nothing
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.quarternary;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{c2db6251-af7b-412b-9e43-225ffde0fa13}");
            }
        }
    }
}