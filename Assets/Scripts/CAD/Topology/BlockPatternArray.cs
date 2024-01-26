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
  [Entity( EntityType.Type.BlockPatternArray )]
  public class BlockPatternArray : CompositeBlockPattern
  {
    private readonly Memento<bool> _countEditable ;
    private readonly Memento<int> _arrayCount ;
    private readonly Memento<Vector3d> _arrayOffset ;

    public BlockPatternArray( Document document ) : base( document )
    {
      _countEditable = new Memento<bool>( this, true ) ;
      _arrayCount = new Memento<int>( this, 1 ) ;
      _arrayOffset = new Memento<Vector3d>( this, new Vector3d( 1, 0, 0 ) ) ;

      RegisterDelayedProperty( "ArrayCount", PropertyType.GeneralInteger, GetArrayCount, SetArrayCount, null ) ;
      RegisterDelayedProperty( "ArrayOffsetX", PropertyType.Length, GetOffsetX, SetOffsetX, null ) ;
      RegisterDelayedProperty( "ArrayOffsetY", PropertyType.Length, GetOffsetY, SetOffsetY, null ) ;
      RegisterDelayedProperty( "ArrayOffsetZ", PropertyType.Length, GetOffsetZ, SetOffsetZ, null ) ;
    }

    protected override void CopyFromImpl( CompositeBlockPattern compositeBlockPattern, CopyObjectStorage storage )
    {
      var array = compositeBlockPattern as BlockPatternArray ;
      _countEditable.CopyFrom( array._countEditable.Value ) ;
      _arrayCount.CopyFrom( array._arrayCount.Value ) ;
      _arrayOffset.CopyFrom( array._arrayOffset.Value ) ;
    }

    public bool CountEditable
    {
      get => _countEditable.Value ;
      set => _countEditable.Value = value ;
    }

    public int ArrayCount
    {
      get => _arrayCount.Value ;
      set
      {
        if ( value < 1 ) value = 1 ;
        if ( _arrayCount.Value != value ) {
          _arrayCount.Value = value ;
          RecreateJointEdges() ;
        }
      }
    }

    public override int CopyCount => ArrayCount ;

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

    private double GetArrayCount() => ArrayCount ;
    private void SetArrayCount( double value ) => ArrayCount = value.ToInteger() ;

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
      return new LocalCodSys3d( baseCod.Origin + _arrayOffset.Value * index, baseCod ) ;
    }

    public override void Mirror( in Vector3d origin, in Vector3d normalDirection )
    {
      base.Mirror( origin, normalDirection ) ;

      ArrayOffset = ArrayOffset.MirrorVectorBy( normalDirection ) ;
    }
  }
}