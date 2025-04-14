using System.Collections.Generic;
using Rhino.Geometry;

namespace Sandbox
{
    /// <summary>
    /// 
    /// </summary>
    public class PointTopological
    {

        private Point3d _p;
        private int _i;     // internal indexing of the points
        private List<PLineTopological> _l = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="P"></param>
        /// <param name="I"></param>
        public PointTopological(Point3d P, int I)
        {

            _p = P;
            _i = I;
            // _l = L

        }

        // ##### PROPERTIES #####

        /// <summary>
        /// 
        /// </summary>
        public Point3d Point
        {
            get
            {
                return _p;
            }
        }

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
        public List<PLineTopological> PLines
        {
            set
            {
                _l = value;
            }
            get
            {
                return _l;
            }
        }

    }
}