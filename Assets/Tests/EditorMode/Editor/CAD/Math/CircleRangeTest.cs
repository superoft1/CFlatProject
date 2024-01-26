using CAD.Math ;
using NUnit.Framework ;
using Assert = UnityEngine.Assertions.Assert;

namespace Tests.EditorMode.Editor.CAD.Math
{
  class CircleTest
  {
    [Test]
    public void SimplePasses()
    {
      var range = new CircleRange( 0, 360 ) ;
      Assert.IsTrue( range.IsInside( 180 ) ) ;
    }

    [Test]
    public void SimplePasses2()
    {
      var range = new CircleRange( 0, 180 ) ;
      Assert.IsTrue( range.IsInside( 180 ) ) ;
      Assert.IsTrue( range.IsInside( 170 ) ) ;
      Assert.IsFalse( range.IsInside( 190 ) ) ;
      Assert.IsFalse( range.IsInside( -90 ) ) ;
      Assert.IsTrue( range.IsInside( -200 ) ) ;
    }

    [Test]
    public void SimplePasses3()
    {
      var range = new CircleRange( -60, 60 ) ;
      Assert.IsTrue( range.IsInside( 0 ) ) ;
      Assert.IsTrue( range.IsInside( -60 ) ) ;
      Assert.IsFalse( range.IsInside( -70 ) ) ;
      Assert.IsTrue( range.IsInside( 60 ) ) ;
      Assert.IsFalse( range.IsInside( 70 ) ) ;
      Assert.IsTrue( range.IsInside( 305 ) ) ;
      Assert.IsTrue( range.IsInside( 720 ) ) ;
    }

    [Test]
    public void SimplePasses4()
    {
      var range = new CircleRange( -60, 60 ) ;
      Assert.AreApproximatelyEqual( 0, (float)range.Clamp( true,0 )) ;
      Assert.AreApproximatelyEqual( 310, (float)range.Clamp(true, -50 )) ;
      Assert.AreApproximatelyEqual( 300, (float)range.Clamp( true,-60 )) ;
      Assert.AreApproximatelyEqual( 60, (float)range.Clamp(true, -70 )) ;
      Assert.AreApproximatelyEqual( 0, (float)range.Clamp( true,0 )) ;
      Assert.AreApproximatelyEqual( 50, (float)range.Clamp( true,50 )) ;
      Assert.AreApproximatelyEqual( 60, (float)range.Clamp( true,60 )) ;
      Assert.AreApproximatelyEqual( 60, (float)range.Clamp( true,70 )) ;
      Assert.AreApproximatelyEqual( 0, (float)range.Clamp( true,360 )) ;
    }
    
    [Test]
    public void SimplePasses5()
    {
      var range = new CircleRange( -60, 60 ) ;
      Assert.AreApproximatelyEqual( 0, (float)range.Clamp(false, 0 )) ;
      Assert.AreApproximatelyEqual( 310, (float)range.Clamp(false, -50 )) ;
      Assert.AreApproximatelyEqual( 300, (float)range.Clamp(false, -60 )) ;
      Assert.AreApproximatelyEqual( 300, (float)range.Clamp(false, -70 )) ;
      Assert.AreApproximatelyEqual( 0, (float)range.Clamp( false,0 )) ;
      Assert.AreApproximatelyEqual( 50, (float)range.Clamp(false, 50 )) ;
      Assert.AreApproximatelyEqual( 60, (float)range.Clamp( false,60 )) ;
      Assert.AreApproximatelyEqual( 300, (float)range.Clamp( false,70 )) ;
      Assert.AreApproximatelyEqual( 0, (float)range.Clamp( false,360 )) ;
    }
  }
}