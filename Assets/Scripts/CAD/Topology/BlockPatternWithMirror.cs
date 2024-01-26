using System ;
using System.Collections.Generic ;
using System.ComponentModel ;
using System.Linq ;
using System.Text ;
using System.Threading.Tasks ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using MaterialUI ;
using UnityEngine ;

namespace Chiyoda.CAD.Topology
{
  [Entity( EntityType.Type.BlockPatternWithMirror )]
  public class BlockPatternWithMirror : CompositeBlockPattern
  {
    private readonly Memento<Vector3d> _arrayOffset ;

    public BlockPatternWithMirror( Document document ) : base( document )
    {
      _arrayOffset = new Memento<Vector3d>( this, new Vector3d( 1, 0, 0 ) ) ;

      RegisterDelayedProperty( "ArrayOffsetX", PropertyType.Length, GetOffsetX, SetOffsetX, null ) ;
      RegisterDelayedProperty( "ArrayOffsetY", PropertyType.Length, GetOffsetY, SetOffsetY, null ) ;
      RegisterDelayedProperty( "ArrayOffsetZ", PropertyType.Length, GetOffsetZ, SetOffsetZ, null ) ;
    }

    protected override void CopyFromImpl( CompositeBlockPattern compositeBlockPattern, CopyObjectStorage storage )
    {
      var array = compositeBlockPattern as BlockPatternWithMirror ;
      _arrayOffset.CopyFrom( array._arrayOffset.Value ) ;
    }

    public override int CopyCount => 2 ;

    public Vector3d ArrayOffset
    {
      get => _arrayOffset.Value ;
      set
      {
        if ( _arrayOffset.Value != value ) {
          _arrayOffset.Value = value ;
          RecreateJointEdges() ;
        }
      }
    }

    public double GetOffsetX() => ArrayOffset.x ;
    public double GetOffsetY() => ArrayOffset.y ;
    public double GetOffsetZ() => ArrayOffset.z ;

    public void SetOffsetX( double value )
    {
      var offset = ArrayOffset ;
      ArrayOffset = new Vector3d( value, offset.y, offset.z ) ;
    }

    public void SetOffsetY( double value )
    {
      var offset = ArrayOffset ;
      ArrayOffset = new Vector3d( offset.x, value, offset.z ) ;
    }

    public void SetOffsetZ( double value )
    {
      var offset = ArrayOffset ;
      ArrayOffset = new Vector3d( offset.x, offset.y, value ) ;
    }

    protected override LocalCodSys3d CreateCopyLocalCod( LocalCodSys3d baseCod, int index )
    {
      return new LocalCodSys3d( baseCod.Origin + _arrayOffset.Value * index, baseCod.MirrorXYBy( baseCod.Origin, _arrayOffset.Value ) ) ;
    }

    public override void Mirror( in Vector3d origin, in Vector3d normalDirection )
    {
      base.Mirror( origin, normalDirection ) ;

      ArrayOffset = ArrayOffset.MirrorVectorBy( normalDirection ) ;
    }
  }
}