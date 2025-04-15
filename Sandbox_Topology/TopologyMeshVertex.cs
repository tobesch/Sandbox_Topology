using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;

namespace Sandbox
{

    /// <summary>
    /// 
    /// </summary>
    public class TopologyMeshVertex : GH_Component
    {

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TopologyMeshVertex() : base("Mesh Topology Vertex", "Mesh Topo Vertex", "Analyses the vertex topology of a Mesh", "Sandbox", "Topology")

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
            pManager.AddPointParameter("List of mesh vertices", "V", "Ordered list of mesh vertices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Vertex-Vertex structure", "VV", "For each vertex the list of adjacent vertex indices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Vertex-Face structure", "VF", "For each vertex the list of adjacent face indices", GH_ParamAccess.tree);
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
            var _V_tree = new DataTree<Point3d>();
            var _VVValues = new DataTree<int>();
            var _VFValues = new DataTree<int>();

            for (int i = 0; i < _meshes.Branches.Count; i++)
            {
                var v_path = new GH_Path(i);

                foreach (GH_Mesh _mesh in _meshes.Branches[i])
                {
                    var _vertexList = _mesh.Value.Vertices;
                    for (int j = 0; j < _vertexList.Count; j++)
                    {
                        var _indices = _vertexList.GetConnectedVertices(j);
                        var vv_path = new GH_Path(i, j);
                        foreach (int _vertex in _indices)
                        {
                            if (_vertex != j)
                                _VVValues.Add(_vertex, vv_path);
                        }
                    }

                    for (int j = 0; j < _vertexList.Count; j++)
                    {
                        var _faces = _vertexList.GetVertexFaces(j);
                        var vf_path = new GH_Path(i, j);
                        _VFValues.AddRange(_faces, vf_path);
                    }

                    var _vertices = _vertexList.ToPoint3dArray();
                    _V_tree.AddRange(_vertices, v_path);
                }
            }

            DA.SetDataTree(0, _V_tree);
            DA.SetDataTree(1, _VVValues);
            DA.SetDataTree(2, _VFValues);

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
                return My.Resources.Resources.TopologyMeshPoint;
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
                return new Guid("{412f2c84-4675-4282-8b55-8a8c8d325e6d}");
            }
        }
    }
}