using System.Collections.Generic;
using Microsoft.VisualBasic;
using Rhino.Geometry;

namespace Sandbox
{

    static class TopologyShared
    {

        private static bool ContainsPoint(List<PointTopological> _points, Point3d _check, double _T)
        {

            foreach (PointTopological _item in _points)
            {
                if (_item.Point.DistanceTo(_check) < _T)
                {
                    // consider it the same point
                    return true;
                }
            }

            return false;

        }

        public static List<PointTopological> GetPointTopo(List<Polyline> P, double _T)
        {

            var _ptList = new List<PointTopological>();

            int _count = 0;
            foreach (Polyline _poly in P)
            {

                var _points = _poly.ToArray();

                for (int i = 0; i < _points.Length; i++)
                {
                    // check if point exists in _ptList already
                    if (!ContainsPoint(_ptList, _points[i], _T))
                    {
                        _ptList.Add(new PointTopological(_points[i], _count));
                        _count += 1;
                    }
                }
            }

            return _ptList;

        }

        public static List<PLineTopological> GetPLineTopo(List<Polyline> P, List<PointTopological> _ptDict, double _T)
        {

            var _lDict = new List<PLineTopological>();

            int _count = 0;
            foreach (Polyline _poly in P)
            {

                var _points = _poly.ToArray();

                var _indices = new List<int>();

                for (int i = 0; i < _points.Length; i++)
                {
                    foreach (PointTopological _item in _ptDict)
                    {
                        if (_item.Point.DistanceTo(_points[i]) < _T)
                        {
                            _indices.Add(_item.Index);
                            break;
                        }
                    }
                }

                _lDict.Add(new PLineTopological(_indices, _count));

                _count += 1;

            }

            return _lDict;

        }

        public static void SetPointPLineTopo(List<PLineTopological> _lineList, List<PointTopological> _pointList)
        {

            foreach (PointTopological _pt in _pointList)
            {

                var _lList = new List<PLineTopological>();

                foreach (PLineTopological _l in _lineList)
                {

                    foreach (int _index in _l.PointIndices)
                    {
                        if (_index == _pt.Index)
                        {
                            _lList.Add(_l);
                            break;
                        }
                    }

                }

                _pt.PLines = _lList;

            }

        }
    }
}