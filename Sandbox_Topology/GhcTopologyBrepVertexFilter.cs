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
    public class GhcTopologyBrepVertexFilter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the NakedPolygonVertices class.
        /// </summary>
        public GhcTopologyBrepVertexFilter() : base("Brep Topology Vertex Filter", "Brep Topo Vertex Filter", "Filter the vertices of a brep based on their connectivity", "Sandbox", "Topology")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Vertex list", "V", "Ordered list of points", GH_ParamAccess.tree);
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
            GH_Structure<GH_Point> _V;
            GH_Structure<GH_Integer> _VF;
            int _Val = 0;

            // 2. Retrieve input data.
            if (!DA.GetDataTree(0, out _V))
                return;
            if (!DA.GetDataTree(1, out _VF))
                return;
            if (!DA.GetData(2, ref _Val))
                return;

            // 3. Abort on invalid inputs.
            // 3.1. get the number of branches in the trees
            if (!(_V.PathCount > 0))
                return;
            if (!(_VF.PathCount > 0))
                return;
            if (!(_Val > 0))
                return;

            // 4. Do something useful.
            var _idTree = new Grasshopper.DataTree<int>();
            var _ptTree = new Grasshopper.DataTree<Point3d>();

            for (int i = 0; i < _VF.Branches.Count; i++)
            {
                var branch = _VF.Branches[i];

                if (branch.Count == _Val)
                {
                    var curr_path = _VF.Paths[i]; // the path of the current branch
                    var main_path = new GH_Path(curr_path.Indices[0]);
                    int vertex_index = curr_path.Indices[1];
                    _idTree.Add(vertex_index, main_path);
                    _ptTree.Add(_V.Branches[curr_path.Indices[0]][vertex_index].Value, main_path);
                }
            }

            DA.SetDataTree(0, _idTree);
            DA.SetDataTree(1, _ptTree);

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
                return Properties.Resources.Resources.TopologyBrepPointFilter;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{f3fc318d-6774-4556-b1f8-34342a5e3b92}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.tertiary;
            }
        }
    }
}
