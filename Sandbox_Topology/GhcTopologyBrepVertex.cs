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
    public class GhcTopologyBrepVertex : GH_Component
    {

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GhcTopologyBrepVertex() : base("Brep Topology Vertex", "Brep Topo Vertex", "Analyses the vertex topology of a Brep", "Sandbox", "Topology")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Brep to analyse", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("List of Vertices", "V", "Ordered list of unique brep vertices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Face-Vertex structure", "FV", "For each face list vertex indices belonging to face", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Vertex-Face structure", "VF", "For each vertex list adjacent face indices", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Declare placeholder variables and assign initial invalid data.
            // This way, if the input parameters fail to supply valid data, we know when to abort.
            GH_Structure<GH_Brep> _breps; // It’s an out parameter so you don’t have to construct it ahead of time -- David Rutten

            // 2. Retrieve input data.
            if (!DA.GetDataTree(0, out _breps))
                return;

            // 3. Abort on invalid inputs.
            if (!(_breps.PathCount > 0))
                return;

            for (int i = 0; i < _breps.Branches.Count; i++)
            {
                foreach (GH_Brep _brep in _breps.Branches[i])
                {
                    // 3.1. Check for non-manifold Breps
                    if (!_brep.Value.IsManifold)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "One of the input breps is non-manifold!");
                        return;
                    }
                    // 3.2. Check if the topology is valid
                    if (!_brep.Value.IsValidTopology(out _))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "One of the input breps has invalid topology!");
                        return;
                    }
                }
            }

            // 4. Now do something productive
            var _polyTree = new Grasshopper.DataTree<Polyline>();

            // 4.1. check inputs
            for (int i = 0; i < _breps.Branches.Count; i++)
            {
                var path = new GH_Path(i);
                foreach (GH_Brep _brep in _breps.Branches[i])
                {
                    foreach (BrepLoop _loop in _brep.Value.Loops)
                    {
                        Polyline _poly;
                        if (!_loop.To3dCurve().TryGetPolyline(out _poly))
                            return;
                        _polyTree.Add(_poly, path);
                    }
                }
            }

            var _VValues = new Grasshopper.DataTree<Point3d>();
            var _FVValues = new Grasshopper.DataTree<int>();
            var _VFValues = new Grasshopper.DataTree<int>();

            for (int i = 0; i < _polyTree.Branches.Count; i++)
            {

                var branch = _polyTree.Branch(i);
                var mainpath = new GH_Path(i);

                // 4.2. get topology
                double _T = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
                var _ptList = TopologyShared.GetPointTopo(branch, _T);
                var _fList = TopologyShared.GetPLineTopo(branch, _ptList, _T);
                TopologyShared.SetPointPLineTopo(_fList, _ptList);

                // 4.3: return results
                foreach (PointTopological _ptTopo in _ptList)
                    _VValues.Add(_ptTopo.Point, mainpath);

                for (int j = 0; j < _fList.Count; j++)
                {
                    var _lineTopo = _fList[j]; 
                    var args = new int[] { i, j };
                    var _path = new GH_Path(args); ;
                    foreach (int _index in _lineTopo.PointIndices)
                        _FVValues.Add(_index, _path);
                }

                for (int j = 0; j < _ptList.Count; j++)
                {
                    var _ptTopo = _ptList[j];
                    var args = new int[] { i, j };
                    var _path = new GH_Path(args);
                    foreach (PLineTopological _lineTopo in _ptTopo.PLines)
                        _VFValues.Add(_lineTopo.Index, _path);
                }
            }

            DA.SetDataTree(0, _VValues);
            DA.SetDataTree(1, _FVValues);
            DA.SetDataTree(2, _VFValues);
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
                return My.Resources.Resources.TopologyBrepPoint;
                // Return Nothing
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

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{b585dbc3-37eb-4387-b6d2-7bd220dc2470}");
            }
        }
    }
}