using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq ;
using Chiyoda.CAD.Core;
using UnityEngine;
using Debug = System.Diagnostics.Debug ;

namespace Chiyoda.CAD.Model
{
  public abstract class Entity : MemorableObjectBase, IPropertiedElement
  {
    [NonSerialized]
    private readonly Document _document;

    private readonly Memento<bool> _visible;
    private readonly Memento<string> _objectName;
    private readonly Memento<string> _name;
    private readonly Memento<IElement> _parent;

    private RuleList ruleList = null;

    public event EventHandler VisibilityChanged ;
    public event EventHandler<ItemChangedEventArgs<IElement>> AfterNewlyChildrenChanged;
    public event EventHandler<ItemChangedEventArgs<IElement>> AfterHistoricallyChildrenChanged;
    public event EventHandler<PropertyEventArgs> PropertyAdded;
    public event EventHandler<PropertyEventArgs> PropertyRemoved;

    public override History History => _document.History ;

    public string Name
    {
      get => _name.Value ?? GetType().Name ;
      set => _name.Value = string.IsNullOrEmpty( value ) ? null : value ;
    }

    public string ObjectName
    {
      get { return _objectName.Value; }
      set { _objectName.Value = value; }
    }

    protected MementoList<T> CreateMementoListAndSetupChildrenEvents<T>() where T : IElement
    {
      var memento = new MementoList<T>( this ) ;
      memento.AfterNewlyItemChanged += Memento_AfterNewlyChildrenChanged ;
      memento.AfterHistoricallyItemChanged += Memento_AfterHistoricallyChildrenChanged ;
      return memento ;
    }
    protected MementoDictionary<TKey, TValue> CreateMementoDictionaryAndSetupChildrenEvents<TKey, TValue>() where TValue : IElement
    {
      var memento = new MementoDictionary<TKey, TValue>( this ) ;
      memento.AfterNewlyItemChanged += Memento_AfterNewlyChildrenChanged ;
      memento.AfterHistoricallyItemChanged += Memento_AfterHistoricallyChildrenChanged ;
      return memento ;
    }
    protected Memento<T> CreateMementoAndSetupChildrenEvents<T>() where T : IElement
    {
      var memento = new Memento<T>( this ) ;
      memento.AfterNewlyValueChanged += Memento_AfterNewlyValueChanged ;
      memento.AfterHistoricallyValueChanged += Memento_AfterHistoricallyValueChanged ;
      return memento ;
    }

    private void Memento_AfterNewlyValueChanged<T>( object sender, ValueChangedEventArgs<T> e ) where T : IElement
    {
      OnNewlyChildrenChanged( e.ToItemChangedEventArgs<IElement>() ) ;
    }

    private void Memento_AfterHistoricallyValueChanged<T>( object sender, ValueChangedEventArgs<T> e ) where T : IElement
    {
      OnHistoricallyChildrenChanged( e.ToItemChangedEventArgs<IElement>() ) ;
    }

    private void Memento_AfterNewlyChildrenChanged<T>( object sender, ItemChangedEventArgs<T> e ) where T : IElement
    {
      OnNewlyChildrenChanged( e.As<IElement>() ) ;
    }
    private void Memento_AfterHistoricallyChildrenChanged<T>( object sender, ItemChangedEventArgs<T> e ) where T : IElement
    {
      OnHistoricallyChildrenChanged( e.As<IElement>() ) ;
    }
    private void Memento_AfterNewlyChildrenChanged<TKey, TValue>( object sender, ItemChangedEventArgs<KeyValuePair<TKey, TValue>> e ) where TValue : IElement
    {
      OnNewlyChildrenChanged( e.Convert<IElement>( pair => pair.Value ) ) ;
    }
    private void Memento_AfterHistoricallyChildrenChanged<TKey, TValue>( object sender, ItemChangedEventArgs<KeyValuePair<TKey, TValue>> e ) where TValue : IElement
    {
      OnHistoricallyChildrenChanged( e.Convert<IElement>( pair => pair.Value ) ) ;
    }

    protected virtual void OnNewlyChildrenChanged( ItemChangedEventArgs<IElement> e )
    {
      foreach ( var item in e.RemovedItems ) {
        item.OnRemovedFromParent();
      }
      foreach ( var item in e.AddedItems ) {
        item.OnAddedIntoNewParent( this ) ;
      }

      AfterNewlyChildrenChanged?.Invoke( this, e );
    }

    protected virtual void OnHistoricallyChildrenChanged( ItemChangedEventArgs<IElement> e )
    {
      AfterHistoricallyChildrenChanged?.Invoke( this, e ) ;
    }

    protected Entity( Document document )
    {
      _document = document;

      _visible = CreateMemento( true ) ;
      _objectName = CreateMemento<string>() ;
      _name = CreateMemento<string>() ;
      _parent = CreateMemento<IElement>() ;
    }

    protected internal virtual void InitializeDefaultObjects()
    {
    }
    protected internal virtual void RegisterNonMementoMembersFromDefaultObjects()
    {
      _visible.AfterValueChanged += ( sender, e ) => OnVisibilityChanged( EventArgs.Empty );

      _parent.AfterNewlyValueChanged += ( sender, e ) => OnNewlyParentChanged( e ) ;
    }

    public virtual void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      var entity = another as Entity;
      _visible.CopyFrom( entity._visible.Value ) ;

      _objectName.CopyFrom( entity._objectName.Value );
      _name.CopyFrom( entity._name.Value );
      _parent.CopyFrom( entity._parent.Value.GetCopyObject( storage ) ) ;

      // プロパティのコピー
      foreach ( var prop in entity._properties ) {
        var cloned = _properties[ prop.PropertyName ] ;
        if ( null != cloned ) {
          // 同名プロパティ同士を関連付け
          storage.Register( prop, cloned );
          continue;
        }

        var udprop = prop as IUserDefinedNamedProperty;
        var udcloned = udprop.Clone( this, true );
        storage.Register( udprop, udcloned );
        _properties.Add( udcloned );
      }

      // ルールのコピー
      if ( null == entity.ruleList ) {
        ruleList = null;
      }
      else {
        ruleList = new RuleList( this );
        ruleList.CopyFrom( entity.ruleList, storage );
      }
    }

    public Document Document => _document ;

    public virtual bool UserDefinedPropertyEditable => true;

    public virtual Bounds? GetGlobalBounds() => null ;

    public IElement Parent
    {
      get => _parent.Value ;
    }

    void IElement.OnRemovedFromParent()
    {
      _parent.Value = null ;
    }

    void IElement.OnAddedIntoNewParent( IElement newParent )
    {
      _parent.Value = newParent ;
    }

    protected virtual void OnNewlyParentChanged( ValueChangedEventArgs<IElement> e )
    {
      if ( null != e.OldValue && null != e.NewValue ) {
        throw new InvalidOperationException( "Entity is not unreferenced by old parent" );
      }
    }

    public virtual IEnumerable<IElement> Children
    {
      get { yield break; }
    }

    public IEnumerable<IElement> GetAllDescendants()
    {
      foreach ( var elm in Children ) {
        yield return elm;

        if ( elm is Entity entity ) {
          foreach ( var descendant in entity.GetAllDescendants() ) {
            yield return descendant;
          }
        }
      }
    }

    public bool IsVisible
    {
      get => _visible.Value ;
      set => _visible.Value = value ;
    }

    public virtual bool HasError => false ;

    protected virtual void OnVisibilityChanged( EventArgs e )
    {
      VisibilityChanged?.Invoke( this, e ) ;
    }

    public RuleList RuleList
    {
      get
      {
        if ( null == ruleList ) ruleList = new RuleList( this );
        return ruleList;
      }
    }

    protected void UnbindAllRules()
    {
      if ( null != ruleList ) {
        ruleList.UnbindChangeEvents() ;
        ruleList = null ;
      }

      foreach ( var entity in Children.OfType<Entity>() ) {
        entity.UnbindAllRules() ;
      }
    }

    #region IRuledElement

    private readonly PropertyCollection _properties = new PropertyCollection() ;

    public IUserDefinedNamedProperty RegisterSameNameUserDefinedProperty( IUserDefinedNamedProperty prop )
    {
      var clonedProp = prop.Clone( this, false ) ;
      RegisterProperty( clonedProp, true );
      return clonedProp;
    }

    public IUserDefinedNamedProperty RegisterUserDefinedProperty( string propertyName, PropertyType type, double defaultValue )
    {
      var prop = new UserDefinedNamedProperty( this, propertyName, type, defaultValue );
      RegisterProperty( prop, true );
      return prop;
    }
    public IUserDefinedNamedProperty RegisterUserDefinedProperty( string propertyName, PropertyType type, double defaultValue, string minExpression, string maxExpression )
    {
      var prop = new UserDefinedNamedProperty( this, propertyName, type, defaultValue );
      RegisterProperty( prop, true );
      if ( !string.IsNullOrEmpty( minExpression ) || !string.IsNullOrEmpty( maxExpression ) ) {
        RuleList.AddRangeRule( propertyName, minExpression, maxExpression );
      }
      return prop;
    }
    public IUserDefinedNamedProperty RegisterUserDefinedProperty( string propertyName, PropertyType type, double defaultValue, double minValue, double maxValue )
    {
      var prop = new UserDefinedNamedProperty( this, propertyName, type, defaultValue, minValue, maxValue );
      RegisterProperty( prop, true );
      return prop;
    }
    public IUserDefinedNamedProperty RegisterUserDefinedProperty( string propertyName, PropertyType type, double defaultValue, string minExpression, string maxExpression, double stepValue )
    {
      var prop = new UserDefinedSteppedNamedProperty( this, propertyName, type, defaultValue, defaultValue, defaultValue, stepValue ) ;
      RegisterProperty( prop, true );
      if ( !string.IsNullOrEmpty( minExpression ) || !string.IsNullOrEmpty( maxExpression ) ) {
        RuleList.AddRangeRule( propertyName, minExpression, maxExpression );
      }
      return prop;
    }
    public IUserDefinedNamedProperty RegisterUserDefinedProperty( string propertyName, PropertyType type, double defaultValue, double minValue, double maxValue, double stepValue )
    {
      var prop = new UserDefinedSteppedNamedProperty( this, propertyName, type, defaultValue, minValue, maxValue, stepValue ) ;
      RegisterProperty( prop, true );
      return prop;
    }
    public IUserDefinedNamedProperty RegisterUserDefinedProperty( string propertyName, double defaultValue, IEnumerable<KeyValuePair<string, double>> enums )
    {
      if ( null == enums ) throw new ArgumentNullException( nameof( enums ) );

      var prop = new UserDefinedEnumNamedProperty( this, propertyName, PropertyType.GeneralDouble, defaultValue, enums );
      RegisterProperty( prop, true );
      return prop;
    }
    public IUserDefinedNamedProperty RegisterUserDefinedProperty<TEnum>( string propertyName, TEnum value ) where TEnum : struct, Enum
    {
      return RegisterUserDefinedProperty( propertyName, Convert.ToDouble( value ), UserDefinedNamedProperty.ToNameValueList<TEnum>() );
    }

    public INamedProperty RegisterDelayedProperty( string propertyName, PropertyType type, Func<double> propGetter, Action<double> propSetter, Action<INamedProperty> onPropertyRegistered )
    {
      var prop = new DelayedNamedProperty( Document, propertyName, type, propGetter, propSetter, onPropertyRegistered );
      RegisterProperty( prop, false );
      return prop;
    }

    private void RegisterProperty( INamedProperty property, bool userDefined )
    {
      if ( false == _properties.Add( property ) ) {
        throw new InvalidOperationException( "Exists property \"" + property.PropertyName + "\"." );
      }
      if ( userDefined ) Document.RegisterPropertyChange( property ) ;

      OnPropertyAdded( property );
    }
    public bool UnregisterProperty( string propName )
    {
      var property = _properties.Remove( propName );
      if ( null == property ) return false ;

      OnPropertyRemoved( property );

      return true;
    }

    public INamedProperty GetProperty( string propertyName )
    {
      return _properties[propertyName] ;
    }

    public string GetPropertyNameAt( int propertyIndex )
    {
      return _properties[propertyIndex].PropertyName;
    }

    public PropertyType GetPropertyTypeAt( int propertyIndex )
    {
      return _properties[propertyIndex].Type;
    }

    public IEnumerable<INamedProperty> GetProperties()
    {
      return _properties;
    }

    public int GetPropertyNameCount()
    {
      return _properties.Count;
    }

    public virtual IPropertiedElement GetElementByName( string objectName )
    {
      // 特にオーバーライドしない限り、子要素を探せない
      return null;
    }

    protected virtual void OnPropertyAdded( INamedProperty property )
    {
      PropertyAdded?.Invoke( this, new PropertyEventArgs( property ) );
    }

    protected virtual void OnPropertyRemoved( INamedProperty property )
    {
      PropertyRemoved?.Invoke( this, new PropertyEventArgs( property ) );
    }

    #endregion

    public override string ToString()
    {
      if ( string.IsNullOrWhiteSpace( ObjectName ) ) {
        return Name;
      }
      else {
        return $"{Name} ({ObjectName})";
      }
    }
  }
}