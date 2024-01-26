using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine
{
  public readonly struct Box3d
  {
    public static Box3d Null { get; } = new Box3d( false );

    public Vector3d Min { get; }
    public Vector3d Max { get; }

    public bool IsNull
    {
      get => (Min.x > Max.x);
    }

    public Box3d( Vector3d v1, Vector3d v2 )
    {
      Min = new Vector3d( Math.Min( v1.x, v2.x ), Math.Min( v1.y, v2.y ), Math.Min( v1.z, v2.z ) );
      Max = new Vector3d( Math.Max( v1.x, v2.x ), Math.Max( v1.y, v2.y ), Math.Max( v1.z, v2.z ) );
    }

    public Box3d( Vector3d v )
    {
      Min = v;
      Max = v;
    }

    private Box3d( bool dummy )
    {
      // Nullを作るためのダミーコンストラクタ
      Min = new Vector3d( double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity );
      Max = new Vector3d( double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity );
    }

    public Box3d (Box3d box1, Vector3d v)
    {
      Min = new Vector3d(Math.Min(box1.Min.x, v.x), Math.Min(box1.Min.y, v.y), Math.Min(box1.Min.z, v.z));
      Max = new Vector3d(Math.Max(box1.Max.x, v.x), Math.Max(box1.Max.y, v.y), Math.Max(box1.Max.z, v.z));
    }

    public double XWidth { get { return Max.x - Min.x; } }
    public double YWidth { get { return Max.y - Min.y; } }
    public double ZWidth { get { return Max.z - Min.z; } } 
  }
}
