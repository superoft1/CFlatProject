using Chiyoda.Math ;
using NUnit.Framework ;

namespace Tests.MathTest.Editor
{
  public class MathTestBase
  {
    protected static double delta => Tol.Double ;
    
    // 許容範囲内を指定して比較
    protected static void AssertVec3Equal(Vec3 expected, Vec3 actual)
    { 
      Assert.AreEqual(expected.X, actual.X, delta);
      Assert.AreEqual(expected.Y, actual.Y, delta);
      Assert.AreEqual(expected.Z, actual.Z, delta);
    }
    
  }
}