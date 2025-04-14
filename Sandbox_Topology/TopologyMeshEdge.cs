using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Sandbox
{

    /// <summary>
    /// 
    /// </summary>
    public class TopologyMeshEdge : GH_Component
    {

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TopologyMeshEdge() : base("Mesh Topology Edge", "Mesh Topo Edge", "Analyses the edge topology of a Mesh", "Sandbox", "Topology")

        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to analyse", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("List of edges", "E", "Ordered list of mesh edges", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Face-Face structure", "FF", "For each face the list of adjacent face indices", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Edge-Face structure", "EF", "For each edge the list of adjacent face indices", GH_ParamAccess.tree);
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
            Mesh _mesh = null;

            // 2. Retrieve input data.
            if (!DA.GetData(0, ref _mesh))
                return;

            // 3. Abort on invalid inputs.
            if (!_mesh.IsValid)
                return;

            // 4. Check for non-manifold Mesh
            if (!_mesh.IsManifold(true, out bool _isOriented, out bool _hasBoundary))
                return;

            // 5. Check if the topology is valid
            string log = string.Empty;
            if (!_mesh.IsValidWithLog(out log))
                return;

            // 6. Now do something productive
            var _FFValues = new Grasshopper.DataTree<int>();
            var _EFValues = new Grasshopper.DataTree<int>();

            var _faceList = _mesh.Faces;
            for (int _fIndex = 0, loopTo = _faceList.Count - 1; _fIndex <= loopTo; _fIndex++)
            {
                var _faces = _faceList.AdjacentFaces(_fIndex);
                var ff_path = new GH_Path(_FFValues.BranchCount);
                _FFValues.AddRange(_faces, ff_path);
            }

            var _edgeList = _mesh.TopologyEdges;
            for (int _eIndex = 0, loopTo1 = _edgeList.Count - 1; _eIndex <= loopTo1; _eIndex++)
            {
                var _faces = _edgeList.GetConnectedFaces(_eIndex);
                var ef_path = new GH_Path(_EFValues.BranchCount);
                _EFValues.AddRange(_faces, ef_path);
            }

            var _EList = new List<Line>();
            for (int _eIndex = 0, loopTo2 = _edgeList.Count - 1; _eIndex <= loopTo2; _eIndex++)
                _EList.Add(_edgeList.EdgeLine(_eIndex));

            DA.SetDataList(0, _EList);
            DA.SetDataTree(1, _FFValues);
            DA.SetDataTree(2, _EFValues);

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
                return My.Resources.Resources.TopologyMeshEdge;
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
                return GH_Exposure.quarternary;
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
                return new Guid("{c2db6251-af7b-412b-9e43-225ffde0fa13}");
            }
        }
    }
}