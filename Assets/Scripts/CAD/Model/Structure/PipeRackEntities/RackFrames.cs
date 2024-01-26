using System ;
using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using UnityEngine ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal class RackFrames : MemorableObjectBase
  {
    private readonly ITransverseFrame _baseFrame ;
    private readonly MementoList<ITransverseFrame> _subFrames ;

    public Action FloorNumberChanged ;

    public Action<IList<ITransverseFrame>> FrameExpanded ;
    public Action<int> FrameCurtailed ;

    public event EventHandler WeightChanged ;
    
    public RackFrames( History h, ITransverseFrame baseFrame )
    {
      History = h ;

      FloorNumberChanged = () => { } ;
      FrameExpanded = f => { } ;
      FrameCurtailed = i => { } ;
      
      baseFrame.PositionOffset = 0.0 ;
      baseFrame.WeightChanged += ( s, e ) => WeightChanged?.Invoke( s, e ) ;
      _baseFrame = baseFrame ;
      
      _subFrames = CreateMementoListAndSetupValueEvents<ITransverseFrame>();
      
      var next = _baseFrame.CreateNextFrame() ;
      AttachEvents( next );
      _subFrames.Add( next ) ;
    }

    public void CopyFloorProperties( RackFrames src )
    {
      Width = src.Width ;

      var floorNum = Math.Min( FloorNumber, src.FloorNumber ) ;
      foreach ( var floor in Enumerable.Range( 0, floorNum ) ) {
        _baseFrame[ floor ].CopyFrom( src._baseFrame[ floor ] ) ;
      }
    }
   
    public override History History { get ; }

    public void Expand( int num )
    {
      var nextFrames = Enumerable.Range( 0, num ).Select( i =>
      {
        var f = _subFrames.Last().CreateNextFrame() ;
        AttachEvents( f );
        return f ;
      } ).ToList() ;
      _subFrames.AddRange( nextFrames ) ;
      FrameExpanded?.Invoke( nextFrames );
    }

    public void CurtailFrames( int curtailingNum )
    {
      var num = Math.Min( _subFrames.Count - 1, curtailingNum ) ;
      _subFrames.RemoveRange( _subFrames.Count - num, num ) ;
      FrameCurtailed?.Invoke( num ) ;
    }

    public (double steel, double rc) PhysicalProperties
      => ( ( 1.0 + _subFrames.Count ) * _baseFrame.CalcSteelWeight(), 
           ( 1.0 * _subFrames.Count ) * _baseFrame.RcVolume() ) ;
    
    public double Width
    {
      get => _baseFrame.Width ;
      set
      {
        if ( Math.Abs( Width - value ) < Tolerance.DoubleEpsilon ) {
          return ;
        }
        _baseFrame.Width = value ;
      }
    }

    public double PositionOffset
    {
      get => _subFrames.First().PositionOffset ;
      set {
        var target = _subFrames.Where( s => Math.Abs( s.PositionOffset - value ) > Tolerance.DoubleEpsilon  )
            .ToArray();
        if ( !target.Any() ) {
          return ;
        }
        target.Take( target.Length - 1 ).ForEach( f => f.PositionOffset = value ) ;
        using ( _subFrames.Last().ActivateWeightChangedEvent() ) {
          _subFrames.Last().PositionOffset = value ; // 最後に１回だけイベント発火
        }
      }
    }

    public int FloorNumber
    {
      get => _baseFrame.FloorNumber ;
      set
      {
        _baseFrame.ChangeFloorNumber( value ) ;
        FloorNumberChanged?.Invoke();
      }
    }
    
    
    public void SetLayerBeamMaterial( int layer, IStructuralMaterial material )
    {
      using ( _baseFrame[ layer ].ActivateWeightChangedEvent() ) {
        _baseFrame[ layer ].BeamMaterial = material.CreateCopy() ;
      }
    }
    
    public void SetLayerColumnMaterial( int layer, IStructuralMaterial material )
    {
      using ( _baseFrame[ layer ].ActivateWeightChangedEvent() ) {
        _baseFrame[ layer ].ColumnMaterial = material.CreateCopy() ;
      }
    }
    
    public IStructuralMaterial BeamMaterial
    {
      get => _baseFrame.Items.First().BeamMaterial ;
      set
      {
        Enumerable.Range( 0, FloorNumber - 1 )
          .ForEach( i => _baseFrame[ i ].BeamMaterial = value.CreateCopy() ) ;
        SetLayerBeamMaterial( FloorNumber-1, value );
      }
    }

    public IStructuralMaterial ColumnMaterial
    {
      get => _baseFrame.Items.First().ColumnMaterial ;
      set
      {
        Enumerable.Range( 0, FloorNumber - 1 )
          .ForEach( i =>_baseFrame[ i ].ColumnMaterial = value.CreateCopy() ) ;
        SetLayerColumnMaterial( FloorNumber-1, value );
      }
    }

    public int FrameCount => _subFrames.Count + 1 ;
    public ITransverseFrame this[ int index ] 
      => ( index == 0 ) ? _baseFrame : _subFrames[ index - 1 ] ;
    public ITransverseFrame TailFrame => _subFrames.Last() ;

    public IEnumerable<IStructurePart> CreateElements()
    {
      foreach ( var e in _baseFrame.Elements ) {
        yield return e ;
      }

      var offset = 0.0 ;
      foreach ( var f in _subFrames ) {
        offset += f.PositionOffset ;
        var offsetVec = new Vector3d( 0.0, offset, 0.0 ) ;
        foreach ( var b in f.Elements ) {
          b.LocalCod = b.LocalCod.Translate( offsetVec ) ;
          yield return b ;
        }
      }
    }
    
    public double HeightFromGround( int layer ) => _baseFrame.HeightFromGround( layer ) ;
    public void SetFloorHeight( int layer, double value )
    {
      using ( _baseFrame[ layer ].ActivateWeightChangedEvent() ) {
        _baseFrame[ layer ].Height = value ;
      }
    }
    
    private void AttachEvents( ITransverseFrame frame )
    {
      frame.WeightChanged += ( s, e ) => WeightChanged( s, e ) ;
    }
  }
}