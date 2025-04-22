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
    public class GhcTopologyLinePointFilter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TopoplogyLineFilter class.
        /// </summary>
        public GhcTopologyLinePointFilter() : base("Line Topology Point Filter", "Line Point Filter", "Filters points in a network of lines based on connectivity", "Sandbox", "Topology")

        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("List of points", "P", "Ordered list of unique points", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Point-Point structure", "PP", "Ordered structure listing the points connected to each point", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Valency filter", "V", "Filter points with the specified number of lines connected to it", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("List of points", "P", "List of points matching the valency criteria", GH_ParamAccess.tree);
            pManager.AddTextParameter("List of point IDs", "I", "List of point indices matching the valency criteria", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Declare placeholder variables and assign initial invalid data.
            // This way, if the input parameters fail to supply valid data, we know when to abort.
            GH_Structure<GH_Point> _P;
            GH_Structure<GH_Integer> _PP;
            int _V = 0;

            // 2. Retrieve input data.
            if (!DA.GetDataTree(0, out _P))
                return;
            if (!DA.GetDataTree(1, out _PP))
                return;
            if (!DA.GetData(2, ref _V))
                return;

            // 3. Abort on invalid inputs.
            if (_P.Branches.Count < 1)
                return;
            if (_PP.Branches.Count < 1)
                return;
            if (!(_V > 0))
                return;

            // 4. Do something useful.
            // 4.1 Filter based on Valency parameter
            var _ptTree = new Grasshopper.DataTree<Point3d>();
            var _idTree = new Grasshopper.DataTree<int>();

            for (int i = 0; i < _PP.Branches.Count; i++)
            {
                var _branch = _PP.Branches[i];

                if (_branch.Count == _V)
                {
                    var curr_path = _PP.Paths[i]; // the path of the current branch
                    var main_path = new GH_Path(curr_path.Indices[0]);
                    int vertex_index = curr_path.Indices[1];
                    _ptTree.Add(_P.Branches[curr_path.Indices[0]][vertex_index].Value, main_path);
                    _idTree.Add(vertex_index, main_path);
                }
            }

            // 4.2: return results
            DA.SetDataTree(0, _ptTree);
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
                return Properties.Resources.Resources.TopologyLinePointFilter;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("618d5d0e-e7e8-452b-8c93-8aaae7fb3207");
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
