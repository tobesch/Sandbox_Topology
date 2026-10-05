using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Sandbox
{
    static class TopologyValidation
    {
        public static bool ValidateLine(GH_Line lineGoo, GH_Component component, string location)
        {
            if (lineGoo == null || !lineGoo.Value.IsValid)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Line {location} is invalid.");
                return false;
            }

            return true;
        }

        public static bool TryGetClosedPolyline(GH_Curve curveGoo, GH_Component component, string location, out Polyline polyline)
        {
            polyline = new Polyline();
            Curve curve = curveGoo == null ? null : curveGoo.Value;
            if (curve == null)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Curve {location} is null.");
                return false;
            }

            if (!curve.IsValid)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Curve {location} is invalid.");
                return false;
            }

            if (!curve.TryGetPolyline(out polyline))
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Curve {location} cannot be converted to a polyline.");
                return false;
            }

            if (!polyline.IsClosed || polyline.SegmentCount < 3)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Polyline {location} must be closed and have at least 3 segments.");
                return false;
            }

            return true;
        }

        public static bool ValidateMesh(GH_Mesh meshGoo, GH_Component component, string location)
        {
            Mesh mesh = meshGoo == null ? null : meshGoo.Value;
            if (mesh == null)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Mesh {location} is null.");
                return false;
            }

            if (!mesh.IsValidWithLog(out string log))
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Mesh {location} is invalid: {log}");
                return false;
            }

            return true;
        }

        public static bool ValidateBrep(GH_Brep brepGoo, GH_Component component, string location)
        {
            Brep brep = brepGoo == null ? null : brepGoo.Value;
            if (brep == null)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Brep {location} is null.");
                return false;
            }

            if (!brep.IsValidTopology(out _))
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Brep {location} has invalid topology.");
                return false;
            }

            return true;
        }

        public static bool ValidateFilterTrees<T>(GH_Structure<T> elements, GH_Structure<GH_Integer> adjacency, GH_Component component)
            where T : IGH_Goo
        {
            for (int i = 0; i < elements.PathCount; i++)
            {
                if (elements.Paths[i].Indices.Length != 1 || elements.Paths[i].Indices[0] != i)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Element list branch {elements.Paths[i]} must use the {{network}} path format.");
                    return false;
                }
            }

            for (int i = 0; i < adjacency.PathCount; i++)
            {
                GH_Path path = adjacency.Paths[i];
                if (path.Indices.Length != 2)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Adjacency branch {path} must use the {{network; element}} path format.");
                    return false;
                }

                int network = path.Indices[0];
                int element = path.Indices[1];
                if (network < 0 || network >= elements.Branches.Count || element < 0 || element >= elements.Branches[network].Count)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Adjacency branch {path} does not reference an element in the element list.");
                    return false;
                }
            }

            return true;
        }

        public static bool ValidateElementReferences(GH_Structure<GH_Integer> adjacency, GH_Structure<GH_Line> elements, GH_Component component)
        {
            for (int i = 0; i < adjacency.PathCount; i++)
            {
                GH_Path path = adjacency.Paths[i];
                int network = path.Indices[0];
                foreach (GH_Integer reference in adjacency.Branches[i])
                {
                    if (reference.Value < 0 || reference.Value >= elements.Branches[network].Count)
                    {
                        component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Adjacency branch {path} references line index {reference.Value}, which is not present in the line list.");
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
