using NUnit.Framework;
using Chiyoda.Math;

namespace Tests.CableRouting
{
  public class CableRoutingMgrTest
  {
    [Test]
    public void Execute()
    {
      var vecZero = Vec3.zero ;
      Assert.AreEqual( 0.0, vecZero.X, 1e-10 );
      Assert.AreEqual( 0.0, vecZero.Y, 1e-10 );
      Assert.AreEqual( 0.0, vecZero.Z, 1e-10 );
    }
  }
}