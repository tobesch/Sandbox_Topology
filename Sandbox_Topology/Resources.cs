using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Sandbox.Properties.Resources
{
    internal static class Resources
    {
        private const string IconResourcePrefix = "Sandbox.Resources.";
        private static readonly Dictionary<string, Bitmap> Icons = new Dictionary<string, Bitmap>();
        private static readonly object IconsLock = new object();

        internal static Bitmap TopologyBrepEdge => GetIcon(nameof(TopologyBrepEdge));
        internal static Bitmap TopologyBrepEdgeFilter => GetIcon(nameof(TopologyBrepEdgeFilter));
        internal static Bitmap TopologyBrepPoint => GetIcon(nameof(TopologyBrepPoint));
        internal static Bitmap TopologyBrepPointFilter => GetIcon(nameof(TopologyBrepPointFilter));
        internal static Bitmap TopologyLine => GetIcon(nameof(TopologyLine));
        internal static Bitmap TopologyLineFilter => GetIcon(nameof(TopologyLineFilter));
        internal static Bitmap TopologyLinePointFilter => GetIcon(nameof(TopologyLinePointFilter));
        internal static Bitmap TopologyMeshEdge => GetIcon(nameof(TopologyMeshEdge));
        internal static Bitmap TopologyMeshEdgeFilter => GetIcon(nameof(TopologyMeshEdgeFilter));
        internal static Bitmap TopologyMeshPoint => GetIcon(nameof(TopologyMeshPoint));
        internal static Bitmap TopologyMeshPointFilter => GetIcon(nameof(TopologyMeshPointFilter));
        internal static Bitmap TopologyPolyEdge => GetIcon(nameof(TopologyPolyEdge));
        internal static Bitmap TopologyPolyEdgeFilter => GetIcon(nameof(TopologyPolyEdgeFilter));
        internal static Bitmap TopologyPolyPoint => GetIcon(nameof(TopologyPolyPoint));
        internal static Bitmap TopologyPolyPointFilter => GetIcon(nameof(TopologyPolyPointFilter));

        private static Bitmap GetIcon(string name)
        {
            lock (IconsLock)
            {
                if (Icons.TryGetValue(name, out Bitmap icon))
                {
                    return icon;
                }

                string resourceName = IconResourcePrefix + name + ".png";
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new InvalidOperationException($"Embedded icon resource '{resourceName}' was not found.");
                    }

                    using (var bitmap = new Bitmap(stream))
                    {
                        icon = new Bitmap(bitmap);
                    }
                }

                Icons.Add(name, icon);
                return icon;
            }
        }
    }
}
