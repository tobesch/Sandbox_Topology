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
    public class GhcTopologyLine : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PolygonEdgeTopology class.
        /// </summary>
        public GhcTopologyLine() : base(
            "Line Topology", 
            "Line Topo", 
            "Analyses the topology of a network consisting of lines", 
            "Sandbox", 
            "Topology")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("List of lines", "L", "Network of lines", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Tolerance", "T", "Tolerance value", GH_ParamAccess.item, 0.001d);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("List of points", "P", "Ordered list of unique points", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Line-Point structure", "LP", "For each line lists both end points indices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Point-Point structure", "PP", "For each point list all point indices connected to it", GH_ParamAccess.tree);
            pManager.AddLineParameter("Point-Line structure", "PL", "For each point list all lines connected to it", GH_ParamAccess.tree);
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
            double _T = 0d;

            // 2. Retrieve input data.
            if (!DA.GetDataTree(0, out _L))
                return;
            if (!DA.GetData(1, ref _T))
                return;

            // 3. Abort on invalid inputs.
            if (_L.PathCount < 1)
                return;
            if (!(_T > 0d))
                return;

            // 4. Do something useful.
            var _polyTree = new Grasshopper.DataTree<Polyline>();

            // 4.1 get inputs
            int count = 0;
            foreach (List<GH_Line> _branch in _L.Branches)
            {
                var path = new GH_Path(count);
                foreach (GH_Line _goo in _branch)
                {
                    var _line = _goo.Value;
                    var _poly = new Polyline(new Point3d[] { _line.From, _line.To });
                    _polyTree.Add(_poly, path);
                }
                count += 1;
            }

            var _PValues = new Grasshopper.DataTree<Point3d>();
            var _LPValues = new Grasshopper.DataTree<int>();
            var _PPValues = new Grasshopper.DataTree<int>();
            var _PLValues = new Grasshopper.DataTree<Line>();

            for (int i = 0, loopTo = _polyTree.Branches.Count - 1; i <= loopTo; i++)
            {

                var branch = _polyTree.Branch(i);
                var mainpath = new GH_Path(i);

                // 4.2 get topology
                var _ptList = TopologyShared.GetPointTopo(branch, _T);
                var _lineList = TopologyShared.GetPLineTopo(branch, _ptList, _T);
                TopologyShared.SetPointPLineTopo(_lineList, _ptList);

                // 4.3 return results
                foreach (PointTopological _ptTopo in _ptList)
                    _PValues.Add(_ptTopo.Point, mainpath);

                // For Each _lineTopo As PLineTopological In _lineList
                for (int j = 0, loopTo1 = _lineList.Count - 1; j <= loopTo1; j++)
                {
                    var _lineTopo = _lineList[j];
                    var args = new int[] { i, j };
                    var _path = new GH_Path(args);
                    _LPValues.Add(_lineTopo.PointIndices[0], _path);
                    _LPValues.Add(_lineTopo.PointIndices[1], _path);
                }

                // For Each _ptTopo As PointTopological In _ptList
                for (int j = 0, loopTo2 = _ptList.Count - 1; j <= loopTo2; j++)
                {
                    var _ptTopo = _ptList[j];
                    var args = new int[] { i, j };
                    var _path = new GH_Path(args);
                    foreach (PLineTopological _lineTopo in _ptTopo.PLines)
                    {
                        if (_ptTopo.Index == _lineTopo.PointIndices[0])
                        {
                            _PPValues.Add(_lineTopo.PointIndices[1], _path);
                        }
                        else if (_ptTopo.Index == _lineTopo.PointIndices[1])
                        {
                            _PPValues.Add(_lineTopo.PointIndices[0], _path);
                        }
                    }
                }

                // For Each _ptTopo As PointTopological In _ptList
                for (int j = 0, loopTo3 = _ptList.Count - 1; j <= loopTo3; j++)
                {
                    var _ptTopo = _ptList[j];
                    var args = new int[] { i, j };
                    var _path = new GH_Path(args);
                    foreach (PLineTopological _lineTopo in _ptTopo.PLines)
                        _PLValues.Add(_L.Branches[i][_lineTopo.Index].Value, _path);
                }

            }

            DA.SetDataTree(0, _PValues);
            DA.SetDataTree(1, _LPValues);
            DA.SetDataTree(2, _PPValues);
            DA.SetDataTree(3, _PLValues);

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

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return My.Resources.Resources.TopologyLine;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{a09956f1-a616-4896-a242-eab3fc506087}");
            }
        }
    }
}