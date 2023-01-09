using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Sandbox
{


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
            pManager.AddBrepParameter("Brep", "B", "Brep to analyse", GH_ParamAccess.item);
        }

        /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
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
            Brep _brep = null;

            // 2. Retrieve input data.
            if (!DA.GetData(0, ref _brep))
                return;

            // 3. Abort on invalid inputs.
            if (!_brep.IsValid)
                return;

            // 4. Check for non-manifold Breps
            if (!_brep.IsManifold)
                return;

            // 5. Check if the topology is valid
            string log = string.Empty;
            if (!_brep.IsValidTopology(out log))
                return;

            // 6. Now do something productive
            var _polyList = new List<Polyline>();

            foreach (BrepLoop _loop in _brep.Loops)
            {
                Polyline _poly = null;
                if (!_loop.To3dCurve().TryGetPolyline(out _poly))
                    return;
                _polyList.Add(_poly);
            }

            // 4.2. get topology
            double _T = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            var _ptList = new List<PointTopological>();
            foreach (BrepVertex _vertex in _brep.Vertices)
                _ptList.Add(new PointTopological(_vertex.Location, _ptList.Count));
            // Dim _ptList As List(Of PointTopological) = getPointTopo(_polyList, _T)
            var _fList = TopologyShared.getPLineTopo(_polyList, _ptList, _T);
            TopologyShared.setPointPLineTopo(_fList, _ptList);

            // 4.3: return results
            var _FV = new Grasshopper.DataTree<int>();
            foreach (PLineTopological _lineTopo in _fList)
            {
                var _path = new GH_Path(_FV.BranchCount);
                foreach (int _index in _lineTopo.PointIndices)
                    _FV.Add(_index, _path);
            }

            var _VF = new Grasshopper.DataTree<int>();
            foreach (PointTopological _ptTopo in _ptList)
            {
                var _path = new GH_Path(_VF.BranchCount);
                foreach (PLineTopological _lineTopo in _ptTopo.PLines)
                    _VF.Add(_lineTopo.Index, _path);
            }

            DA.SetDataTree(0, _FV);
            DA.SetDataTree(1, _VF);

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