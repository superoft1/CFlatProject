using System;
using System.Collections ;
using System.Collections.Generic;
using System.Diagnostics ;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology;
using Debug = UnityEngine.Debug ;

namespace Chiyoda.CAD.Core
{
  partial class Document
  {
    private const int MaxLoopCount = 1024 ;

    private class ActionStorage<T>
    {
      private readonly Action<T> _action ;
      private List<T> _caches = new List<T>() ;

      public ActionStorage( Action<T> action )
      {
        _action = action ;
      }

      public static implicit operator bool( ActionStorage<T> storage ) => ( 0 < storage._caches.Count ) ;

      public bool Add( T t )
      {
        if ( null == t ) return false ;
        if ( _caches.Contains( t ) ) return false ;

        _caches.Add( t ) ;
        return true ;
      }

      public bool ActionAll()
      {
        if ( 0 == _caches.Count ) return false ;

        var list = _caches ;
        _caches = new List<T>() ;

        foreach ( var t in list ) {
          _action( t ) ;
        }

        return true ;
      }

      public void Clear() => _caches.Clear() ;
    }

    private readonly ActionStorage<IRule> _modifyingRules = new ActionStorage<IRule>( rule => rule.Modify( false ) ) ;
    private readonly ActionStorage<(IUserDefinedRule, IPropertiedElement, IUserDefinedNamedProperty)> _modifyingUserDefinedRules = new ActionStorage<(IUserDefinedRule, IPropertiedElement, IUserDefinedNamedProperty)>( tuple => { tuple.Item1.Run( tuple.Item2, tuple.Item3 ) ; } ) ;
    private readonly ActionStorage<IRule> _remodifyingRules = new ActionStorage<IRule>( rule => rule.Modify( false ) ) ;
    private readonly ActionStorage<INamedProperty> _changingProps = new ActionStorage<INamedProperty>( prop => prop.ForceTriggerChange() ) ;
    private readonly ActionStorage<LeafEdge> _movedLeafEdges = new ActionStorage<LeafEdge>( le => le.ForceTriggerPositionChange() ) ;
    private readonly ActionStorage<LeafEdge> _connectionMaintenanceOrigins = new ActionStorage<LeafEdge>( origin => new Maintainer.ConnectionMaintainer( origin ).MaintainConnections() ) ;
    private readonly ActionStorage<CompositeBlockPattern> _jointMovedCompositeBlockPatterns = new ActionStorage<CompositeBlockPattern>( le => le.ModifyJointEdges() ) ;

    [NonSerialized]
    private bool _reserveCommit = false ;

    public bool HistoryCommit()
    {
      if ( EdgeMaintenanceRequired() ) {
        _reserveCommit = true ;
        return true ;
      }

      return History.Commit() ;
    }

    internal void RegisterEdgeMoved( Edge edge )
    {
      if ( edge is CompositeEdge ce ) {
        ce.GetAllLeafEdges().ForEach( AddMovedLeafEdge ) ;
      }
      else if ( edge is LeafEdge le ) {
        AddMovedLeafEdge( le ) ;
      }
    }

    internal void RegisterJointEdgeMoving( CompositeBlockPattern cbp )
    {
      _jointMovedCompositeBlockPatterns.Add( cbp ) ;
    }

    private void AddMovedLeafEdge( LeafEdge leafEdge )
    {
      if ( _movedLeafEdges.Add( leafEdge ) ) {
        if ( leafEdge.Closest<BlockPattern>()?.Parent is CompositeBlockPattern cbp ) {
          if ( leafEdge.Vertices.Any( IsNextOfCompositeBlockPatternJointEdge ) ) {
            RegisterJointEdgeMoving( cbp ) ;
          }
        }
      }
    }

    private static bool IsNextOfCompositeBlockPatternJointEdge( HalfVertex v )
    {
      return ( v.Partner?.LeafEdge?.Closest<BlockEdge>() is CompositeBlockPattern ) ;
    }

    internal void RegisterConnectionMaintenance( LeafEdge edge )
    {
      _connectionMaintenanceOrigins.Add( edge ) ;
    }

    internal void RegisterPropertyChange( INamedProperty prop )
    {
      _changingProps.Add( prop ) ;
    }

    internal void RegisterModifyingRule( IRule rule )
    {
      _modifyingRules.Add( rule ) ;
    }

    internal void RegisterModifyingRule( IUserDefinedRule rule, IPropertiedElement owner, IUserDefinedNamedProperty prop )
    {
      _modifyingUserDefinedRules.Add( ( rule, owner, prop ) ) ;
    }

    internal void RegisterRemodifyingRule( IRule rule )
    {
      _remodifyingRules.Add( rule ) ;
    }

    public void MaintainEdgePlacement()
    {
      MaintainEdgePlacementImpl() ;

      if ( _reserveCommit ) {
        _reserveCommit = false ;
        History.Commit() ;
      }
    }

    private bool EdgeMaintenanceRequired()
    {
      return _movedLeafEdges || _changingProps || _modifyingRules || _modifyingUserDefinedRules || _connectionMaintenanceOrigins || _remodifyingRules || _jointMovedCompositeBlockPatterns ;
    }

    private void MaintainEdgePlacementImpl()
    {
      var counter = new MaintenanceCounter() ;

      while ( EdgeMaintenanceRequired() ) {
        if ( _movedLeafEdges.ActionAll() ) {
          if ( ! CheckRecursion( counter, ref counter.MovedLeafEdgeCount ) ) return ;
          continue ;
        }

        if ( _changingProps.ActionAll() ) {
          if ( ! CheckRecursion( counter, ref counter.ChangingPropertyCount ) ) return ;
          continue ;
        }

        if ( _modifyingRules ) {
          while ( _modifyingRules.ActionAll() ) {
            if ( ! CheckRecursion( counter, ref counter.ModifyingRuleCount ) ) return ;
          }

          continue ;
        }

        if ( _modifyingUserDefinedRules ) {
          while ( _modifyingUserDefinedRules.ActionAll() ) {
            if ( ! CheckRecursion( counter, ref counter.ModifyingRuleCount ) ) return ;
          }

          continue ;
        }

        if ( _connectionMaintenanceOrigins.ActionAll() ) {
          if ( ! CheckRecursion( counter, ref counter.ConnectionMaintenanceCount ) ) return ;
          continue ;
        }

        if ( _remodifyingRules.ActionAll() ) {
          if ( ! CheckRecursion( counter, ref counter.ModifyingRuleCount ) ) return ;
          continue ;
        }

        if ( _jointMovedCompositeBlockPatterns.ActionAll() ) {
          if ( ! CheckRecursion( counter, ref counter.JointMovedCount ) ) return ;
        }
      }

      FinishMaintenance( counter ) ;
    }

    private bool CheckRecursion( MaintenanceCounter counter, ref int count )
    {
      ++count ;
      if ( ++counter.TotalCount < MaxLoopCount ) {
        // BPAの各BPのプロパティが一致することをテスト
        //TestAllCompositeBlockPatterns() ;
        return true ;
      }

      _movedLeafEdges.Clear() ;
      _changingProps.Clear() ;
      _modifyingRules.Clear() ;
      _connectionMaintenanceOrigins.Clear() ;
      _remodifyingRules.Clear() ;
      _jointMovedCompositeBlockPatterns.Clear() ;
      Debug.LogWarning( $"MaintainConnections(): more than {MaxLoopCount} recursions!!\n{counter}" ) ;

      return false ;
    }

    private void FinishMaintenance( MaintenanceCounter counter )
    {
      if ( 0 == counter.TotalCount ) return ;

      Debug.Log( $"MaintainConnections(): {counter}" ) ;
    }

    private class MaintenanceCounter
    {
      public int TotalCount ;
      public int MovedLeafEdgeCount ;
      public int ChangingPropertyCount ;
      public int ModifyingRuleCount ;
      public int ConnectionMaintenanceCount ;
      public int JointMovedCount ;

      public override string ToString()
      {
        return $"Total: {TotalCount} / EdgeMove: {MovedLeafEdgeCount} / PropChange: {ChangingPropertyCount} / Rule: {ModifyingRuleCount} / Connection: {ConnectionMaintenanceCount} / JointMove: {JointMovedCount}" ;
      }
    }

    private void TestAllCompositeBlockPatterns()
    {
      foreach ( var cbp in GetCompositeBlockPatterns().Where( c => 2 <= c.CopyCount ) ) {
        var bp0 = cbp.GetBlockPattern( 0 ) ;

        // property check
        for ( var i = 1 ; i < cbp.CopyCount ; ++i ) {
          var bpi = cbp.GetBlockPattern( i ) ;
          foreach ( var prop in cbp.GetProperties() ) {
            var prop1 = bp0.GetProperty( prop.PropertyName ) ;
            var prop2 = bpi.GetProperty( prop.PropertyName ) ;
            if ( null == prop2 ) continue ;
            if ( prop1.Value != prop2.Value ) {
              Debug.Log( $"Property `{prop.PropertyName}' is different between [{0}]({prop1.Value}) and [{i}]({prop2.Value})" ) ;
            }
          }
        }

        // pipe length check
        for ( var i = 1 ; i < cbp.CopyCount ; ++i ) {
          var bpi = cbp.GetBlockPattern( i ) ;
          foreach ( var pair in bp0.GetAllComponents().OfType<Pipe>().Zip( bpi.GetAllComponents().OfType<Pipe>(), ( pipe0, pipei ) => ( pipe0, pipei ) ) ) {
            if ( pair.pipe0.MinimumLengthRatioByDiameter != pair.pipei.MinimumLengthRatioByDiameter ) {
              Debug.Log( $"Property `MinimumLengthRatioByDiameter' of {pair.pipe0} is different between [{0}]({pair.pipe0.MinimumLengthRatioByDiameter}) and [{i}]({pair.pipei.MinimumLengthRatioByDiameter})" ) ;
            }
            if ( pair.pipe0.FlexRatio != pair.pipei.FlexRatio ) {
              Debug.Log( $"Property `FlexRatio' of {pair.pipe0} is different between [{0}]({pair.pipe0.FlexRatio}) and [{i}]({pair.pipei.FlexRatio})" ) ;
            }
            if ( pair.pipe0.MinimumLengthWithoutOletRadius != pair.pipei.MinimumLengthWithoutOletRadius ) {
              Debug.Log( $"Property `MinimumLengthWithoutOletRadius' of {pair.pipe0} is different between [{0}]({pair.pipe0.MinimumLengthWithoutOletRadius}) and [{i}]({pair.pipei.MinimumLengthWithoutOletRadius})" ) ;
            }
            if ( pair.pipe0.PreferredLength != pair.pipei.PreferredLength ) {
              Debug.Log( $"Property `PreferredLength' of {pair.pipe0} is different between [{0}]({pair.pipe0.PreferredLength}) and [{i}]({pair.pipei.PreferredLength})" ) ;
            }
          }
        }
      }
    }

    private IEnumerable<CompositeBlockPattern> GetCompositeBlockPatterns()
    {
      return _edges.SelectMany( GetCompositeBlockPatterns ) ;
    }
    private static IEnumerable<CompositeBlockPattern> GetCompositeBlockPatterns( Edge edge )
    {
      if ( ! ( edge is CompositeEdge ce ) ) yield break ;

      if ( ce is CompositeBlockPattern cbp ) yield return cbp ;
      foreach ( var e in ce.EdgeList ) {
        foreach ( var c in GetCompositeBlockPatterns( e ) ) {
          yield return c ;
        }
      }
    }
  }
}
