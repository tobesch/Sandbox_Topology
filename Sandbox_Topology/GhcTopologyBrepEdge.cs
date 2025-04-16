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
    public class GhcTopologyBrepEdge : GH_Component
    {

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GhcTopologyBrepEdge() : base(
            "Brep Topology Edge",
            "Brep Topo Edge",
            "Analyses the edge topology of a Brep",
            "Sandbox",
            "Topology")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Breps", "B", "Breps to analyse", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("List of edge curves", "E", "Ordered list of unique brep edge curves", GH_ParamAccess.tree);
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
            var e_tree = new Grasshopper.DataTree<Curve>();
            var fe_tree = new Grasshopper.DataTree<int>();
            var ef_tree = new Grasshopper.DataTree<int>();

            for (int i = 0; i < _breps.Branches.Count; i++)
            {
                foreach (GH_Brep _brep in _breps.Branches[i])
                {
                    var _edges = _brep.Value.Edges;
                    var e_path = new GH_Path(i);
                    e_tree.AddRange(_edges, e_path);

                    for (int j = 0; j < _brep.Value.Faces.Count; j++)
                    {
                        var face_edges = _brep.Value.Faces[j].AdjacentEdges();
                        var fe_path = new GH_Path(i, j);
                        fe_tree.AddRange(face_edges, fe_path);
                    }

                    for (int j = 0; j < _brep.Value.Edges.Count; j++)
                    {
                        var _faces = _brep.Value.Edges[j].AdjacentFaces();
                        var ef_path = new GH_Path(i, j);
                        ef_tree.AddRange(_faces, ef_path);
                    }                    
                }
            }

            DA.SetDataTree(0, e_tree);
            DA.SetDataTree(1, fe_tree);
            DA.SetDataTree(2, ef_tree);
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
                return new Guid("{9a99f8d8-9f33-4e83-9d8e-562820438a28}");
            }
        }
    }
}