using System.Collections.Generic;

namespace Sandbox
{
    public class PLineTopological
    {

        private List<int> _v;
        private int _i;     // internal indexing of the lines

        public PLineTopological(List<int> V, int I)
        {

            _v = V;
            _i = I;

        }

        // ##### PROPERTIES #####

        public int Index
        {
            get
            {
                return _i;
            }
        }

        public List<int> PointIndices
        {
            get
            {
                return _v;
            }
        }

    }
}