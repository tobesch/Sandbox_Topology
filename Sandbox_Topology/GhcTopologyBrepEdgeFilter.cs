using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Sandbox
{


    public class GhcTopologyBrepEdgeFilter : GH_Component
    {
        /// <summary>
    /// Initializes a new instance of the NakedPolygonVertices class.
    /// </summary>
        public GhcTopologyBrepEdgeFilter() : base("Brep Topology Edge Filter", "Brep Topo Edge Filter", "Filter the edges of a brep based on their valency", "Sandbox", "Topology")
        {
        }

        /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Edge list", "E", "Ordered list of edges", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Edge-Face structure", "EF", "For each edge the list of adjacent face indices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Valency filter", "V", "Filter edges with the specified number of adjacent faces", GH_ParamAccess.item, 1);
        }

        /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("List of edge IDs", "I", "List of edge indices matching the valency criteria", GH_ParamAccess.list);
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
            var _E = new List<GH_Line>();
            GH_Structure<GH_Integer> _EF = null;
            int _C = 0;

            // 2. Retrieve input data.
            if (!DA.GetDataList(0, _E))
                return;
            if (!DA.GetDataTree(1, out _EF))
                return;
            if (!DA.GetData(2, ref _C))
                return;

            // 3. Abort on invalid inputs.
            // 3.1. get the number of branches in the trees
            if (!(_E.Count > 0))
                return;
            if (!(_EF.PathCount > 0))
                return;
            if (!(_C > 0))
                return;

            // 4. Do something useful.

            var _idList = new List<int>();
            for (int i = 0, loopTo = _EF.Branches.Count - 1; i <= loopTo; i++)
            {
                var _branch = _EF.Branches[i];
                if (_branch.Count == _C)
                {
                    _idList.Add(i);
                }
            }

            var _eList = new List<Line>();
            foreach (int _id in _idList)
                _eList.Add(_E[_id].Value);

            DA.SetDataList(0, _idList);
            DA.SetDataList(1, _eList);

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

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.tertiary;
            }
        }
    }
}