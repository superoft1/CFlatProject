using System;
using System.Collections.Generic;
using System.Linq;

namespace Chiyoda.CAD.Core
{
  public interface IRule : IO.ISerializableObject
  {
    (string ObjectName, string PropertyName) Target { get ; }
    IEnumerable<(string ObjectName, string PropertyName)> TriggerSources { get ; }

    void Modify( bool forceTriggerChange );
    void BindChangeEvents();
    void UnbindChangeEvents();

    IRule AddTriggerSourcePropertyName( string propertyName );
    IRule AddTriggerSourcePropertyName( string objectName, string propertyName );

    bool RemoveTriggerSourcePropertyName( string objectName, string propertyName ) ;

    IRule Clone( CopyObjectStorage storage );
  }

  public abstract class AbstractRule : IRule
  {
    protected readonly IPropertiedElement _rootElement; // 主にブロックパターン
    protected List<KeyValuePair<string, string>> _triggerSourceNames = null;
    private bool _bound = false;

    protected AbstractRule( IPropertiedElement rootElement )
    {
      _rootElement = rootElement ?? throw new ArgumentNullException( nameof( rootElement ) );
    }

    protected AbstractRule( AbstractRule another, CopyObjectStorage storage )
    {
      _rootElement = another._rootElement.GetCopyObject( storage );
      if ( null != another._triggerSourceNames ) {
        _triggerSourceNames = new List<KeyValuePair<string, string>>( another._triggerSourceNames );
      }
    }

    public abstract IRule Clone( CopyObjectStorage storage );

    public abstract (string ObjectName, string PropertyName) Target { get ; }

    public IEnumerable<(string ObjectName, string PropertyName)> TriggerSources
    {
      get
      {
        foreach ( var source in CollectExpressionSources() ) {
          yield return source ;
        }

        if ( null != _triggerSourceNames ) {
          foreach ( var pair in _triggerSourceNames ) {
            yield return ( ObjectName: pair.Key, PropertyName: pair.Value ) ;
          }
        }
      }
    }

    public abstract void Modify( bool forceTriggerChange );
    public abstract void BindChangeEvents();
    public abstract void UnbindChangeEvents();

    protected abstract IEnumerable<(string ObjectName, string PropertyName)> CollectExpressionSources() ;

    protected void BindChangeEvents( IPropertyExpression expression )
    {
      BindChangeEvents( expression, _rootElement ) ;
    }
    protected void UnbindChangeEvents( IPropertyExpression expression )
    {
      UnbindChangeEvents( expression, _rootElement ) ;
    }
    protected void BindChangeEvents( IPropertyExpression expression, IPropertiedElement rootElement )
    {
      _bound = true;
      foreach ( var pair in expression.GetSourceProperties( rootElement ) ) {
        if ( IsSelf( pair.Key, pair.Value ) ) continue;
        pair.Value.AddAffectingRule( this );
      }
      foreach ( var pair in GetTriggerSources() ) {
        if ( IsSelf( pair.Key, pair.Value ) ) continue;
        pair.Value.AddAffectingRule( this );
      }
    }
    protected void UnbindChangeEvents( IPropertyExpression expression, IPropertiedElement rootElement )
    {
      foreach ( var pair in expression.GetSourceProperties( rootElement ) ) {
        if ( IsSelf( pair.Key, pair.Value ) ) continue;
        pair.Value.RemoveAffectingRule( this );
      }
      foreach ( var pair in GetTriggerSources() ) {
        if ( IsSelf( pair.Key, pair.Value ) ) continue;
        pair.Value.RemoveAffectingRule( this );
      }
      _bound = false;
    }

    protected IEnumerable<(string ObjectName, string PropertyName)> CollectExpressionSources( IPropertyExpression expression )
    {
      foreach ( var pair in expression.GetSourceProperties( _rootElement ) ) {
        if ( IsSelf( pair.Key, pair.Value ) ) continue;

        yield return ( ObjectName: pair.Key?.ObjectName, PropertyName: pair.Value.PropertyName ) ;
      }
    }


    protected abstract bool IsSelf( IPropertiedElement elm, INamedProperty prop );

    protected IPropertiedElement GetClonedRootElement( CopyObjectStorage storage )
    {
      return storage.Get( _rootElement as ICopyable ) as IPropertiedElement;
    }

    public IRule AddTriggerSourcePropertyName( string propertyName )
    {
      return AddTriggerSourcePropertyName( null, propertyName );
    }
    public IRule AddTriggerSourcePropertyName( string objectName, string propertyName )
    {
      if ( _bound ) {
        var pair = GetProperty( objectName, propertyName );
        if ( !IsSelf( pair.Key, pair.Value ) ) {
          pair.Value.AddAffectingRule( this );
        }
      }
      AddTriggerSource( new KeyValuePair<string, string>( objectName, propertyName ) );

      return this;
    }

    public bool RemoveTriggerSourcePropertyName( string objectName, string propertyName )
    {
      var removed = false ;
      
      if ( _bound ) {
        var pair = GetProperty( objectName, propertyName );
        if ( !IsSelf( pair.Key, pair.Value ) ) {
          removed = pair.Value.RemoveAffectingRule( this );
        }
      }
      return RemoveTriggerSource( new KeyValuePair<string, string>( objectName, propertyName ) ) || removed;
    }

    private IEnumerable<KeyValuePair<IPropertiedElement, INamedProperty>> GetTriggerSources()
    {
      if ( null == _triggerSourceNames ) return Array.Empty<KeyValuePair<IPropertiedElement, INamedProperty>>();
      return _triggerSourceNames.Select( pair => GetProperty( pair.Key, pair.Value ) );
    }

    private KeyValuePair<IPropertiedElement, INamedProperty> GetProperty( string objectName, string propertyName )
    {
      var elm = (null == objectName) ? _rootElement : _rootElement.GetElementByName( objectName );
      if ( null == elm ) {
        UnityEngine.Debug.Log($"{objectName} not found!");
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.ElementNotFound, objectName );
      }
      var prop = elm.GetProperty( propertyName );
      if ( null == prop ) {
        UnityEngine.Debug.Log($"{objectName}.{propertyName} not found!");
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.PropertyNotFound, objectName, propertyName );
      }

      return new KeyValuePair<IPropertiedElement, INamedProperty>( elm, prop );
    }

    private void AddTriggerSource( KeyValuePair<string, string> pair )
    {
      if ( null == _triggerSourceNames ) {
        _triggerSourceNames = new List<KeyValuePair<string, string>> { pair } ;
      }
      else if ( false == _triggerSourceNames.Contains( pair ) ) {
        _triggerSourceNames.Add( pair ) ;
      }
    }

    private bool RemoveTriggerSource( KeyValuePair<string, string> pair )
    {
      if ( null == _triggerSourceNames ) return false ;
      return _triggerSourceNames.Remove( pair ) ;
    }
  }

  [IO.CustomSerializer( typeof( Serializers.ObjectPropertyRuleSerializer ) )]
  public class ObjectPropertyRule : AbstractRule
  {
    private readonly string _objectName;
    private readonly string _propertyName;
    private readonly IPropertyExpression _expression;

    internal string ObjectName => _objectName ;
    internal string PropertyName => _propertyName ;
    internal IPropertyExpression Expression => _expression ;

    public ObjectPropertyRule( IPropertiedElement rootElement, string objectName, string propertyName, IPropertyExpression expression )
      : base( rootElement )
    {
      _objectName = objectName;
      _propertyName = propertyName;
      _expression = expression;
    }

    protected ObjectPropertyRule( ObjectPropertyRule another, CopyObjectStorage storage )
      : base( another, storage )
    {
      _objectName = another._objectName;
      _propertyName = another._propertyName;
      _expression = another._expression;
    }

    public override IRule Clone( CopyObjectStorage storage )
    {
      var rule = new ObjectPropertyRule( this, storage );
      storage.Register( this, rule );
      return rule;
    }

    public override (string ObjectName, string PropertyName) Target => ( ObjectName: _objectName, PropertyName: _propertyName ) ;

    public override void Modify( bool forceTriggerChange )
    {
      var elm = ( null == _objectName ) ? _rootElement : _rootElement.GetElementByName( _objectName ) ;
      if ( null == elm ) {
        if ( ':' == _objectName[ 0 ] ) return ; // 特殊オブジェクトはエラー不要
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.ElementNotFound, _objectName ) ;
      }

      var prop = elm.GetProperty( _propertyName ) ;
      if ( null == prop ) {
        throw new PropertyRuntimeException( PropertyRuntimeErrorType.PropertyNotFound, _objectName, _propertyName ) ;
      }

      double value ;
      try {
        value = _expression.GetValue( null, _rootElement ) ;
      }
      catch ( PropertyMustBeRevalidatedException ) {
        _rootElement.Document.RegisterRemodifyingRule( this ) ;
        return ;
      }
      catch ( PropertyValidationAbortedException ) {
        return ; // これ以上値の計算を行なわない
      }

      if ( MementoEqualityComparer<double>.Equals( prop.Value, value ) ) {
        if ( forceTriggerChange ) prop.ForceTriggerChange() ;
      }
      else {
        prop.Value = value ;
      }
    }

    public override void BindChangeEvents()
    {
      BindChangeEvents( _expression );
    }
    public override void UnbindChangeEvents()
    {
      UnbindChangeEvents( _expression );
    }

    protected override IEnumerable<(string ObjectName, string PropertyName)> CollectExpressionSources()
    {
      return CollectExpressionSources( _expression ) ;
    }

    protected override bool IsSelf( IPropertiedElement elm, INamedProperty prop )
    {
      return (elm.ObjectName == _objectName && prop.PropertyName == _propertyName);
    }
  }

  [IO.CustomSerializer( typeof( Serializers.UserDefinedNamedPropertyRangeRuleSerializer ) )]
  public class UserDefinedNamedPropertyRangeRule : AbstractRule
  {
    public string PropertyName { get ; }
    private readonly IPropertyExpression _minExpression;
    private readonly IPropertyExpression _maxExpression;

    internal IPropertyExpression MinExpression => _minExpression ;
    internal IPropertyExpression MaxExpression => _maxExpression ;

    public event EventHandler RangeChanged ;

    public UserDefinedNamedPropertyRangeRule( IPropertiedElement rootElement, string propertyName, IPropertyExpression minExpression, IPropertyExpression maxExpression )
      : base( rootElement )
    {
      PropertyName = propertyName;
      _minExpression = minExpression;
      _maxExpression = maxExpression;
    }

    protected UserDefinedNamedPropertyRangeRule( UserDefinedNamedPropertyRangeRule another, CopyObjectStorage storage )
      : base( another, storage )
    {
      PropertyName = another.PropertyName;
      _minExpression = another._minExpression;
      _maxExpression = another._maxExpression;
    }

    public override IRule Clone( CopyObjectStorage storage )
    {
      var rule = new UserDefinedNamedPropertyRangeRule( this, storage );
      storage.Register( this, rule );
      return rule;
    }

    public override (string ObjectName, string PropertyName) Target => ( ObjectName: null, PropertyName: PropertyName ) ;

    public override void Modify( bool forceTriggerChange )
    {
      var prop = _rootElement.GetProperty( PropertyName ) as IUserDefinedRangedNamedProperty ;
      if ( null == prop ) return ;

      bool rangeChanged = false ;
      if ( null != _minExpression ) {
        try {
          var min = _minExpression.GetValue( null, _rootElement ) ;
          if ( false == MementoEqualityComparer<double>.Equals( prop.MinValue, min ) ) {
            prop.MinValue = min ;
            rangeChanged = true ;
          }
        }
        catch ( PropertyMustBeRevalidatedException ) {
          _rootElement.Document.RegisterRemodifyingRule( this ) ;
          return ;
        }
        catch ( PropertyValidationAbortedException ) {
        }
      }

      if ( null != _maxExpression ) {
        try {
          var max = _maxExpression.GetValue( null, _rootElement ) ;
          if ( false == MementoEqualityComparer<double>.Equals( prop.MaxValue, max ) ) {
            prop.MaxValue = max ;
            rangeChanged = true ;
          }
        }
        catch ( PropertyMustBeRevalidatedException ) {
          if ( rangeChanged ) {
            RangeChanged?.Invoke( this, EventArgs.Empty ) ;
          }

          _rootElement.Document.RegisterRemodifyingRule( this ) ;
          return ;
        }
        catch ( PropertyValidationAbortedException ) {
        }
      }

      if ( rangeChanged ) {
        RangeChanged?.Invoke( this, EventArgs.Empty ) ;
      }

      var newValue = Math.Max( prop.MinValue, Math.Min( prop.MaxValue, prop.Value ) ) ;
      if ( MementoEqualityComparer<double>.Equals( prop.Value, newValue ) ) {
        if ( forceTriggerChange ) prop.ForceTriggerChange() ;
      }
      else {
        prop.Value = prop.Value ;
      }
    }

    public override void BindChangeEvents()
    {
      if ( null != _minExpression ) {
        BindChangeEvents( _minExpression );
      }
      if ( null != _maxExpression ) {
        BindChangeEvents( _maxExpression );
      }
    }

    public override void UnbindChangeEvents()
    {
      if ( null != _minExpression ) {
        UnbindChangeEvents( _minExpression );
      }
      if ( null != _maxExpression ) {
        UnbindChangeEvents( _maxExpression );
      }
    }

    protected override IEnumerable<(string ObjectName, string PropertyName)> CollectExpressionSources()
    {
      if ( null != _minExpression ) {
        foreach ( var source in CollectExpressionSources( _minExpression )  ) {
          yield return source ;
        }
      }
      if ( null != _maxExpression ) {
        foreach ( var source in CollectExpressionSources( _maxExpression )  ) {
          yield return source ;
        }
      }
    }

    protected override bool IsSelf( IPropertiedElement elm, INamedProperty prop )
    {
      return (elm == _rootElement && prop.PropertyName == PropertyName);
    }


    public IRule CreateSynchronizedRangeRule(IPropertiedElement newElement)
    {
      return new SynchronizedRangeRule( newElement, this ) ;
    }

    public void RemoveSynchronizedRangeRule( IList<IRule> rules )
    {
      for ( int i = 0, n = rules.Count ; i < n ; ++i ) {
        if ( rules[ i ] is SynchronizedRangeRule sync && sync.TargetRangeRule == this ) {
          rules.RemoveAt( i ) ;
          return ;
        }
      }
    }

    [IO.CustomSerializer( typeof( Serializers.SynchronizedRangeRuleSerializer ) )]
    internal class SynchronizedRangeRule : AbstractRule
    {
      public UserDefinedNamedPropertyRangeRule TargetRangeRule { get ; }

      public SynchronizedRangeRule( IPropertiedElement rootElement, UserDefinedNamedPropertyRangeRule rangeRule ) : base( rootElement )
      {
        TargetRangeRule = rangeRule ;
      }

      private SynchronizedRangeRule( SynchronizedRangeRule another, CopyObjectStorage storage ) : base( another, storage )
      {
        TargetRangeRule = another.TargetRangeRule.GetCopyObject( storage ) ?? (UserDefinedNamedPropertyRangeRule) another.TargetRangeRule.Clone( storage ) ;
      }

      public override IRule Clone( CopyObjectStorage storage )
      {
        var rule = new SynchronizedRangeRule( this, storage );
        storage.Register( this, rule );
        return rule;
      }

      public override (string ObjectName, string PropertyName) Target => ( ObjectName: null, PropertyName: TargetRangeRule.PropertyName ) ;

      public override void Modify( bool forceTriggerChange )
      {
        var prop = _rootElement.GetProperty( TargetRangeRule.PropertyName ) as IUserDefinedRangedNamedProperty;
        if ( null == prop ) return;

        if ( null != TargetRangeRule._minExpression ) {
          try {
            prop.MinValue = TargetRangeRule._minExpression.GetValue( null, TargetRangeRule._rootElement ) ;
          }
          catch ( PropertyMustBeRevalidatedException ) {
            _rootElement.Document.RegisterRemodifyingRule( this );
            return ;
          }
          catch ( PropertyValidationAbortedException ) {
          }
        }

        if ( null != TargetRangeRule._maxExpression ) {
          try {
            prop.MaxValue = TargetRangeRule._maxExpression.GetValue( null, TargetRangeRule._rootElement ) ;
          }
          catch ( PropertyMustBeRevalidatedException ) {
            _rootElement.Document.RegisterRemodifyingRule( this );
            return ;
          }
          catch ( PropertyValidationAbortedException ) {
          }
        }
        
        var newValue = Math.Max( prop.MinValue, Math.Min( prop.MaxValue, prop.Value ) ) ;
        if ( MementoEqualityComparer<double>.Equals( prop.Value, newValue ) ) {
          if ( forceTriggerChange ) prop.ForceTriggerChange() ;
        }
        else {
          prop.Value = prop.Value ;
        }
      }

      public override void BindChangeEvents()
      {
        TargetRangeRule.RangeChanged += TargetRangeRule_RangeChanged ;
      }

      public override void UnbindChangeEvents()
      {
        TargetRangeRule.RangeChanged -= TargetRangeRule_RangeChanged ;
      }

      private void TargetRangeRule_RangeChanged( object sender, EventArgs e )
      {
        Modify( false ) ;
      }

      protected override IEnumerable<(string ObjectName, string PropertyName)> CollectExpressionSources()
      {
        return Array.Empty<(string ObjectName, string PropertyName)>() ;
      }

      protected override bool IsSelf( IPropertiedElement elm, INamedProperty prop )
      {
        return (elm == _rootElement && prop.PropertyName == TargetRangeRule.PropertyName);
      }
    }
  }
}
