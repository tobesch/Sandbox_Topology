using System.Collections.Generic;

namespace Sandbox
{
    /// <summary>
    /// 
    /// </summary>
    public class PLineTopological
    {

        private List<int> _v;
        private int _i;     // internal indexing of the lines

        /// <summary>
        /// 
        /// </summary>
        /// <param name="V"></param>
        /// <param name="I"></param>
        public PLineTopological(List<int> V, int I)
        {

            _v = V;
            _i = I;

        }

        // ##### PROPERTIES #####

        /// <summary>
        /// 
        /// </summary>
        public int Index
        {
            get
            {
                return _i;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<int> PointIndices
        {
            get
            {
                return _v;
            }
        }

    }
}