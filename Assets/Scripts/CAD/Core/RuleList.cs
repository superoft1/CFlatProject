using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.IO ;
using Chiyoda.DB ;
using UnityEngine ;

namespace Chiyoda.CAD.Core
{
  [CustomSerializer( typeof( Serializers.RuleListSerializer ) )]
  public class RuleList : ICopyable
  {
    private readonly IPropertiedElement _rootElement; // 主にブロックパターン
    private readonly List<IRule> _rules = new List<IRule>();

    public RuleList( IPropertiedElement rootElement )
    {
      _rootElement = rootElement;
    }

    internal IPropertiedElement RootElement => _rootElement ;
    internal IEnumerable<IRule> Rules => _rules ;
    
    public void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      var ruleList = another as RuleList;

      UnbindChangeEvents();

      _rules.Clear();
      _rules.AddRange( ruleList._rules.Select( rule => rule.GetCopyObject( storage ) ?? rule.Clone( storage ) ) );

      storage.RegisterRuleList( this );
    }

    public IRule AddRangeRule( string propertyName, string minExpression, string maxExpression )
    {
      var rule = new UserDefinedNamedPropertyRangeRule( _rootElement, propertyName, minExpression.ParseExpression(), maxExpression.ParseExpression() );
      AddRule( rule );
      return rule;
    }

    public IRule AddRule( string targetProperty, string expression )
    {
      var prop = targetProperty.ParseObjectPropertyExpression();
      var ex = expression.ParseExpression();

      var rule = new ObjectPropertyRule( _rootElement, prop.ObjectName, prop.PropertyName, ex );
      AddRule( rule );
      return rule;
    }

    internal void AddRule( IRule rule )
    {
      _rules.Add( rule );
    }

    public void BindChangeEvents( bool bindAndModify )
    {
      foreach ( var rule in _rules ) rule.BindChangeEvents();

      RemoveRedundantBindings() ;

      if ( bindAndModify ) {
        foreach ( var rule in _rules ) rule.Modify( true ) ;
      }
    }

    public void UnbindChangeEvents()
    {
      foreach ( var rule in _rules ) rule.UnbindChangeEvents();
    }
    
    private void RemoveRedundantBindings()
    {
      bool anyBindingIsRemoved ;
      do {
        anyBindingIsRemoved = false ;

        // 所持プロパティ一覧を収集
        var map = new Dictionary<(string ObjectName, string PropertyName), HashSet<(string ObjectName, string PropertyName)>>() ;
        foreach ( var rule in _rules ) {
          var target = rule.Target ;
          if ( false == rule.TriggerSources.Any() ) continue ;

          if ( ! map.TryGetValue( target, out var hash ) ) {
            hash = new HashSet<(string ObjectName, string PropertyName)>() ;
            map.Add( target, hash ) ;
          }

          foreach ( var source in rule.TriggerSources ) {
            AddChildrenRecursive( hash, source, map ) ;
          }
        }

        // 間接影響がある場合、直接影響は冗長なのでUnbind
        foreach ( var rule in _rules ) {
          HashSet<(string ObjectName, string PropertyName)> subSources = null ;
          foreach ( var source in rule.TriggerSources ) {
            if ( false == map.TryGetValue( source, out var set ) ) continue ;

            if ( null == subSources ) {
              subSources = new HashSet<(string ObjectName, string PropertyName)>( set ) ;
            }
            else {
              subSources.UnionWith( set ) ;
            }
          }

          if ( null == subSources ) continue ;

          foreach ( var source in rule.TriggerSources.ToArray() ) {
            if ( false == subSources.Contains( source ) ) continue ;
            if ( string.IsNullOrEmpty( source.ObjectName ) ) continue ;
            if ( false == rule.RemoveTriggerSourcePropertyName( source.ObjectName, source.PropertyName ) ) continue ;

            anyBindingIsRemoved = true ;
          }
        }
      } while ( anyBindingIsRemoved ) ;
    }

    private static void AddChildrenRecursive( HashSet<(string ObjectName, string PropertyName)> hash, (string ObjectName, string PropertyName) source, Dictionary<(string ObjectName, string PropertyName), HashSet<(string ObjectName, string PropertyName)>> map )
    {
      if ( ! hash.Add( source ) ) return ;

      if ( map.TryGetValue( source, out var children ) ) {
        foreach ( var child in children ) {
          AddChildrenRecursive( hash, child, map ) ;
        }
      }
    }

    internal void AddClonedRangeRules( RuleList another, string propertyName )
    {
      foreach ( var rangeRule in another._rules.OfType<UserDefinedNamedPropertyRangeRule>() ) {
        if ( rangeRule.PropertyName == propertyName ) {
          AddRule( rangeRule.CreateSynchronizedRangeRule( _rootElement ) ) ;
        }
      }
    }
    internal void RemoveClonedRangeRules( RuleList another, string propertyName )
    {
      foreach ( var rangeRule in another._rules.OfType<UserDefinedNamedPropertyRangeRule>() ) {
        if ( rangeRule.PropertyName == propertyName ) {
          rangeRule.RemoveSynchronizedRangeRule( another._rules ) ;
        }
      }
    }
  }
}
