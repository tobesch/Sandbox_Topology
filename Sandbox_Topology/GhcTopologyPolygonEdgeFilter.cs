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
    public class GhcTopologyPolygonEdgeFilter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the NakedPolygonVertices class.
        /// </summary>
        public GhcTopologyPolygonEdgeFilter() : base("Polygon Topology Edge Filter", "Poly Topo Edge Filter", "Filter the edges in a polygon network based on their valency", "Sandbox", "Topology")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Edge list", "E", "Ordered list of edges", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Edge-Loop structure", "EL", "Ordered structure listing the polylines adjacent to each edge", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Valency filter", "V", "Filter edges with the specified number of adjacent polylines", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("List of edge IDs", "I", "List of edge indices matching the valency criteria", GH_ParamAccess.tree);
            pManager.AddLineParameter("List of edges", "E", "List of edges matching the valency criteria", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Declare placeholder variables and assign initial invalid data.
            // This way, if the input parameters fail to supply valid data, we know when to abort.
            GH_Structure<GH_Line> _E;
            GH_Structure<GH_Integer> _EL;
            int _V = 0;

            // 2. Retrieve input data.
            if (!DA.GetDataTree(0, out _E))
                return;
            if (!DA.GetDataTree(1, out _EL))
                return;
            if (!DA.GetData(2, ref _V))
                return;

            // 3. Abort on invalid inputs.
            // 3.1. get the number of branches in the trees
            if (!(_E.PathCount > 0))
                return;
            if (!(_EL.PathCount > 0))
                return;
            if (!(_V > 0))
                return;

            // 4. Do something useful.

            var _idTree = new Grasshopper.DataTree<int>();
            var _edgeTree = new Grasshopper.DataTree<Line>();

            for (int i = 0; i < _E.Branches.Count; i++)
            {

                List<GH_Line> branch = (List<GH_Line>)_E.Branches[i];
                var mainpath = _E.Paths[i];

                for (int j = 0; j < branch.Count; j++)
                {
                    var args = new int[] { i, j };
                    var path = new GH_Path(args);
                    if (_EL.get_Branch(path).Count == _V)
                    {
                        _idTree.Add(j, mainpath);
                        _edgeTree.Add(branch[j].Value, mainpath);
                    }
                }

            }

            DA.SetDataTree(0, _idTree);
            DA.SetDataTree(1, _edgeTree);


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
                return My.Resources.Resources.TopologyPolyEdgeFilter;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{d9534bba-01a3-48c2-b989-4884c03421d9}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.secondary;
            }
        }
    }
}