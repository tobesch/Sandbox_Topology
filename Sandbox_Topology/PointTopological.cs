using System.Collections.Generic;
using Rhino.Geometry;

namespace Sandbox
{

    public class PointTopological
    {

        private Point3d _p;
        private int _i;     // internal indexing of the points
        private List<PLineTopological> _l = null;

        public PointTopological(Point3d P, int I)
        {

            _p = P;
            _i = I;
            // _l = L

        }

        // ##### PROPERTIES #####

        public Point3d Point
        {
            get
            {
                return _p;
            }
        }

        public int Index
        {
            get
            {
                return _i;
            }
        }

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