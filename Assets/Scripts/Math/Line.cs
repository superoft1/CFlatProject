using Tests.MathTest.Editor ;

namespace Chiyoda.Math
{
  public struct Line
  {
    public Vec3 Origin ;
    public Vec3 Direction ;

    public Line(Vec3 origin, Vec3 direction)
    {
      this.Origin = origin ;
      this.Direction = direction ;
    }

    public override string ToString()
    {
      return $"Line[Origin{Origin.ToElementString()},Direction{Direction.ToElementString()}]" ;
    }

    public Vec3 GetPointAt( double parameter )
    {
      return this.Origin + this.Direction * parameter;
    }

    public (double Distance, Vec3 PointOnLine, double Parameter) DistanceTo( Vec3 targetPoint )
    {
      var pointOnLine1 = this.Origin;
      var pointOnLine2 = this.Origin + this.Direction;

      var vecAB = pointOnLine2 - pointOnLine1;
      var vecAP = targetPoint - pointOnLine1;

      var projectedInfo = vecAP.ProjectTo(vecAB);
      var vecAQ = projectedInfo.Point ;
      var vecPQ = vecAQ - vecAP;
      var closestPoint = pointOnLine1 + vecAQ;

      return (vecPQ.Length, closestPoint, projectedInfo.Parmeter);
    }

    public (double Distance, Vec3 PointOnSelf, Vec3 PointOnTarget, double ParameterOnSelf, double ParameterOnTarget) DistanceTo( Line targetLine )
    {
      Vec3 line1_point1 = this.Origin;
      Vec3 line1_point2 = this.Origin + this.Direction;
      Vec3 line2_point1 = targetLine.Origin;
      Vec3 line2_point2 = targetLine.Origin + targetLine.Direction;

      Vec3 vec_da = line1_point2 - line1_point1;
      Vec3 vec_db = line2_point2 - line2_point1;
      Vec3 vec_ab = line2_point1 - line1_point1;

      double abs_vec_db = vec_db.Length*vec_db.Length;
      double abs_vec_da = vec_da.Length*vec_da.Length;

      double delta = (abs_vec_da*abs_vec_db - vec_da.Dot( vec_db )*vec_da.Dot( vec_db ));

      if (delta <= Tol.Double)
      {
        // 平行または含まれるケース
        var info = targetLine.DistanceTo(this.Origin);
        return (info.Distance, this.Origin, targetLine.Origin, 0.0, 0.0);
      }

      var parameterOnSelf = (abs_vec_db * vec_ab.Dot(vec_da) - vec_da.Dot(vec_db) * vec_ab.Dot(vec_db)) / delta;
      var parameterOnTarget = (vec_da.Dot( vec_db )*vec_ab.Dot( vec_da ) - abs_vec_da*vec_ab.Dot( vec_db ))/ delta;

      var closestPointOnSelf = line1_point1 + vec_da*parameterOnSelf;
      var closestPointOnTarget = line2_point1 + vec_db*parameterOnTarget;
      double distance = closestPointOnSelf.DistanceTo( closestPointOnTarget );
      return ( distance, closestPointOnSelf, closestPointOnTarget, parameterOnSelf, parameterOnTarget ) ;
    }
  }
}