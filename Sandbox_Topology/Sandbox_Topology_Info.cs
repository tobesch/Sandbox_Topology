using Grasshopper.Kernel;

namespace Sandbox
{

    public class Sandbox_Topology_Info : GH_AssemblyInfo
    {

        public override string AssemblyName
        {
            get
            {
                return "Sandbox Topology";
            }
        }

        public override GH_LibraryLicense License
        {
            get
            {
                return GH_LibraryLicense.opensource;
            }
        }
        public override string Description
        {
            get
            {
                return "Tools for experiments in computational architecture";
            }
        }
        public override string Version
        {
            get
            {
                return "1.0.0.0";
            }
        }
        public override string Name
        {
            get
            {
                return "Sandbox Topology";
            }
        }
        public override string AuthorContact
        {
            get
            {
                return "tobias.schwinn@gmail.com";
            }
        }
        public override string AuthorName
        {
            get
            {
                return "Tobias Schwinn";
            }
        }

        // Override here any more methods you see fit.
        // Start typing Public Overrides..., select a property and push Enter.

    }
}