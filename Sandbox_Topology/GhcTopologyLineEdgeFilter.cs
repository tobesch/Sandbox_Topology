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
    public class GhcTopologyLineEdgeFilter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TopoplogyLineFilter class.
        /// </summary>
        public GhcTopologyLineEdgeFilter() : base("Line Topology Edge Filter", "Line Edge Filter", "Filters edges in a network of lines based on connectivity", "Sandbox", "Topology")

        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("List of lines", "L", "Ordered list of unique lines", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Point-Line structure", "PL", "Ordered structure listing the lines connected to each point", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Valency filter", "V", "Filter points with the specified number of lines connected to it", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("List of lines", "L", "List of lines connected to points matching the valency criteria", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("List of line IDs", "I", "List of line indices connected to points matching the valency criteria", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Declare placeholder variables and assign initial invalid data.
            // This way, if the input parameters fail to supply valid data, we know when to abort.
            GH_Structure<GH_Line> _L;
            GH_Structure<GH_Integer> _PL;
            int _V = 0;

            // 2. Retrieve input data.
            if (!DA.GetDataTree(0, out _L))
                return;
            if (!DA.GetDataTree(1, out _PL))
                return;
            if (!DA.GetData(2, ref _V))
                return;

            // 3. Abort on invalid inputs.
            if (_L.Branches.Count < 1)
                return;
            if (_PL.Branches.Count < 1)
                return;
            if (!(_V > 0))
                return;

            // 4. Do something useful.
            // 4.1 Filter based on Valency parameter
            var _lineTree = new Grasshopper.DataTree<Line>();
            var _idTree = new Grasshopper.DataTree<int>();

            for (int i = 0; i < _PL.Branches.Count; i++)
            {
                var _branch = _PL.Branches[i];

                if (_branch.Count == _V)
                {
                    var curr_path = _PL.Paths[i]; // the path of the current branch
                    var main_path = new GH_Path(curr_path.Indices[0]);

                    foreach (GH_Integer _item in _branch)
                    {
                        _lineTree.Add(_L.Branches[curr_path.Indices[0]][_item.Value].Value, curr_path);
                        _idTree.Add(_item.Value, curr_path);
                    }
                }
            }

            // 4.2: return results
            DA.SetDataTree(0, _lineTree);
            DA.SetDataTree(1, _idTree);

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
                return Properties.Resources.Resources.TopologyLineFilter;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("e93a15b7-7acd-418f-8d5d-704466bee99e");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.primary;
            }
        }
    }
}
