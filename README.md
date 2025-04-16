Sandbox_Topology
================

# Short description

Sandbox Topology provides tools for the topological analysis and filtering of line, polyline, mesh and brep networks.

Such networks are defined at the lowest level by points and lines / curves, but might also include loops and faces as in the case of polylines, meshes or breps / polysurfaces.

If you have every asked yourself questions such as, "How can I get all vertices in a mesh that have exactly 5 face neighbors?" or "How can I select the two faces that share a given edge in a brep?", then this plugin is for you.

# Motivation

On various occasions during a computational design process, low-level control over the sub-elements that make up the design geometry might become necessary as well as decision making based on the adjacency of faces, valency of nodes, or connectivity between points. Usage examples include design for fabrication, line and mesh networks for dynamic relaxation, path analysis, planarization of polygon networks, optimization, etc.

Sandbox Topology combines separate tools, which I have informally developed over the course of some 2-3 years alongside projects in academia and practice. While the development of custom digital tools that are specific to design projects has become a very common process, there often isn't the time to collect, prepare, and release a "clean" version of such a custom developed tool. However, having encountered similar tasks of topological network analysis often enough, and after having validated the tools across various projects, I'm happy to share it as Sandbox Topology with the Grasshopper ecosystem.

# Related work

Others, of course, have also addressed some of the issues and scenarios that are included here, such as Daniel Piker's Topologizer for line networks or David Rutten's Brep Topology component. However, topological network analysis being a common topic, I do think that it deserves an organized toolbox in the form of its very own Grasshopper addon.

# Functionality

The add-on consists of four groups of components that relate to the analysis of line, (closed) polyline, mesh, and brep networks. Each group contains components that perform the topological analysis and components for filtering based on the information obtained by the analysis. While Brep and Mesh analysis mostly utilize built-in Rhino Common functionality, Line and Polyline analysis rely on custom developed TopologicalPoint and PLine classes.

In the latest version, the tools support datatrees as input. In other words, multiple networks can be analyzed and filtered in parallel. For a full change log, see https://github.com/tobesch/Sandbox_Topology/releases

# Compatibility

The latest version of Sandbox Topology uses .NET Framework 4.5, so it should still be compatible with Rhino 6.

# Installation:

As of Rhino 7, the best way to install the plugin is via Rhino's PackageManager. Just type "PackageManager", then search for "Sandbox", make sure you select the latest version, and click "Install". In this case, example files can be found %appdata%\McNeel\Rhinoceros\packages\7.0. 

Alternatively, in Rhino 6 launch Grasshopper, choose File > Special Folders > Components folder. Copy the .gha file there. Right-click the file > Properties > make sure there is no "blocked" text. Restart Rhino and Grasshopper. Example files can be found below.

# Example files

Example files that go along with the different releases can be found in the PackageManager installation folder (Rhino 7 and later) or as part of the source code at the following location:
https://github.com/tobesch/Sandbox_Topology/releases

# License

In the spirit of the Grasshopper community that thrives on knowledge sharing, I made the add-on open source (MIT license). The repository for the code development is publicly available for download and contribution on Github.

# Disclaimer

While I have tested this toolbox, it might still contain bugs. Please use it "as is", it does not come with any warranties for what it can do or liability for damages it can cause. If you see room for improvement feel free to contact me or even consider contributing to its development.

- Tobias

PS: The title image is from wikipedia (https://en.wikipedia.org/wiki/File:NetworkTopology-Star.png) and currently serves as a placeholder...