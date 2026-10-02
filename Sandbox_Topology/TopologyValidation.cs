using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace Sandbox
{
    static class TopologyValidation
    {
        public static bool ValidateMesh(Mesh mesh, GH_Component component, string location)
        {
            if (mesh == null || !mesh.IsValid)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Mesh {location} is invalid.");
                return false;
            }

            if (!mesh.IsManifold(true, out _, out _))
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Mesh {location} must be manifold.");
                return false;
            }

            if (!mesh.IsValidWithLog(out string log))
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Mesh {location} has invalid topology: {log}");
                return false;
            }

            return true;
        }

        public static bool ValidateBrep(Brep brep, GH_Component component, string location)
        {
            if (brep == null || !brep.IsManifold)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Brep {location} must be manifold.");
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
            if (elements.PathCount == 0)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Element list must contain at least one branch.");
                return false;
            }

            if (adjacency.PathCount == 0)
            {
                component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Adjacency structure must contain at least one branch.");
                return false;
            }

            for (int i = 0; i < elements.PathCount; i++)
            {
                if (elements.Paths[i].Indices.Length != 1 || elements.Paths[i].Indices[0] != i)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Element list branch {elements.Paths[i]} must use the {{network}} path format.");
                    return false;
                }

                if (elements.Branches[i].Count == 0)
                {
                    component.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Element list branch {elements.Paths[i]} must contain at least one element.");
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
