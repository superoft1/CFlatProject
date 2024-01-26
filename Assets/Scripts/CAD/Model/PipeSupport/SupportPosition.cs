using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using MaterialUI ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  /// <summary>
  /// サポート座標クラス。
  /// </summary>
  public abstract class SupportPositionBase
  {
    /// <summary>
    /// サポート座標と関連するサポート。
    /// </summary>
    public Support Support { get ; }

    protected SupportPositionBase( Support support )
    {
      Support = support ;
    }

    /// <summary>
    /// サポート座標が新たに変更された際のイベント
    /// </summary>
    public event EventHandler NewlyPositionChanged ;

    /// <summary>
    /// サポート座標が履歴操作により変更された際のイベント
    /// </summary>
    public event EventHandler HistoricallyPositionChanged ;

    /// <summary>
    /// サポート座標 (直近<see cref="ISupportParentElement"/>=BlockPatter or Documentに対する相対座標)。
    /// </summary>
    public abstract Vector3d Position { get ; set ; }

    /// <summary>
    /// サポート対象エッジ。
    /// </summary>
    public abstract LeafEdge Target { get ; set ; }

    /// <summary>
    /// 何らかの理由でサポート座標が変化した際、必ず呼ばれなければならないメソッド。
    /// </summary>
    protected virtual void OnNewlyPositionChanged()
    {
      NewlyPositionChanged?.Invoke( this, EventArgs.Empty ) ;
    }

    protected virtual void OnHistoricallyPositionChanged()
    {
      HistoricallyPositionChanged?.Invoke( this, EventArgs.Empty ) ;
    }

    public abstract void Mirror( in Vector3d origin, in Vector3d normalDirection ) ;
  }



  /// <summary>
  /// 絶対座標でサポートするサポート座標クラス
  /// </summary>
  public class AbsoluteSupportPosition : SupportPositionBase
  {
    private readonly Memento<Vector3d> _position ;

    public override Vector3d Position
    {
      get => _position.Value ;
      set => _position.Value = value ;
    }

    public override LeafEdge Target
    {
      get => null ;
      set
      {
        if ( value != null ) throw new InvalidOperationException() ;
      }
    }

    public AbsoluteSupportPosition( Support support ) : base( support )
    {
      _position = new Memento<Vector3d>( support ) ;
      _position.AfterNewlyValueChanged += ( sender, e ) => OnNewlyPositionChanged() ;
      _position.AfterHistoricallyValueChanged += ( sender, e ) => OnHistoricallyPositionChanged() ;
    }

    public override void Mirror( in Vector3d origin, in Vector3d normalDirection )
    {
      Position = Position.MirrorPointBy( origin, normalDirection ) ;
    }
  }



  /// <summary>
  /// PipingPieceに対する相対座標でサポートするサポート座標クラス。
  /// </summary>
  public class RelativeSupportPosition : SupportPositionBase
  {
    private readonly Memento<Vector3d> _relativePosition ;
    private readonly Memento<LeafEdge> _target ;
    private Vector3d _lastPosition ;

    public override LeafEdge Target
    {
      get => _target.Value ;
      set => _target.Value = value ;
    }

    public Vector3d RelativePosition
    {
      get => _relativePosition.Value ;
      set => _relativePosition.Value = value ;
    }

    public override Vector3d Position
    {
      get => _lastPosition ;
      set => SetPosition( value ) ;
    }

    public RelativeSupportPosition( Support support ) : base( support )
    {
      _relativePosition = new Memento<Vector3d>( support ) ;
      _relativePosition.AfterNewlyValueChanged += ( sender, e ) => CheckNewlyPositionChanged() ;
      _relativePosition.AfterHistoricallyValueChanged += ( sender, e ) => CheckHistoricallyPositionChanged() ;

      _target = new Memento<LeafEdge>( support ) ;
      _target.AfterValueChanged += ( sender, e ) =>
      {
        if ( null != e.OldValue ) {
          e.OldValue.NewlyLocalCodChanged -= LeafEdge_NewlyEdgeMoved ;
          e.OldValue.HistoricallyLocalCodChanged -= LeafEdge_HistoricallyEdgeMoved ;
          e.OldValue.AfterNewlyChildrenChanged -= LeafEdge_NewlyChildrenChanged ;
          e.OldValue.AfterHistoricallyChildrenChanged -= LeafEdge_HistoricallyChildrenChanged ;
          if ( e.OldValue.PipingPiece is Pipe pipe ) {
            pipe.AfterNewlyValueChanged -= Pipe_NewlyValueChanged ;
            pipe.AfterHistoricallyValueChanged -= Pipe_HistoricallyValueChanged ;
          }
        }

        if ( null != e.NewValue ) {
          e.NewValue.NewlyLocalCodChanged += LeafEdge_NewlyEdgeMoved ;
          e.NewValue.HistoricallyLocalCodChanged += LeafEdge_HistoricallyEdgeMoved ;
          e.NewValue.AfterNewlyChildrenChanged += LeafEdge_NewlyChildrenChanged ;
          e.NewValue.AfterHistoricallyChildrenChanged += LeafEdge_HistoricallyChildrenChanged ;
          if ( e.NewValue.PipingPiece is Pipe pipe ) {
            pipe.AfterNewlyValueChanged += Pipe_NewlyValueChanged ;
            pipe.AfterHistoricallyValueChanged += Pipe_HistoricallyValueChanged ;
          }
        }
      } ;
      _target.AfterNewlyValueChanged += ( sender, e ) => CheckNewlyPositionChanged() ;
      _target.AfterHistoricallyValueChanged += ( sender, e ) => CheckHistoricallyPositionChanged() ;
    }

    private void CheckNewlyPositionChanged()
    {
      var newPosition = CalcPosition() ;
      if ( ! MementoEqualityComparer<Vector3d>.Equals( _lastPosition, newPosition ) ) {
        _lastPosition = newPosition ;
        OnNewlyPositionChanged() ;
      }
    }

    private void CheckHistoricallyPositionChanged()
    {
      _lastPosition = CalcPosition() ;
      OnHistoricallyPositionChanged() ;
    }

    private Vector3d CalcPosition()
    {
      if ( null == Target ) return Vector3d.zero ;

      var pos = _relativePosition.Value ;
      for ( IElement elm = Target ; ( elm is Edge edge ) && ! ( elm is ISupportParentElement ) ; elm = elm.Parent ) {
        pos = edge.LocalCod.GlobalizePoint( pos ) ;
      }

      return pos ;
    }

    private void SetPosition( Vector3d position )
    {
      var pp = Target?.PipingPiece ;
      if ( null == Target?.PipingPiece ) throw new InvalidOperationException() ;

      var stack = new Stack<LocalCodSys3d>() ;
      for ( IElement elm = Target ; ( elm is Edge edge ) && ! ( elm is ISupportParentElement ) ; elm = elm.Parent ) {
        stack.Push( edge.LocalCod ) ;
      }

      while ( 0 < stack.Count ) {
        position = stack.Pop().LocalizePoint( position ) ;
      }

      _relativePosition.Value = position ;
    }

    private void LeafEdge_NewlyChildrenChanged( object sender, ItemChangedEventArgs<IElement> e )
    {
      LeafEdge_ChildrenChanged( e, CheckNewlyPositionChanged ) ;
    }

    private void LeafEdge_HistoricallyChildrenChanged( object sender, ItemChangedEventArgs<IElement> e )
    {
      LeafEdge_ChildrenChanged( e, CheckHistoricallyPositionChanged ) ;
    }

    private void LeafEdge_ChildrenChanged( ItemChangedEventArgs<IElement> e, Action afterPipeAdded )
    {
      bool pipeAdded = false ;
      e.RemovedItems.OfType<Pipe>().ForEach( pipe =>
      {
        pipe.AfterNewlyValueChanged -= Pipe_NewlyValueChanged ;
        pipe.AfterHistoricallyValueChanged -= Pipe_HistoricallyValueChanged ;
      } ) ;
      e.AddedItems.OfType<Pipe>().ForEach( pipe =>
      {
        pipe.AfterNewlyValueChanged += Pipe_NewlyValueChanged ;
        pipe.AfterHistoricallyValueChanged += Pipe_HistoricallyValueChanged ;
        pipeAdded = true ;
      } ) ;
      if ( pipeAdded ) {
        afterPipeAdded() ;
      }
    }

    private void LeafEdge_NewlyEdgeMoved( object sender, EventArgs e )
    {
      CheckNewlyPositionChanged() ;
    }

    private void LeafEdge_HistoricallyEdgeMoved( object sender, EventArgs e )
    {
      CheckHistoricallyPositionChanged() ;
    }

    private void Pipe_NewlyValueChanged( object sender, EventArgs e )
    {
      CheckNewlyPositionChanged() ;
    }

    private void Pipe_HistoricallyValueChanged( object sender, EventArgs e )
    {
      CheckHistoricallyPositionChanged() ;
    }

    public override void Mirror( in Vector3d origin, in Vector3d normalDirection )
    {
      RelativePosition = RelativePosition.MirrorVectorBy( normalDirection ) ;
    }
  }



  /// <summary>
  /// 特定の直管パイプをサポートするサポート座標クラス。
  /// </summary>
  public class PipeSupportPosition : SupportPositionBase
  {
    private readonly Memento<double> _positionParam ;
    private readonly Memento<LeafEdge> _target ;
    private Vector3d _lastPosition ;

    public override LeafEdge Target
    {
      get => _target.Value ;
      set => _target.Value = value ;
    }

    public double PositionParameter
    {
      get => _positionParam.Value ;
      set => _positionParam.Value = value ;
    }

    public override Vector3d Position
    {
      get => _lastPosition ;
      set => SetPosition( value ) ;
    }

    public PipeSupportPosition( Support support ) : base( support )
    {
      _positionParam = new Memento<double>( support ) ;
      _positionParam.AfterNewlyValueChanged += ( sender, e ) => CheckNewlyPositionChanged() ;
      _positionParam.AfterHistoricallyValueChanged += ( sender, e ) => CheckHistoricallyPositionChanged() ;

      _target = new Memento<LeafEdge>( support ) ;

      _target.AfterValueChanged += ( sender, e ) =>
      {
        if ( null != e.OldValue ) {
          e.OldValue.NewlyLocalCodChanged -= LeafEdge_MoveEdge ;
          e.OldValue.AfterNewlyChildrenChanged -= LeafEdge_NewlyChildrenChanged ;
          e.OldValue.AfterHistoricallyChildrenChanged -= LeafEdge_HistoricallyChildrenChanged ;
          if ( e.OldValue.PipingPiece is Pipe pipe ) {
            pipe.AfterNewlyValueChanged -= Pipe_NewlyValueChanged ;
            pipe.AfterHistoricallyValueChanged -= Pipe_HistoricallyValueChanged ;
          }
        }

        if ( null != e.NewValue ) {
          e.NewValue.NewlyLocalCodChanged += LeafEdge_MoveEdge ;
          e.NewValue.AfterNewlyChildrenChanged += LeafEdge_NewlyChildrenChanged ;
          e.NewValue.AfterHistoricallyChildrenChanged += LeafEdge_HistoricallyChildrenChanged ;
          if ( e.NewValue.PipingPiece is Pipe pipe ) {
            pipe.AfterNewlyValueChanged += Pipe_NewlyValueChanged ;
            pipe.AfterHistoricallyValueChanged += Pipe_HistoricallyValueChanged ;
          }
        }
      } ;
      _target.AfterNewlyValueChanged += ( sender, e ) => CheckNewlyPositionChanged() ;
      _target.AfterHistoricallyValueChanged += ( sender, e ) => CheckHistoricallyPositionChanged() ;
    }

    private void CheckNewlyPositionChanged()
    {
      var newPosition = CalcPosition() ;
      if ( ! MementoEqualityComparer<Vector3d>.Equals( _lastPosition, newPosition ) ) {
        _lastPosition = newPosition ;
        OnNewlyPositionChanged() ;
      }
    }

    private void CheckHistoricallyPositionChanged()
    {
      _lastPosition = CalcPosition() ;
      OnHistoricallyPositionChanged() ;
    }

    private Vector3d CalcPosition()
    {
      var pipe = Target?.PipingPiece as Pipe ;
      if ( null == pipe ) return Vector3d.zero ;

      if ( pipe.ConnectPointCount < 2 ) return Vector3d.zero ;

      var pos = _positionParam.Value * pipe.GetConnectPoint( 1 ).Point ;
      for ( IElement elm = Target ; ( elm is Edge edge ) && ! ( elm is ISupportParentElement ) ; elm = elm.Parent ) {
        pos = edge.LocalCod.GlobalizePoint( pos ) ;
      }

      return pos ;
    }

    private void SetPosition( Vector3d position )
    {
      var pipe = Target?.PipingPiece as Pipe ;
      if ( null == pipe ) throw new InvalidOperationException() ;

      if ( pipe.ConnectPointCount < 2 ) throw new InvalidOperationException() ;

      var vec = pipe.GetConnectPoint( 1 ).Point ;
      var mag2 = vec.sqrMagnitude ;
      if ( mag2 < Vector3d.kEpsilon * Vector3d.kEpsilon ) return ;

      var stack = new Stack<LocalCodSys3d>() ;
      for ( IElement elm = Target ; ( elm is Edge edge ) && ! ( elm is ISupportParentElement ) ; elm = elm.Parent ) {
        stack.Push( edge.LocalCod ) ;
      }

      while ( 0 < stack.Count ) {
        position = stack.Pop().LocalizePoint( position ) ;
      }

      _positionParam.Value = Vector3d.Dot( position, vec ) / mag2 ;
    }

    private void LeafEdge_NewlyChildrenChanged( object sender, ItemChangedEventArgs<IElement> e )
    {
      LeafEdge_ChildrenChanged( e, CheckNewlyPositionChanged ) ;
    }

    private void LeafEdge_HistoricallyChildrenChanged( object sender, ItemChangedEventArgs<IElement> e )
    {
      LeafEdge_ChildrenChanged( e, CheckHistoricallyPositionChanged ) ;
    }

    private void LeafEdge_ChildrenChanged( ItemChangedEventArgs<IElement> e, Action afterPipeAdded )
    {
      bool pipeAdded = false ;
      e.RemovedItems.OfType<Pipe>().ForEach( pipe =>
      {
        pipe.AfterNewlyValueChanged -= Pipe_NewlyValueChanged ;
        pipe.AfterHistoricallyValueChanged -= Pipe_HistoricallyValueChanged ;
      } ) ;
      e.AddedItems.OfType<Pipe>().ForEach( pipe =>
      {
        pipe.AfterNewlyValueChanged -= Pipe_NewlyValueChanged ;
        pipe.AfterHistoricallyValueChanged -= Pipe_HistoricallyValueChanged ;
        pipeAdded = true ;
      } ) ;
      if ( pipeAdded ) {
        afterPipeAdded() ;
      }
    }

    private void LeafEdge_MoveEdge( object sender, EventArgs e )
    {
      CheckNewlyPositionChanged() ;
    }

    private void Pipe_NewlyValueChanged( object sender, EventArgs e )
    {
      CheckNewlyPositionChanged() ;
    }

    private void Pipe_HistoricallyValueChanged( object sender, EventArgs e )
    {
      CheckHistoricallyPositionChanged() ;
    }

    public override void Mirror( in Vector3d origin, in Vector3d normalDirection )
    {
    }
  }
}
