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
    public class GhcTopologyPolygonPointFilter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GhcTopologyPolygonPointFilter class.
        /// </summary>
        public GhcTopologyPolygonPointFilter() : base("Polygon Topology Point Filter", "Poly Topo Point Filter", "Filter the points in a polygon network based on their connectivity", "Sandbox", "Topology")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point list", "P", "Ordered list of points", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Point-Loop structure", "PL", "Ordered structure listing the polylines adjacent to each point", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Valency filter", "V", "Filter points with the specified number of adjacent polylines", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("List of point IDs", "I", "List of point indices matching the valency criteria", GH_ParamAccess.tree);
            pManager.AddPointParameter("List of points", "P", "List of points matching the valency criteria", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Declare placeholder variables and assign initial invalid data.
            // This way, if the input parameters fail to supply valid data, we know when to abort.
            var _P = new GH_Structure<GH_Point>();
            var _PF = new GH_Structure<GH_Integer>();
            int _V = 0;

            // 2. Retrieve input data.
            if (!DA.GetDataTree(0, out _P))
                return;
            if (!DA.GetDataTree(1, out _PF))
                return;
            if (!DA.GetData(2, ref _V))
                return;

            // 3. Abort on invalid inputs.
            // 3.1. get the number of branches in the trees
            if (!(_P.PathCount > 0))
                return;
            if (!(_PF.PathCount > 0))
                return;
            if (!(_V > 0))
                return;

            // 4. Do something useful.
            // Dim _ptList As List(Of Point3d) = _P
            var _pfTree = _PF;

            var _idTree = new Grasshopper.DataTree<int>();
            var _ptTree = new Grasshopper.DataTree<Point3d>();

            for (int i = 0, loopTo = _P.Branches.Count - 1; i <= loopTo; i++)
            {

                List<GH_Point> branch = (List<GH_Point>)_P.get_Branch(i);
                var mainpath = new GH_Path(i);

                for (int j = 0, loopTo1 = branch.Count - 1; j <= loopTo1; j++)
                {
                    var args = new int[] { i, j };
                    var path = new GH_Path(args);
                    if (_PF.get_Branch(path).Count == _V)
                    {
                        _idTree.Add(j, mainpath);
                        _ptTree.Add(branch[j].Value, mainpath);
                    }
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
                return My.Resources.Resources.TopologyPolyPointFilter;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{340977b2-12a6-4dbd-9dac-b2a6c058c5e8}");
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