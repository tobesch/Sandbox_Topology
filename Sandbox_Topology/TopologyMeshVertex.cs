using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

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
            pManager.AddMeshParameter("Mesh", "M", "Mesh to analyse", GH_ParamAccess.item);
        }

        /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("List of mesh vertices", "V", "Ordered list of mesh vertices", GH_ParamAccess.list);
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
            Mesh _mesh = null;

            // 2. Retrieve input data.
            if (!DA.GetData(0, ref _mesh))
                return;

            // 3. Abort on invalid inputs.
            if (!_mesh.IsValid)
                return;

            // 4. Check for non-manifold Mesh
            bool _isOriented;
            bool _hasBoundary;
            if (!_mesh.IsManifold(true, out _isOriented, out _hasBoundary))
                return;

            // 5. Check if the topology is valid
            string log = string.Empty;
            if (!_mesh.IsValidWithLog(out log))
                return;

            // 6. Now do something productive
            var _VVValues = new Grasshopper.DataTree<int>();
            var _VFValues = new Grasshopper.DataTree<int>();

            var _vertexList = _mesh.Vertices;
            for (int _vIndex = 0, loopTo = _vertexList.Count - 1; _vIndex <= loopTo; _vIndex++)
            {
                var _indices = _vertexList.GetConnectedVertices(_vIndex);
                var vv_path = new GH_Path(_VVValues.BranchCount);
                foreach (int _vertex in _indices)
                {
                    if (_vertex != _vIndex)
                        _VVValues.Add(_vertex, vv_path);
                }
            }

            for (int _vIndex = 0, loopTo1 = _vertexList.Count - 1; _vIndex <= loopTo1; _vIndex++)
            {
                var _faces = _vertexList.GetVertexFaces(_vIndex);
                var vf_path = new GH_Path(_VFValues.BranchCount);
                _VFValues.AddRange(_faces, vf_path);
            }

            var _VList = new List<Point3d>();
            var _vertices = _vertexList.ToPoint3dArray();
            _VList.AddRange(_vertices);

            DA.SetDataList(0, _VList);
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