# Sandbox Topology Copilot Instructions

## Project scope

Sandbox Topology is a single C# Grasshopper plugin assembly for Rhino. It targets **.NET Framework 4.5** and uses the checked-in RhinoCommon and Grasshopper 6.35 packages under `packages/`. The project emits a `.gha` Grasshopper plugin; the source assembly itself is not the distributable artifact.

The repository also contains Grasshopper examples in `Examples/` and package-ready copies in `PackageManager/`. Keep the package manifest version and the assembly version in `Sandbox_Topology/Sandbox_Topology_Info.cs` aligned when making a release.

## Build, test, and lint

- Build the solution with Visual Studio 2022 or MSBuild:

  ```powershell
  msbuild .\Sandbox_Topology.sln /t:Build /p:Configuration=Release /p:Platform="Any CPU"
  ```

- The project default configuration is `Debug32`. To build that configuration explicitly:

  ```powershell
  msbuild .\Sandbox_Topology.sln /t:Build /p:Configuration=Debug32 /p:Platform="Any CPU"
  ```

- Builds run the project's post-build event: it copies the target assembly to a `.gha`, copies that plugin to `%AppData%\Grasshopper\Libraries\Sandbox_Topology.gha`, and removes the original `.dll`. Close Rhino/Grasshopper first if that destination file is locked.

- There is no automated test project, test runner, linter, or single-test command. Validate behavior in Rhino/Grasshopper with the matching `.gh` definition in `Examples/` (or `PackageManager/examples/`) and inspect the component's data-tree output.

## Architecture

- `Sandbox_Topology/` is the only buildable project. Every user-facing tool is a `GH_Component` subclass in the `Sandbox` namespace and appears under the Grasshopper `Sandbox` tab and `Topology` panel.
- Components are organized by geometry family:
  - Line and closed-polyline components calculate topology with the custom model in `TopologyShared.cs`, `PointTopological.cs`, and `PLineTopological.cs`.
  - Mesh and Brep components use RhinoCommon topology APIs directly after validating their input geometry.
  - `*Filter` components consume the ordered geometry and adjacency data trees emitted by their matching analysis component; they filter an element when its associated adjacency branch has the requested count.
- `Sandbox_Topology_Info.cs` supplies Grasshopper assembly metadata. Component icons are embedded through `Resources.resx` and accessed through `Properties.Resources.Resources`.
- `PackageManager/manifest.yml`, `PackageManager/Sandbox_Topology.gha`, and `PackageManager/examples/` form the Rhino package payload. The top-level `Examples/` directory is the development copy of the example definitions.

## Component and data-tree conventions

- Preserve every released component's `ComponentGuid`; Grasshopper documents serialize component instances by this value, so changing it breaks existing `.gh` files.
- Components accept and return `GH_ParamAccess.tree` where topology must remain partitioned by input network. Analysis components put the ordered geometry in `{network}` and the adjacency list for element `j` in `{network; j}`. Filters depend on this two-level path shape: they use the first index to select the source geometry branch and the second as that geometry's index.
- Line/polyline topology treats two points as identical only when `DistanceTo` is strictly less than the supplied positive tolerance. Preserve both the strict comparison and first-seen ordering because the emitted adjacency indices refer to that ordered unique-point list.
- Validate inputs before calculating topology. Existing mesh and Brep components reject invalid or non-manifold geometry; Brep validation reports a Grasshopper runtime warning. New components that expose topology should retain compatible validation and tree-path behavior.
- Keep component constructor metadata, input/output parameter names, short names, descriptions, category/subcategory, icon resource, and `GH_Exposure` coherent with the paired analysis/filter components. Add a matching embedded 24×24 icon resource for a new component.
- New source files and resources must be added explicitly to `Sandbox_Topology/Sandbox_Topology.csproj`; this is an old-style project and does not glob source files automatically.
