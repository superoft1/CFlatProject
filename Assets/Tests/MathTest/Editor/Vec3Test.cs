using Chiyoda.Math ;
using NUnit.Framework ;

namespace Tests.MathTest.Editor
{
  public class Vec3Test:MathTestBase
  {
    [Test]
    public void Init()
    {
      {
        var zero = Vec3.zero ;
        Assert.AreEqual( 0.0, zero.X, delta );
        Assert.AreEqual( 0.0, zero.Y, delta );
        Assert.AreEqual( 0.0, zero.Z, delta );
      }
      {
        var vec = new Vec3(1.0,2.0,3.0);
        Assert.AreEqual( 1.0, vec.X, delta );
        Assert.AreEqual( 2.0, vec.Y, delta );
        Assert.AreEqual( 3.0, vec.Z, delta );
      }
    }

    [Test]
    public void ToStr()
    {
      var vec = new Vec3(1.0,2.0,3.0);
      Assert.AreEqual( "Vec[1,2,3]", vec.ToString() );      
    }

    [Test]
    public void AssignValue()
    {
      var vec = new Vec3(1.0,2.0,3.0);

      vec.X = 2.0 ;
      vec.Y *= 2.0 ;
      vec.Z -= 3.0 ;

      Assert.AreEqual( 2.0, vec.X, delta ) ;
      Assert.AreEqual( 4.0, vec.Y, delta ) ;
      Assert.AreEqual( 0.0, vec.Z, delta ) ;
    }

    [Test]
    public void Equals()
    {
      var vec1 = new Vec3(1.0,2.0,3.0);
      var vec2 = new Vec3(1.0,2.0,3.0);
      var vec3 = new Vec3(1.0,2.0,3.0 + 1e-10);
      
      Assert.True( vec1 == vec2 );
      Assert.True( vec1 != vec3 );

      Assert.True( vec1.Equals( vec2 ) ) ;
      Assert.True( !vec1.Equals( vec3 ) ) ;
    }

    [Test]
    public void Add()
    {
      var vec1 = new Vec3(1.0,2.0,3.0);
      var vec2 = new Vec3(4.0,5.0,6.0);

      var added = vec1 + vec2 ;

      Assert.AreEqual( 5.0, added.X, delta ) ;
      Assert.AreEqual( 7.0, added.Y, delta ) ;
      Assert.AreEqual( 9.0, added.Z, delta ) ;

      added += vec1 ;
      Assert.AreEqual( 6.0, added.X, delta ) ;
      Assert.AreEqual( 9.0, added.Y, delta ) ;
      Assert.AreEqual( 12.0, added.Z, delta ) ;
    }
    
    [Test]
    public void Sub()
    {
      var vec1 = new Vec3(1.0,2.0,3.0);
      var vec2 = new Vec3(4.0,5.0,6.0);

      var added = vec2 - vec1 ;

      Assert.AreEqual( 3.0, added.X, delta ) ;
      Assert.AreEqual( 3.0, added.Y, delta ) ;
      Assert.AreEqual( 3.0, added.Z, delta ) ;

      added -= vec2 ;
      Assert.AreEqual( -1.0, added.X, delta ) ;
      Assert.AreEqual( -2.0, added.Y, delta ) ;
      Assert.AreEqual( -3.0, added.Z, delta ) ;

      var minus = -vec1 ;
      Assert.AreEqual( -1.0, minus.X, delta ) ;
      Assert.AreEqual( -2.0, minus.Y, delta ) ;
      Assert.AreEqual( -3.0, minus.Z, delta ) ;
    }

    [Test]
    public void Multi()
    {
      var vec = new Vec3(1.0,2.0,3.0);
      {
        var multi = vec * 5 ;

        Assert.AreEqual( 5.0, multi.X, delta ) ;
        Assert.AreEqual( 10.0, multi.Y, delta ) ;
        Assert.AreEqual( 15.0, multi.Z, delta ) ;
      }

      {
        var multi = -3.0 * vec ;

        Assert.AreEqual( -3.0, multi.X, delta ) ;
        Assert.AreEqual( -6.0, multi.Y, delta ) ;
        Assert.AreEqual( -9.0, multi.Z, delta ) ;
      }
    }

    [Test]
    public void Devide()
    {
      var vec = new Vec3(1.0,2.0,3.0);
      {
        var dev = vec / 5 ;

        Assert.AreEqual( 1.0/5.0, dev.X, delta ) ;
        Assert.AreEqual( 2.0/5.0, dev.Y, delta ) ;
        Assert.AreEqual( 3.0/5.0, dev.Z, delta ) ;
      }
    }

    [Test]
    public void Dot()
    {
      var vec1 = new Vec3( 1.0, 2.0, 3.0 ) ;
      var vec2 = new Vec3( 2.0, 4.0, 6.0 ) ;

      {
        var innerProduct = vec1.Dot( vec2 ) ;
        Assert.AreEqual( 28.0, innerProduct, delta ) ;
      }

      {
        var innerProduct = vec1.Dot( Vec3.zero ) ;
        Assert.AreEqual( 0.0, innerProduct, delta ) ;
      }
    }

    [Test]
    public void Cross()
    {
      {
        var outerProduct = ( new Vec3( 1.0, 0.0, 0.0 ) ).Cross( new Vec3( 0.0, 1.0, 0.0 ) ) ;
        Assert.AreEqual( 0.0, outerProduct.X, delta ) ;
        Assert.AreEqual( 0.0, outerProduct.Y, delta ) ;
        Assert.AreEqual( 1.0, outerProduct.Z, delta ) ;
      }
      {
        var outerProduct = ( new Vec3( 1.0, 2.0, 3.0 ) ).Cross( new Vec3( 1.0, 1.0, 4.0 ) ) ;
        Assert.AreEqual(  5.0, outerProduct.X, delta ) ;
        Assert.AreEqual( -1.0, outerProduct.Y, delta ) ;
        Assert.AreEqual( -1.0, outerProduct.Z, delta ) ;
      }
      {
        var outerProduct = ( new Vec3( 1.0, 2.0, 3.0 ) ).Cross( Vec3.zero ) ;
        Assert.AreEqual( 0.0, outerProduct.X, delta ) ;
        Assert.AreEqual( 0.0, outerProduct.Y, delta ) ;
        Assert.AreEqual( 0.0, outerProduct.Z, delta ) ;
      }
    }

    [Test]
    public void Length()
    {
      Assert.AreEqual( 0.0, Vec3.zero.Length, delta ) ;
      Assert.AreEqual( System.Math.Sqrt( 14.0 ), ( new Vec3( 1.0, 2.0, 3.0 ) ).Length, delta ) ;
      Assert.AreEqual( 5.0, ( new Vec3( 3.0, 4.0,  0.0 ) ).Length, delta ) ;
    }
    
    [Test]
    public void Distance()
    {
      var vec1 = new Vec3( 1.0, 3.0, -5.0 ) ;
      var vec2 = new Vec3( 4.0, 7.0,  -5.0 );
      Assert.AreEqual( 5.0, vec2.DistanceTo( vec1 ), delta ) ;
      Assert.AreEqual( 5.0, vec1.DistanceTo( vec2 ), delta ) ;
      Assert.AreEqual( 0.0, vec1.DistanceTo( vec1 ), delta ) ;
    }

    [Test]
    public void Angle()
    {
      var vec1 = new Vec3( 1.0, 0.0, 0.0 ) ;
      var vec2 = new Vec3( 1.0, 1.0, 0.0 ) ;
      var vec3 = new Vec3( -1.0 ,  1.0, 0.0 ) ;
      var vec4 = new Vec3( -1.0 ,  0.0, 0.0 ) ;
      var vec5 = new Vec3( -1.0 , -1.0, 0.0 ) ;
      var vec6 = new Vec3(  1.0 , -1.0, 0.0 ) ;

      // result should be between 0 and PI.
      Assert.AreEqual(0.0,vec1.Angle(vec1), delta) ;
      Assert.AreEqual(System.Math.PI/4.0,vec1.Angle(vec2), delta) ;
      Assert.AreEqual(System.Math.PI*3.0/4.0, vec1.Angle(vec3), delta) ;
      Assert.AreEqual(System.Math.PI, vec1.Angle(vec4), delta) ;
      Assert.AreEqual(System.Math.PI*3.0/4.0, vec1.Angle(vec5), delta) ;
      Assert.AreEqual(System.Math.PI/4.0, vec1.Angle(vec6), delta) ;
    }

    [Test]
    public void IsParallelTo()
    {
      var vecZero = Vec3.zero ;
      var vec1    = new Vec3( 1.0, 2.0, 3.0 ) ;
      var vec2    = new Vec3( 1.0, 1.0, 4.0 ) ;
      var vec3    = new Vec3( 2.0, 4.0, 6.0 ) ;
      var vec4    = new Vec3( -1.0, -2.0, -3.0 ) ;
      var vec5    = new Vec3( -3.0, -6.0, -9.0 ) ;

      Assert.False(vec1.IsParallelTo(vecZero)) ;
      Assert.False(vecZero.IsParallelTo(vec2)) ;
      Assert.False(vec1.IsParallelTo(vec2)) ;

      Assert.True(vec1.IsParallelTo(vec3)) ;
      Assert.True(vec1.IsParallelTo(vec4)) ;
      Assert.True(vec1.IsParallelTo(vec5)) ;
    }
    
    [Test]
    public void IsSameDirectionTo()
    {
      var vecZero = Vec3.zero ;
      var vec1    = new Vec3( 1.0, 2.0, 3.0 ) ;
      var vec2    = new Vec3( 1.0, 1.0, 4.0 ) ;
      var vec3    = new Vec3( 2.0, 4.0, 6.0 ) ;
      var vec4    = new Vec3( -1.0, -2.0, -3.0 ) ;
      var vec5    = new Vec3( -3.0, -6.0, -9.0 ) ;
      
      Assert.False(vec1.IsSameDirectionTo(vecZero)) ;
      Assert.False(vecZero.IsSameDirectionTo(vec2)) ;
      Assert.False(vec1.IsSameDirectionTo(vec2)) ;

      Assert.True(vec1.IsSameDirectionTo(vec3)) ;
      Assert.False(vec1.IsSameDirectionTo(vec4)) ;
      Assert.False(vec1.IsSameDirectionTo(vec5)) ;
    }

    [Test]
    public void Normalized()
    {
      var vec = new Vec3( 1.0, 2.0, 4.0 ) ;
      var normalized = vec.Normalized;
      Assert.AreEqual(1.0, normalized.Length, Tol.Double) ;
      Assert.True( normalized.IsSameDirectionTo( vec ) ) ;
  
      // zero div 処理
//      Assert.True(Vec3.zero == Vec3.zero.Normalized) ;
    }

    [Test]
    public void ProjectTo()
    {
      var vec1 = new Vec3( 1.0, 1.0, 0.0 ) ;
      var vec2 = new Vec3( 0.0, 1.0, 0.0 ) ;
      var vec3 = new Vec3( 2.0, 2.0, 0.0 ) ;
      {
        var info = vec2.ProjectTo( vec1 ) ;
        Assert.AreEqual(new Vec3(0.5,0.5,0.0), info.Point );
        Assert.AreEqual( 0.5, info.Parmeter, delta );
      }
      {
        var info = vec1.ProjectTo( vec2 ) ;
        Assert.AreEqual(new Vec3(0.0,1.0,0.0), info.Point );
        Assert.AreEqual( 1.0, info.Parmeter, delta );
      }
      {
        var info = vec3.ProjectTo( vec2 ) ;
        Assert.AreEqual(new Vec3(0.0,2.0,0.0), info.Point );
        Assert.AreEqual( 2.0, info.Parmeter, delta );
      }
      {// project zero vector
        var info = vec2.ProjectTo( Vec3.zero ) ;
        Assert.AreEqual(Vec3.zero, info.Point );
        Assert.AreEqual( 0.0, info.Parmeter, delta );
      }
      {// project to self
        var info = vec1.ProjectTo( vec1 ) ;
        Assert.AreEqual(new Vec3(1.0,1.0,0.0), info.Point );
        Assert.AreEqual( 1.0, info.Parmeter, delta );
      }
    }
  }
}