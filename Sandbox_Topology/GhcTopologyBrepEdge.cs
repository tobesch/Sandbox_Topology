using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Sandbox
{


    public class GhcTopologyBrepEdge : GH_Component
    {

        /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
        public GhcTopologyBrepEdge() : base("Brep Topology Edge", "Brep Topo Edge", "Analyses the edge topology of a Brep", "Sandbox", "Topology")
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
            pManager.AddIntegerParameter("Face-Edge structure", "FE", "For each face list edge indices belonging to face", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Edge-Face structure", "EF", "For each edge lists adjacent face indices", GH_ParamAccess.tree);
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
            var e_tree = new Grasshopper.DataTree<int>();

            foreach (BrepFace _face in _brep.Faces)
            {

                var _edges = _face.AdjacentEdges();
                var e_path = new GH_Path(e_tree.BranchCount);
                e_tree.AddRange(_edges, e_path);

            }


            var f_tree = new Grasshopper.DataTree<int>();

            foreach (BrepEdge _edge in _brep.Edges)
            {

                var _faces = _edge.AdjacentFaces();
                var f_path = new GH_Path(f_tree.BranchCount);
                f_tree.AddRange(_faces, f_path);
            }

            DA.SetDataTree(0, e_tree);
            DA.SetDataTree(1, f_tree);

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
                return My.Resources.Resources.TopologyBrepEdge;
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
                return new Guid("{9a99f8d8-9f33-4e83-9d8e-562820438a28}");
            }
        }
    }
}