using System;
using System.Collections.Generic;
using System.IO;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Sandbox
{

    /// <summary>
    /// 
    /// </summary>
    public class GhcTopologyBrepEdgeFilter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the NakedPolygonVertices class.
        /// </summary>
        public GhcTopologyBrepEdgeFilter() : base(
            "Brep Topology Edge Filter",
            "Brep Topo Edge Filter",
            "Filter the edges of a brep based on their valency",
            "Sandbox",
            "Topology")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Edge list", "E", "Ordered list of edges", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Edge-Face structure", "EF", "For each edge the list of adjacent face indices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Valency filter", "V", "Filter edges with the specified number of adjacent faces", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("List of edge IDs", "I", "List of edge indices matching the valency criteria", GH_ParamAccess.tree);
            pManager.AddCurveParameter("List of edges", "E", "List of edges matching the valency criteria", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Declare placeholder variables and assign initial invalid data.
            // This way, if the input parameters fail to supply valid data, we know when to abort.
            GH_Structure<GH_Curve> _E;
            GH_Structure<GH_Integer> _EF;
            int _Val = 0;

            // 2. Retrieve input data.
            if (!DA.GetDataTree(0, out _E))
                return;
            if (!DA.GetDataTree(1, out _EF))
                return;
            if (!DA.GetData(2, ref _Val))
                return;

            // 3. Abort on invalid inputs.
            // 3.1. get the number of branches in the trees
            if (!(_E.PathCount > 0))
                return;
            if (!(_EF.PathCount > 0))
                return;
            if (!(_Val > 0))
                return;

            // 4. Do something useful.
            var id_tree = new Grasshopper.DataTree<int>();
            var e_tree = new Grasshopper.DataTree<Curve>();

            for (int i = 0; i < _EF.Branches.Count; i++)
            {
                var _branch = _EF.Branches[i];

                if (_branch.Count == _Val)
                {
                    var curr_path = _EF.Paths[i]; // the path of the current branch
                    var main_path = new GH_Path(curr_path.Indices[0]);
                    int edge_index = curr_path.Indices[1];
                    id_tree.Add(edge_index, main_path);
                    e_tree.Add(_E.Branches[curr_path.Indices[0]][edge_index].Value, main_path);
                }                    
            }

            DA.SetDataTree(0, id_tree);
            DA.SetDataTree(1, e_tree);

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
                return My.Resources.Resources.TopologyBrepEdgeFilter;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{7ca5a286-2357-4e49-bc8b-38682c40b558}");
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