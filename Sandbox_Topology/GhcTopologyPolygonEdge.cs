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
    public class GhcTopologyPolygonEdge : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TopologyPolygonEdge class.
        /// </summary>
        public GhcTopologyPolygonEdge() : base("Polygon Topology Edge", "Poly Topo Edge", "Analyses the edge topology of a curve network consisting of closed polylines", "Sandbox", "Topology")
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
            pManager.AddLineParameter("List of edges", "E", "Ordered list of unique polyline edges", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Loop-Edge structure", "LE", "For each polyline lists edge indices belonging to polyline", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Edge-Loop structure", "EL", "For each edge lists adjacent polyline indices", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Declare placeholder variables and assign initial invalid data.
            // This way, if the input parameters fail to supply valid data, we know when to abort.
            GH_Structure<GH_Curve> _C; // It’s an out parameter so you don’t have to construct it ahead of time -- David Rutten
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
            for (int i = 0; i < _C.Branches.Count; i++)
            {
                var path = new GH_Path(i);
                foreach (GH_Curve _crv in _C.Branches[i])
                {
                    Polyline _poly;
                    if (!_crv.Value.TryGetPolyline(out _poly))
                        return;
                    _polyTree.Add(_poly, path);
                }
            }

            var _EValues = new Grasshopper.DataTree<Line>();
            var _FEValues = new Grasshopper.DataTree<int>();
            var _EFValues = new Grasshopper.DataTree<int>();

            for (int i = 0; i < _polyTree.Branches.Count; i++)
            {

                var branch = _polyTree.Branch(i);
                var mainpath = new GH_Path(i);

                // 4.2. get topology
                var _edgeDict = getEdgeDict(branch, _T);
                var _fDict = getFaceDict(branch, _edgeDict, _T);
                var _edgeFaceDict = getEdgeFaceDict(_fDict, _edgeDict);

                // 4.3: return results
                foreach (KeyValuePair<string, Line> _pair in _edgeDict)
                    _EValues.Add(_pair.Value, mainpath);

                for (int j = 0; j < _fDict.Count; j++)
                {
                    var _edgeIndexList = _fDict["F" + j];
                    var args = new int[] { i, j };
                    var _path = new GH_Path(args);
                    // For Each _edgeIndexList As List(Of String) In _fDict.Values
                    foreach (string _item in _edgeIndexList)
                        _FEValues.Add(Int32.Parse(_item.Substring(1)), _path);
                }

                for (int j = 0; j < _edgeFaceDict.Count; j++)
                {
                    var _fList = _edgeFaceDict["E" + j];
                    var args = new int[] { i, j };
                    var _path = new GH_Path(args);
                    // For Each _fList As List(Of String) In _edgeFaceDict.Values
                    foreach (string _item in _fList)
                        _EFValues.Add(Int32.Parse(_item.Substring(1)), _path);
                }

            }

            DA.SetDataTree(0, _EValues);
            DA.SetDataTree(1, _FEValues);
            DA.SetDataTree(2, _EFValues);

        }

        private Dictionary<string, List<string>> getEdgeFaceDict(Dictionary<string, List<string>> _fDict, Dictionary<string, Line> _edgeDict)
        {

            var _edgeFaceDict = new Dictionary<string, List<string>>();

            foreach (string _edgeID in _edgeDict.Keys)
            {

                var _fList = new List<string>();

                foreach (string _key in _fDict.Keys)
                {
                    var _values = _fDict[_key];

                    foreach (string _value in _values)
                    {
                        if ((_edgeID ?? "") == (_value ?? ""))
                        {
                            _fList.Add(_key);
                        }
                    }

                }

                _edgeFaceDict.Add(_edgeID, _fList);

            }

            return _edgeFaceDict;

        }

        private Dictionary<string, List<string>> getFaceDict(List<Polyline> _polyList, Dictionary<string, Line> _edgeDict, double _T)
        {

            var _fDict = new Dictionary<string, List<string>>();

            int _count = 0;
            foreach (Polyline _poly in _polyList)
            {

                string _Fkey = "F" + _count;

                Line[] _edges = _poly.GetSegments();

                var _value = new List<string>();

                for (int i = 0; i < _edges.Length; i++)
                {

                    foreach (string _key in _edgeDict.Keys)
                    {
                        if (compareEdges(_edgeDict[_key], _edges[i], _T))
                        {
                            _value.Add(_key);
                            break;
                        }
                    }

                }

                _fDict.Add(_Fkey, _value);

                _count += 1;

            }

            return _fDict;

        }

        private Dictionary<string, Line> getEdgeDict(List<Polyline> _polyList, double _T)
        {

            var _edgeDict = new Dictionary<string, Line>();

            int _count = 0;
            foreach (Polyline _poly in _polyList)
            {
                Line[] _edges = _poly.GetSegments();

                for (int i = 0; i < _edges.Length; i++)
                {
                    // check if edge exists in _edgeDict already
                    if (!containsEdge(_edgeDict, _edges[i], _T))
                    {
                        string _key = "E" + _count;
                        var _value = _edges[i];
                        _edgeDict.Add(_key, _value);
                        _count += 1;
                    }
                }
            }

            return _edgeDict;

        }

        private bool compareEdges(Line _line1, Line _line2, double _T)
        {

            var _startPt = _line1.PointAt(0d);
            var _endPt = _line1.PointAt(1d);
            if (_startPt.DistanceTo(_line2.PointAt(0d)) < _T && _endPt.DistanceTo(_line2.PointAt(1d)) < _T)
            {
                // consider it the same edge
                return true;
            }
            else if (_startPt.DistanceTo(_line2.PointAt(1d)) < _T && _endPt.DistanceTo(_line2.PointAt(0d)) < _T)
            {
                // consider it the same edge
                return true;
            }

            return false;

        }

        private bool containsEdge(Dictionary<string, Line> _edgeDict, Line _check, double _T)
        {

            foreach (Line _l in _edgeDict.Values)
            {
                var _startPt = _l.PointAt(0d);
                var _endPt = _l.PointAt(1d);
                if (_startPt.DistanceTo(_check.PointAt(0d)) < _T && _endPt.DistanceTo(_check.PointAt(1d)) < _T)
                {
                    // consider it the same edge
                    return true;
                }
                else if (_startPt.DistanceTo(_check.PointAt(1d)) < _T && _endPt.DistanceTo(_check.PointAt(0d)) < _T)
                {
                    // consider it the same edge
                    return true;
                }
            }

            return false;

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

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return My.Resources.Resources.TopologyPolyEdge;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{83e3815d-02aa-422c-9650-4905ff46d48a}");
            }
        }
    }
}