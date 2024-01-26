using Chiyoda.Math ;
using NUnit.Framework ;
using UnityScript.Steps ;

namespace Tests.MathTest.Editor
{
  public class LineTest:MathTestBase
  {
    [Test]
    public void Init()
    {
      var line = new Line( new Vec3( 1.0, 2.0, 3.0 ), new Vec3( 1.0, 1.0, 1.0 ) ) ;
      Assert.AreEqual( new Vec3( 1.0, 2.0, 3.0 ), line.Origin );
      Assert.AreEqual( new Vec3( 1.0, 1.0, 1.0 ), line.Direction );
    }

    [Test]
    public void ToStr()
    {
      var line = new Line( new Vec3( 1.0, 2.0, 3.0 ), new Vec3( 1.0, 1.0, 1.0 ) ) ;
      Assert.AreEqual( "Line[Origin[1,2,3],Direction[1,1,1]]", line.ToString() );
    }
    
    [Test]
    public void GetPointAt()
    {
      var line = new Line( new Vec3( 2.0, 3.0, 5.0 ), new Vec3( 1.0, 1.0, 0.0 ) );
      AssertVec3Equal( new Vec3(2.0, 3.0, 5.0), line.GetPointAt( 0.0 ) ) ;
      AssertVec3Equal( new Vec3(3.0, 4.0, 5.0), line.GetPointAt(1.0));
      AssertVec3Equal( new Vec3(2.5, 3.5, 5.0),  line.GetPointAt(0.5));
      AssertVec3Equal( new Vec3(4.0, 5.0, 5.0),  line.GetPointAt(2.0));
      AssertVec3Equal( new Vec3(1.0, 2.0, 5.0),  line.GetPointAt(-1.0));
    }

    [Test]
    public void DistanceToToPoint()
    {
      // Line＆Point
      var pnt1 = new Vec3(1.0, 0.0, 0.0);
      {
        var line = new Line(new Vec3(1.0, 0.0, 0.0), new Vec3(0.0, 0.0, 1.0));
        var info = line.DistanceTo( pnt1 ) ;
        Assert.AreEqual( 0.0, info.Distance, delta ) ;
        AssertVec3Equal( new Vec3( 1.0, 0.0, 0.0 ), info.PointOnLine ) ;
        Assert.AreEqual( 0.0, info.Parameter, delta ) ;
      }
      {
        var line2 = new Line(new Vec3(0.0, 0.0, 0.0), new Vec3(0.0, 0.0, 1.0));
        var info = line2.DistanceTo( pnt1 ) ;
        Assert.AreEqual( 1.0, info.Distance, delta ) ;
        AssertVec3Equal( new Vec3( 0.0, 0.0, 0.0 ), info.PointOnLine ) ;
        Assert.AreEqual( 0.0, info.Parameter, delta ) ;
      }
      {
        var line3 = new Line(new Vec3(0.0, 0.0, 0.0), new Vec3(1.0, 1.0, 1.0));
        var info = line3.DistanceTo( pnt1 ) ;
        Assert.AreEqual( System.Math.Sqrt( 6.0 ) / 3.0, info.Distance, delta ) ;
        AssertVec3Equal( new Vec3( 1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0 ), info.PointOnLine ) ;
        Assert.AreEqual( 1.0 / 3.0, info.Parameter, delta ) ;
      }
      {
        var line4 = new Line(new Vec3(0.0, 1.0, -1.0), new Vec3(0.0, 0.0, 1.0));
        var info = line4.DistanceTo( pnt1 ) ;
        Assert.AreEqual( System.Math.Sqrt( 2.0 ), info.Distance, delta ) ;
        AssertVec3Equal( new Vec3( 0.0, 1.0, 0.0 ), info.PointOnLine ) ;
        Assert.AreEqual( 1.0, info.Parameter, delta ) ;
      }
    }
    
    [Test]
    public void DistanceToToLine()
    {
      // TODO 後で確認
      // Line＆Line
      Line baseLine = new Line(new Vec3(1.0, 1.0, 1.0), new Vec3(1.0, 0.0, 0.0));
      {
        Line line1 = new Line(new Vec3(1.0, 0.0, 0.0), new Vec3(0.0, 0.0, 1.0));
        var info = baseLine.DistanceTo(line1);
        AssertVec3Equal(new Vec3(1.0, 1.0, 1.0), info.PointOnSelf);
        AssertVec3Equal(new Vec3(1.0, 0.0, 1.0), info.PointOnTarget);
        Assert.AreEqual(0.0, info.ParameterOnSelf, delta);
        Assert.AreEqual(1.0, info.ParameterOnTarget, delta);
        Assert.AreEqual(1.0, info.Distance, delta);
      }
      {
        Line line2 = new Line(new Vec3(0.0, 0.0, 0.0), new Vec3(0.0, 0.0, 1.0));
        var info = baseLine.DistanceTo(line2);
        AssertVec3Equal(new Vec3(0.0, 1.0, 1.0), info.PointOnSelf);
        AssertVec3Equal(new Vec3(0.0, 0.0, 1.0), info.PointOnTarget);
        Assert.AreEqual(-1.0, info.ParameterOnSelf, delta);
        Assert.AreEqual(1.0, info.ParameterOnTarget, delta);
        Assert.AreEqual(1.0, info.Distance, delta);
      }
      {
        Line line3 = new Line(new Vec3(0.0, 0.0, 0.0), new Vec3(1.0, 1.0, 1.0));
        var info = baseLine.DistanceTo(line3);
        AssertVec3Equal(new Vec3(1.0, 1.0, 1.0), info.PointOnSelf) ;
        AssertVec3Equal(new Vec3(1.0, 1.0, 1.0), info.PointOnTarget);
        Assert.AreEqual(0.0, info.ParameterOnSelf, delta);
        Assert.AreEqual(1.0, info.ParameterOnTarget, delta);
        Assert.AreEqual(0.0, info.Distance, delta);
      }
      {
        Line line4 = new Line(new Vec3(0.0, 1.0, 0.0), new Vec3(0.0, -1.0, 1.0));
        var info = baseLine.DistanceTo(line4);
        AssertVec3Equal(new Vec3(0.0, 1.0, 1.0), info.PointOnSelf);
        AssertVec3Equal(new Vec3(0.0, 0.5, 0.5), info.PointOnTarget);
        Assert.AreEqual(-1.0, info.ParameterOnSelf, delta);
        Assert.AreEqual(0.5, info.ParameterOnTarget, delta);
        Assert.AreEqual(System.Math.Sqrt(2.0) / 2.0, info.Distance, delta);
      }
      { // 並行のパタン
        Line line5parallel = new Line(new Vec3(-2.0, 1.0, 0.0), new Vec3(1.0, 0.0, 0.0));
        var info = baseLine.DistanceTo(line5parallel);
        AssertVec3Equal(baseLine.Origin, info.PointOnSelf);
        AssertVec3Equal(line5parallel.Origin, info.PointOnTarget);
        Assert.AreEqual(0.0, info.ParameterOnSelf, delta);
        Assert.AreEqual(0.0, info.ParameterOnTarget, delta);
        Assert.AreEqual(1.0, info.Distance, delta);
      }
      { // 含んでいるパタン
        Line line6include = new Line(new Vec3(0.0, 1.0, 1.0), new Vec3(1.0, 0.0, 0.0));
        var info = baseLine.DistanceTo( line6include) ;
        AssertVec3Equal( baseLine.Origin, info.PointOnSelf ) ;
        AssertVec3Equal( line6include.Origin, info.PointOnTarget ) ;
        Assert.AreEqual( 0.0, info.ParameterOnSelf, delta ) ;
        Assert.AreEqual( 0.0, info.ParameterOnTarget, delta ) ;
        Assert.AreEqual( 0.0, info.Distance, delta ) ;
      }
    }
  }
}