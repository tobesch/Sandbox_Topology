using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Sandbox
{


    public class TopologyLineFilter : GH_Component
    {
        /// <summary>
    /// Initializes a new instance of the TopoplogyLineFilter class.
    /// </summary>
        public TopologyLineFilter() : base("Line Topology Filter", "Line Filter", "Filters a network of lines based on connectivity", "Sandbox", "Topology")

        {
        }

        /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("List of points", "P", "Ordered list of unique points", GH_ParamAccess.list);
            pManager.AddLineParameter("Point-Line structure", "PL", "Ordered structure listing the lines connected to each point", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Valency filter", "V", "Filter points with the specified number of lines connected to it", GH_ParamAccess.item, 1);
        }

        /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("List of points", "P", "List of points matching the valency criteria", GH_ParamAccess.list);
            pManager.AddLineParameter("Line structure", "L", "For each filtered point lists the lines connected to it", GH_ParamAccess.tree);
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
            var _PL = new GH_Structure<GH_Line>();
            int _V = 0;

            // 2. Retrieve input data.
            if (!DA.GetDataList(0, _P))
                return;
            if (!DA.GetDataTree(1, out _PL))
                return;
            if (!DA.GetData(2, ref _V))
                return;

            // 3. Abort on invalid inputs.
            if (!(_P.Count > 0))
                return;
            if (!(_PL.Branches.Count > 0))
                return;
            if (!(_V > 0))
                return;

            // 4. Do something useful.
            // 4.1 Filter based on Valency parameter
            var _ptList = new List<Point3d>();
            var _lValues = new Grasshopper.DataTree<Line>();

            for (int i = 0, loopTo = _PL.Branches.Count - 1; i <= loopTo; i++)
            {

                var _branch = _PL.Branches[i];
                if (_branch.Count == _V)
                {
                    _ptList.Add(_P[i].Value);

                    var _path = new GH_Path(i);
                    foreach (GH_Line _item in _branch)
                        _lValues.Add(_item.Value, _path);

                }
            }

            // 4.2: return results
            DA.SetDataList(0, _ptList);
            DA.SetDataTree(1, _lValues);

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
                return My.Resources.Resources.TopologyLineFilter;
            }
        }

        /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{251697aa-b454-4f30-8bf7-0f00ea707fc2}");
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