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
    public class TopologyMeshVertexFilter : GH_Component
    {
        /// <summary>
    /// Initializes a new instance of the NakedPolygonVertices class.
    /// </summary>
        public TopologyMeshVertexFilter() : base("Mesh Topology Vertex Filter", "Mesh Topo Vertex Filter", "Filter the vertices of a mesh based on their connectivity", "Sandbox", "Topology")

        {
        }

        /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Vertex list", "V", "Ordered list of points", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Vertex-Face structure", "VF", "For each vertex the list of adjacent face indices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Valency filter", "Val", "Filter vertices with the specified number of adjacent faces", GH_ParamAccess.item, 1);
        }

        /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("List of vertex IDs", "I", "List of vertex indices matching the valency criteria", GH_ParamAccess.list);
            pManager.AddPointParameter("List of vertices", "P", "List of vertices matching the valency criteria", GH_ParamAccess.tree);
        }

        /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Declare placeholder variables and assign initial invalid data.
            // This way, if the input parameters fail to supply valid data, we know when to abort.
            var _P = new List<GH_Point>();
            GH_Structure<GH_Integer> _PF = null;
            int _V = 0;

            // 2. Retrieve input data.
            if (!DA.GetDataList(0, _P))
                return;
            if (!DA.GetDataTree(1, out _PF))
                return;
            if (!DA.GetData(2, ref _V))
                return;

            // 3. Abort on invalid inputs.
            // 3.1. get the number of branches in the trees
            if (!(_P.Count > 0))
                return;
            if (!(_PF.PathCount > 0))
                return;
            if (!(_V > 0))
                return;

            // 4. Do something useful.
            var _idList = new List<int>();
            for (int i = 0, loopTo = _PF.Branches.Count - 1; i <= loopTo; i++)
            {
                var _branch = _PF.Branches[i];
                if (_branch.Count == _V)
                {
                    _idList.Add(i);
                }
            }

            var _ptList = new List<Point3d>();
            foreach (int _id in _idList)
                _ptList.Add(_P[_id].Value);

            DA.SetDataList(0, _idList);
            DA.SetDataList(1, _ptList);

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
                return My.Resources.Resources.TopologyMeshPointFilter;
            }
        }

        /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{0d5dea63-930b-4842-92d6-6a81d9ea3fc9}");
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
    }
}