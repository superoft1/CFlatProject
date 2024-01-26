using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chiyoda.CAD.Plotplan
{
    public abstract class BoundaryDistanceEvaluatorOnXYplane:Evaluation1dFunction 
    {
        protected static double BoundaryDistance(List<BoundaryLine> firstLines, List<BoundaryLine>secondLines)
        {
            var dist = double.PositiveInfinity;
            foreach (BoundaryLine firstLine in firstLines)
            {
                foreach (BoundaryLine secondLine in secondLines)
                {
                    dist = Math.Min(dist, LinesDistance(firstLine, secondLine));
                    if (dist<=0)
                    {
                        return dist;
                    }
                }
            }
            return dist;
        }

        private static double LinesDistance(BoundaryLine firstLine, BoundaryLine secondLine)
        {
            if (firstLine is StraightLine && secondLine is StraightLine)
            {
                var stf = firstLine as StraightLine;
                var sts = secondLine as StraightLine;
                return StraightLinesDistance(stf, sts);
            }
            if (firstLine is CircularLine && secondLine is StraightLine)
            {
                var cir = firstLine as CircularLine;
                var st  = secondLine as StraightLine;
                return StraightLineAndCircularLineDistance(st,cir);
            }
            if (firstLine is StraightLine && secondLine is CircularLine)
            {
                var cir = secondLine as CircularLine;
                var st = firstLine as StraightLine;
                return StraightLineAndCircularLineDistance(st, cir);
            }
            if (firstLine is CircularLine && secondLine is CircularLine)
            {
                var cirf = firstLine as CircularLine;
                var cirs = secondLine as CircularLine;
                return CircularLinesDistance(cirf, cirs);
            }
            return -1;
        }

        private static double StraightLinesDistance(StraightLine firstLine ,StraightLine secondLine)
        {
            if (IsIntersect(firstLine,secondLine)) { return 0; }
            var dist = double.PositiveInfinity;
            var fs = firstLine.SourcePoint;
            var ft = firstLine.TargetPoint;
            var ss = secondLine.SourcePoint;
            var st = secondLine.TargetPoint;
            dist = Math.Min(dist, PointAndLineDistance(fs, ss, st));
            dist = Math.Min(dist, PointAndLineDistance(ft, ss, st));
            dist = Math.Min(dist, PointAndLineDistance(ss, fs, ft));
            dist = Math.Min(dist, PointAndLineDistance(st, fs, ft));
            return dist;
        }

        private static double PointAndLineDistance(Vector3d point, StraightLine straightLine)
        {
            return PointAndLineDistance(point, straightLine.SourcePoint, straightLine.TargetPoint);
        }
        private static double PointAndLineDistance(Vector3d point,Vector3d lineEndPoint1,Vector3d lineEndPoint2)
        {
            var v = lineEndPoint2 - lineEndPoint1;
            var s = point - lineEndPoint1;
            var t = point - lineEndPoint2;
            v.z = 0;
            s.z = 0;
            t.z = 0;
            var sv = Vector3d.Dot(s, v);
            var tv = Vector3d.Dot(t, -v);
            if (sv <= 0)
            {
                return s.magnitude ;
            }
            if (tv <= 0)
            {
                return t.magnitude;
            }
            var h = s-Vector3d.Project(s, v);
            return h.magnitude;
        }

        private static double StraightLineAndCircularLineDistance(StraightLine straightLine, CircularLine circularLine)
        {
            var dist = double.PositiveInfinity;
            var center = circularLine.Center;
            var cs = circularLine.SourcePoint - center;
            var ct = circularLine.TargetPoint - center;
            var ss = straightLine.SourcePoint - center;
            var st = straightLine.TargetPoint - center;
            var csRad = Radian(cs);
            var ctRad = Radian(ct, csRad);
            var ssRad = Radian(ss, csRad);
            var stRad = Radian(st, csRad);
            var r = cs.magnitude;
            var v = st - ss;
            var rSup = Math.Max(ss.magnitude, st.magnitude);
            bool isIntersectS = IsIntersect(ss, st, Vector3d.zero, cs.normalized * rSup);
            bool isIntersectT = IsIntersect(ss, st, Vector3d.zero, ct.normalized * rSup);

            if (!(isIntersectS || isIntersectT))
            {
                if (ssRad <= ctRad)
                {
                    return StraightLineAndCircleDistance(ss, st, Vector3d.zero, r);
                }
                dist = Math.Min(PointAndLineDistance(cs, ss, st),dist);
                dist = Math.Min(PointAndLineDistance(ct, ss, st),dist);
                return dist;
            }
            if (isIntersectS)
            {
                var interS = IntersectionPoint(ss,st,Vector3d.zero, cs.normalized * rSup);
                if (isIntersectT)
                {
                    var interT = IntersectionPoint(ss, st, Vector3d.zero, ct.normalized * rSup);
                    if (ssRad <= ctRad)
                    {
                        dist = Math.Min(PointAndLineDistance(cs, interS, interT), dist);
                        dist = Math.Min(PointAndLineDistance(ct, interS, interT), dist);
                        if (ssRad <= stRad)
                        {
                            dist = Math.Min(StraightLineAndCircleDistance(ss, interS, Vector3d.zero, r), dist);
                            dist = Math.Min(StraightLineAndCircleDistance(st, interT, Vector3d.zero, r), dist);

                            return dist;
                        }
                        dist = Math.Min(StraightLineAndCircleDistance(st, interS, Vector3d.zero, r), dist);
                        dist = Math.Min(StraightLineAndCircleDistance(ss, interT, Vector3d.zero, r), dist);
                        return dist;
                    }
                    dist = Math.Min(StraightLineAndCircleDistance(interS, interT, Vector3d.zero, r), dist);
                    //return dist ←ひょっとしたらこれだけでいける？
                    if (ssRad <= stRad)
                    {
                        dist = Math.Min(PointAndLineDistance(cs, interS, st), dist);
                        dist = Math.Min(PointAndLineDistance(ct, interT, ss), dist);
                        return dist;
                    }
                    dist = Math.Min(PointAndLineDistance(cs, interS, ss), dist);
                    dist = Math.Min(PointAndLineDistance(ct, interT, st), dist);
                    return dist;
                }
                if(ssRad<=stRad)
                {
                    dist = Math.Min(StraightLineAndCircleDistance(interS, ss, Vector3d.zero, r), dist);
                    dist = Math.Min(PointAndLineDistance(cs, interS, st), dist);
                    dist = Math.Min(PointAndLineDistance(ct, interS, st), dist);
                    return dist;
                }
                dist = Math.Min(StraightLineAndCircleDistance(interS, st, Vector3d.zero, r), dist);
                dist = Math.Min(PointAndLineDistance(cs, interS, ss), dist);
                dist = Math.Min(PointAndLineDistance(ct, interS, ss), dist);
                return dist;
            }
            if (isIntersectT)
            {
                var interT = IntersectionPoint(ss, st, Vector3d.zero, ct.normalized * rSup);
                if (ssRad <= stRad)
                {
                    dist = Math.Min(StraightLineAndCircleDistance(interT, ss, Vector3d.zero, r), dist);
                    dist = Math.Min(PointAndLineDistance(cs, interT, st), dist);
                    dist = Math.Min(PointAndLineDistance(ct, interT, st), dist);
                    return dist;
                }
                dist = Math.Min(StraightLineAndCircleDistance(interT, st, Vector3d.zero, r), dist);
                dist = Math.Min(PointAndLineDistance(cs, interT, ss), dist);
                dist = Math.Min(PointAndLineDistance(ct, interT, ss), dist);
                return dist;
            }
            return -1;
        }


        private static double CircularLinesDistance(CircularLine firstCircularLine, CircularLine secondCircularLine)
        {
            var dist = double.PositiveInfinity;
            if (IsIntersect(firstCircularLine, secondCircularLine)) { return 0; }
            var fc = firstCircularLine.Center;
            var fs = firstCircularLine.SourcePoint;
            var ft = firstCircularLine.TargetPoint;
            var fr = firstCircularLine.Radius;
            var sc = secondCircularLine.Center;
            var ss = secondCircularLine.SourcePoint;
            var st = secondCircularLine.TargetPoint;
            var sr = secondCircularLine.Radius;
            var v = sc - fc;
            var d = v.magnitude;
            var fm = fc + v.normalized * fr;
            var sm = sc - v.normalized * sr;
            var fsRad = Radian(fs - fc);
            var fmRad = Radian(fm - fc, fsRad);
            var ftRad = Radian(ft - fc,fsRad);
            var ssRad = Radian(ss - sc);
            var smRad = Radian(sm - sc, ssRad);
            var stRad = Radian(st - sc,ssRad);

            dist = Math.Min(PointAndCircularLineDistance(fs, sc, ss, ssRad, st, stRad), dist);
            dist = Math.Min(PointAndCircularLineDistance(ft, sc, ss, ssRad, st, stRad), dist);
            dist = Math.Min(PointAndCircularLineDistance(ss, fc, fs, fsRad, ft, ftRad), dist);
            dist = Math.Min(PointAndCircularLineDistance(st, fc, fs, fsRad, ft, ftRad), dist);
            if (fmRad <= ftRad)
            {
                dist = Math.Min(PointAndCircularLineDistance(fm, sc, ss, ssRad, st, stRad), dist);
            }
            if (smRad <= stRad)
            {
                dist = Math.Min(PointAndCircularLineDistance(sm, fc, fs, fsRad, ft, ftRad), dist);
            }
            return dist;
        }

        private static double PointAndCircularLineDistance(Vector3d point, CircularLine circularLine)
        {
            var c = circularLine.Center;
            var s = circularLine.SourcePoint;
            var t = circularLine.TargetPoint;
            return PointAndCircularLineDistance(point,c,s,t);
        }

        private static double PointAndCircularLineDistance(Vector3d point, Vector3d center, Vector3d sourcePoint, Vector3d targetPoint)
        {
            var sRad = Radian(sourcePoint - center);
            var tRad = Radian(targetPoint - center,sRad);
            return PointAndCircularLineDistance(point, center, sourcePoint,sRad, targetPoint,tRad);
        }
        private static double PointAndCircularLineDistance(Vector3d point, Vector3d center, Vector3d sourcePoint,double sourceRadian, Vector3d tagetPoint,double targetRadian)
        {
            var pRad = Radian(point - center,sourceRadian);
            if (pRad <= targetRadian)
            {
                var d = point - center;
                var r = point - sourcePoint;
                d.z = 0;
                r.z = 0;
                return Math.Abs(d.magnitude - r.magnitude);
            }
            var sp = (point - sourcePoint);
            var tp = (point - tagetPoint);
            sp.z = 0;
            tp.z = 0;
            return Math.Min(sp.magnitude , tp.magnitude);
        }
        private static double Radian(Vector3d vec, double infimum=-Math.PI)
        {
            double rad = Math.Atan2(vec.y, vec.x);
            while(rad<= infimum)
            {
                rad += 2 * Math.PI;
            }
            return rad;
        }
        private static double StraightLineAndCircleDistance(StraightLine straightLine, CircularLine circularLine)
        {
            var center = circularLine.Center;
            var cs = circularLine.SourcePoint;
            var r = circularLine.Radius;
            var ss = straightLine.SourcePoint;
            var st = straightLine.TargetPoint;
            return StraightLineAndCircleDistance(ss, st, center, r);
        }
        private static double StraightLineAndCircleDistance(Vector3d lineEndPoint1, Vector3d lineEndPoint2,Vector3d center,double r)
        {
            var s = lineEndPoint1 - center;
            var t = lineEndPoint2 - center;
            s.z = 0;
            t.z = 0;
            if ((s.magnitude-r)*(t.magnitude-r)<=0)
            {
                return 0;
            }
            var v = t- s;
            var h = Vector3d.Project(s, v);
            if (Vector3d.Dot(h - s, h - v) <= 0)
            {
                return h.magnitude;
            }
            return Math.Min(s.magnitude,t.magnitude);
        }

        private static bool IsIntersect(CircularLine firstLine, CircularLine secondLine)
        {
            var fc = firstLine.Center;
            var fs = firstLine.SourcePoint;
            var ft = firstLine.TargetPoint;
            var fr = firstLine.Radius;
            var sc = secondLine.Center;
            var ss = secondLine.SourcePoint;
            var st = secondLine.TargetPoint;
            var sr = secondLine.Radius;
            var v = sc - fc;
            var d = v.magnitude;

            if (fr + sr >= d && sr + d >= fr && d + fr >= sr) 
            {
                var theta = Math.Acos((fr * fr + d * d - sr * sr) / 2 * fr * d);
                var fsRad = Radian(fs - fc);
                var ftRad = Radian(ft - fc,fsRad);
                var fvRad = Radian(v, fsRad);
                var ccwFlag = fvRad + theta <= ftRad || fsRad + 2 * Math.PI <= fvRad + theta;
                var cwFlag = fvRad - theta >= fsRad || ftRad - 2 * Math.PI <= fvRad - theta;
                if(ccwFlag||cwFlag)
                {
                    var phi = Math.Acos((sr * sr + d * d - fr * fr) / 2 * sr * d);
                    var ssRad = Radian(ss - sc);
                    var stRad = Radian(st - sc, ssRad);
                    var svRad = Radian(v, ssRad);
                    if(ccwFlag&(svRad - phi >= ssRad || stRad - 2 * Math.PI <= svRad - phi))
                    {
                        return true;
                    }
                    if (cwFlag&(svRad + phi <= stRad || ssRad + 2 * Math.PI <= svRad + phi))
                    {
                        return true;
                    }
                }
            }
            return false;

        }
        private static bool IsIntersect(StraightLine firstLine, StraightLine secondLine)
        {
            var fs = firstLine.SourcePoint;
            var ft = firstLine.TargetPoint;
            var ss = secondLine.SourcePoint;
            var st = secondLine.TargetPoint;
            return IsIntersect(fs, ft, ss, st);
        }
        private static bool IsIntersect(Vector3d p, Vector3d q, Vector3d r, Vector3d s)
        {
            p.z = 0;
            q.z = 0;
            r.z = 0;
            s.z = 0;
            var f = p - q;
            var u = p - r;
            var v = p - s;
            if (Vector3d.Dot(Vector3d.Cross(f, u), Vector3d.Cross(f, v)) >= 0) { return false; }
            var g = r - s;
            var w = r - p;
            var t = r - p;
            if (Vector3d.Dot(Vector3d.Cross(g, w), Vector3d.Cross(g, t)) >= 0) { return false; }
            return true;
        }
        private static Vector3d IntersectionPoint(Vector3d p, Vector3d q, Vector3d r, Vector3d s)
        {
            var u = q - p;
            var v = s - r;
            var w = p - r;
            var h = w - Vector3d.Project(w, u);
            h.Normalize();
            double a=Vector3d.Dot(h, v);
            if (a==0)
            {
                return new Vector3d(double.NaN, double.NaN, double.NaN);
            }
            return r + v / a;
        }
    }
    
}

