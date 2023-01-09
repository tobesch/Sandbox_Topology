using System;
using Grasshopper.Kernel;

using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Sandbox
{


    public class GhcTopologyPolygonPoint : GH_Component
    {
        /// <summary>
    /// Initializes a new instance of the PolygonTopology class.
    /// </summary>
        public GhcTopologyPolygonPoint() : base("Polygon Topology Point", "Poly Topo Point", "Analyses the point topology of a network consisting of closed polylines", "Sandbox", "Topology")
        {
        }

        /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("List of polylines", "C", "Network of closed polylines", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Tolerance", "T", "Tolerance value", GH_ParamAccess.item, 0.001d);
        }

        /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("List of points", "P", "Ordered list of unique points", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Loop-Point structure", "LP", "For each polyline lists all point indices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Point-Loop structure", "PL", "For each point lists all adjacent polyline indices", GH_ParamAccess.tree);
        }

        /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Declare placeholder variables and assign initial invalid data.
            // This way, if the input parameters fail to supply valid data, we know when to abort.
            var _C = new GH_Structure<GH_Curve>();
            double _T = 0d;

            // 2. Retrieve input data.
            if (!DA.GetDataTree(0, out _C))
                return;
            if (!DA.GetData(1, ref _T))
                return;

            // 3. Abort on invalid inputs.
            if (!(_C.PathCount > 0))
                return;
            if (!(_T > 0d))
                return;

            // 4. Do something useful.
            var _polyTree = new Grasshopper.DataTree<Polyline>();

            // 4.1. check inputs
            for (int i = 0, loopTo = _C.Branches.Count - 1; i <= loopTo; i += 1)
            {
                var path = new GH_Path(i);
                foreach (GH_Curve _crv in _C.Branches[i])
                {
                    Polyline _poly = null;
                    if (!_crv.Value.TryGetPolyline(out _poly))
                        return;
                    _polyTree.Add(_poly, path);
                }
            }

            var _PValues = new Grasshopper.DataTree<Point3d>();
            var _FPValues = new Grasshopper.DataTree<int>();
            var _PFValues = new Grasshopper.DataTree<int>();

            for (int i = 0, loopTo1 = _polyTree.Branches.Count - 1; i <= loopTo1; i++)
            {

                var branch = _polyTree.Branch(i);
                var mainpath = new GH_Path(i);

                // 4.2. get topology
                var _ptList = TopologyShared.getPointTopo(branch, _T);
                var _fList = TopologyShared.getPLineTopo(branch, _ptList, _T);
                TopologyShared.setPointPLineTopo(_fList, _ptList);

                // 4.3: return results
                foreach (PointTopological _ptTopo in _ptList)
                    _PValues.Add(_ptTopo.Point, mainpath);

                for (int j = 0, loopTo2 = _fList.Count - 1; j <= loopTo2; j++)
                {
                    var _lineTopo = _fList[j];
                    var args = new int[] { i, j };
                    var _path = new GH_Path(args);
                    foreach (int _index in _lineTopo.PointIndices)
                        _FPValues.Add(_index, _path);
                }

                for (int j = 0, loopTo3 = _ptList.Count - 1; j <= loopTo3; j++)
                {
                    var _ptTopo = _ptList[j];
                    var args = new int[] { i, j };
                    var _path = new GH_Path(args);
                    foreach (PLineTopological _lineTopo in _ptTopo.PLines)
                        _PFValues.Add(_lineTopo.Index, _path);
                }
            }

            DA.SetDataTree(0, _PValues);
            DA.SetDataTree(1, _FPValues);
            DA.SetDataTree(2, _PFValues);

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
                // return Resources.IconForThisComponent;
                return My.Resources.Resources.TopologyPolyPoint;
            }
        }

        /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{df9c3597-cbc2-4a05-b6e5-22eac1049469}");
            }
        }

        public override GH_Exposure Exposure
        {
            get
            {
                return GH_Exposure.secondary;
            }
        }

    }
}