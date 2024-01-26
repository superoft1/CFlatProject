using System.Linq.Expressions ;
using Chiyoda.Math ;
using NUnit.Framework ;
using UnityEngine.Experimental.PlayerLoop ;

namespace Tests.MathTest.Editor
{
  public class FiniteLineTest:MathTestBase
  {
    [Test]
    public void Init()
    {
      var line = new FiniteLine( new Vec3( 1.0, 0.0, 2.0 ), new Vec3( 1.0, -3.5, 1.0 ) ) ;

      AssertVec3Equal( new Vec3( 1.0, 0.0, 2.0 ), line.From ) ;
      AssertVec3Equal( new Vec3( 1.0, -3.5, 1.0 ), line.To ) ;
    }

    [Test]
    public void ToStr()
    {
      var line = new FiniteLine( new Vec3( 1.0, 0.0, 2.0 ), new Vec3( 1.0, -3.5, 1.0 ) ) ;
      Assert.AreEqual( "FiniteLine[From[1,0,2],To[1,-3.5,1]]", line.ToString() );
    }

    [Test]
    public void Equals()
    {
      Assert.AreEqual( new FiniteLine( new Vec3( 1, 2, 3 ), new Vec3( 2, 3, 4 ) ),
                   new FiniteLine( new Vec3( 1, 2, 3 ), new Vec3( 2, 3, 4 ) ) ) ;

      Assert.AreNotEqual( new FiniteLine( new Vec3( 1, 2, 3 ), new Vec3( 2, 3, 4 ) ),
                   new FiniteLine( new Vec3( 1, 2, 3 ), new Vec3( 2, 3, 3 ) ) ) ;

      // FromとToが入れ替わっている場合は別のモノとする
      Assert.AreNotEqual( new FiniteLine( new Vec3( 1, 2, 3 ), new Vec3( 2, 3, 4 ) ),
                   new FiniteLine( new Vec3( 2, 3, 4 ), new Vec3( 1, 2, 3 ) ) ) ;
    }

    [Test]
    public void GetPointAt()
    {
      var line = new FiniteLine( new Vec3( 1.0, 1.0, 2.0 ), new Vec3( 2.0, 3.0, -4.0 ) ) ;
      AssertVec3Equal( new Vec3(1.0, 1.0,  2.0), line.GetPointAt(0.0));
      AssertVec3Equal( new Vec3(1.5, 2.0, -1.0),line.GetPointAt(0.5));
      AssertVec3Equal( new Vec3( 2.0, 3.0, -4.0 ), line.GetPointAt( 1.0 ) ) ;
      
      // 0～1以外なら、終点か始点を返す
      AssertVec3Equal( new Vec3( 1.0, 1.0, 2.0 ), line.GetPointAt( -1.0 ) ) ;
      AssertVec3Equal( new Vec3( 2.0, 3.0, -4.0 ), line.GetPointAt( 1.5 ) ) ;
    }

    [Test]
    public void Length()
    {
      Assert.AreEqual( System.Math.Sqrt( 8.0 ), new FiniteLine(new Vec3(0.0, 0.0, 2.0), new Vec3(2.0, 2.0, 2.0)).Length, delta) ;
    }

    [Test]
    public void DistanceToPoint()
    {
      // line実装後に
      
      /*
      var delta = Tol.Double ;
      var line = new FiniteLine( new Vec3( 0.0, 0.0, 2.0 ), new Vec3( 2.0, 2.0, 2.0 ) ) ;

      {
        var info = line.DistanceTo( new Vec3( 1.0, 1.0, -4.0 ) ) ;
        Assert.AreEqual( 6.0, info.Distance, delta ) ;
        Assert.AreEqual( new Vec3( 1.0, 1.0, 2.0 ), info.PointOnLine ) ;
        Assert.AreEqual( 0.5, info.Parameter, delta ) ;
      }
      
      {
        var info = line.DistanceTo( new Vec3( 1.0, 1.0, 2.0 ) ) ;
        Assert.AreEqual( 0.0, info.Distance, delta ) ;
        Assert.AreEqual( new Vec3( 1.0, 1.0, 2.0 ), info.PointOnLine ) ;
        Assert.AreEqual( 0.5, info.Parameter, delta ) ;
      }
      
      {
        var info = line.DistanceTo( new Vec3( 3.0, 3.0, 2.0) ) ;
        Assert.AreEqual( System.Math.Sqrt(2), info.Distance, delta ) ;
        Assert.AreEqual( new Vec3( 2.0, 2.0, 2.0 ) , info.PointOnLine ) ;
        Assert.AreEqual( 1.0, info.Parameter, delta ) ;
      }

      {
        var info = line.DistanceTo( new Vec3( 0.0, -1.0, 3.0) ) ;
        Assert.AreEqual( System.Math.Sqrt(2), info.Distance, delta ) ;
        Assert.AreEqual( new Vec3( 0.0, 0.0, 2.0) , info.PointOnLine ) ;
        Assert.AreEqual( 0.0, info.Parameter, delta ) ;
      }
      */
    }
  }
}