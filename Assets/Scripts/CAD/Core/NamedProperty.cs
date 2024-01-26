using System ;
using System.Collections.Generic ;
using System.ComponentModel ;
using System.Linq ;
using System.Reflection ;
using System.Text ;
using UnityEngine ;

namespace Chiyoda.CAD.Core
{
  /// <summary>
  /// 名前つきプロパティの型
  /// </summary>
  public enum PropertyType
  {
    TemporaryValue,
    GeneralDouble,
    GeneralInteger,
    Length,
    Angle,
    Boolean,
    DiameterRange,
  }

  /// <summary>
  /// 名前つきプロパティ
  /// </summary>
  public interface INamedProperty
  {
    /// <summary>
    /// プロパティ名。
    /// </summary>
    string PropertyName { get ; }

    /// <summary>
    /// プロパティ型。
    /// </summary>
    PropertyType Type { get ; }

    /// <summary>
    /// プロパティ値。
    /// </summary>
    double Value { get ; set ; }

    /// <summary>
    /// プロパティが取りうる値一覧(連続値ならnull)。
    /// </summary>
    IDictionary<string, double> EnumValues { get ; }

    /// <summary>
    /// プロパティが遅延されているかどうか。
    /// </summary>
    bool IsDelaying { get ; }

    /// <summary>
    /// 値の変更時の通知先ルールを設定する。
    /// </summary>
    /// <param name="rule">ルール</param>
    void AddAffectingRule( IRule rule ) ;

    /// <summary>
    /// 値の変更時の通知先ルールを削除する。
    /// </summary>
    /// <param name="rule">ルール</param>
    bool RemoveAffectingRule( IRule rule ) ;

    /// <summary>
    /// 値の変更イベントを強制発火する。
    /// </summary>
    void ForceTriggerChange() ;
  }

  /// <summary>
  /// 名前つきプロパティ (ユーザー定義版)
  /// </summary>
  public interface IUserDefinedNamedProperty : INamedProperty, IO.ISerializableObject
  {
    IUserDefinedNamedProperty Clone( IPropertiedElement owner, bool copyRules ) ;

    void ModifyFrom( IUserDefinedNamedProperty another ) ;

    void AddUserDefinedRule( IUserDefinedRule rule ) ;
    void RemoveUserDefinedRule( IUserDefinedRule rule ) ;
  }

  public interface IUserDefinedRangedNamedProperty : IUserDefinedNamedProperty
  {
    double MinValue { get ; set ; }
    double MaxValue { get ; set ; }
  }

  /// <summary>
  /// INamedPropertyのベース クラス
  /// </summary>
  public abstract class NamedPropertyBase : INamedProperty
  {
    private List<IRule> _affectingRules = null ;

    public string PropertyName { get ; }

    public PropertyType Type { get ; }

    public virtual IDictionary<string, double> EnumValues => null ;

    public virtual double Step => 0.0 ;

    public bool IsDelaying => false ;

    public double Value
    {
      get { return GetValue() ; }
      set
      {
        if ( ! SetValue( value ) ) return ;

        OnValueChanged() ;
      }
    }

    protected abstract double GetValue() ;
    protected abstract bool SetValue( double value ) ;
    
    protected abstract Document Document { get ; }

    protected NamedPropertyBase( string propertyName, PropertyType type )
    {
      PropertyName = propertyName ;
      Type = type ;
    }

    public void AddAffectingRule( IRule rule )
    {
      if ( null == _affectingRules ) {
        _affectingRules = new List<IRule> { rule } ;
      }
      else if ( false == _affectingRules.Contains( rule ) ) {
        _affectingRules.Add( rule ) ;
      }
    }

    public bool RemoveAffectingRule( IRule rule )
    {
      if ( null == _affectingRules ) {
        return false ;
      }
      else {
        return _affectingRules.Remove( rule ) ;
      }
    }

    public virtual void ForceTriggerChange()
    {
      OnValueChanged() ;
    }

    protected virtual void OnValueChanged()
    {
      if ( null != _affectingRules ) {
        var doc = Document ;
        foreach ( var rule in _affectingRules ) {
          doc.RegisterModifyingRule( rule );
        }
      }
    }
  }

  /// <summary>
  /// ユーザー定義名前つきプロパティ 基底クラス。
  /// </summary>
  public abstract class UserDefinedNamedPropertyBase : NamedPropertyBase, IUserDefinedNamedProperty
  {
    private readonly IPropertiedElement _owner ;
    protected readonly Memento<double> _value ;
    private List<IUserDefinedRule> _userDefinedRules = null ;

    protected override Document Document => _owner.Document ;

    protected UserDefinedNamedPropertyBase( IPropertiedElement owner, string propertyName, PropertyType type, double defaultValue )
      : base( propertyName, type )
    {
      _owner = owner ;
      _value = new Memento<double>( owner, defaultValue ) ;
    }

    public abstract IUserDefinedNamedProperty Clone( IPropertiedElement newOwner, bool copyRules ) ;

    public abstract void ModifyFrom( IUserDefinedNamedProperty another ) ;

    protected void CloneUserDefinedRules( UserDefinedNamedPropertyBase prop )
    {
      if ( null != prop._userDefinedRules ) {
        this._userDefinedRules = new List<IUserDefinedRule>( prop._userDefinedRules ) ;
      }
      else {
        this._userDefinedRules = null ;
      }
    }

    protected override double GetValue()
    {
      return _value.Value ;
    }

    protected abstract override bool SetValue( double value ) ;

    public void AddUserDefinedRule( IUserDefinedRule rule )
    {
      if ( null == _userDefinedRules ) {
        _userDefinedRules = new List<IUserDefinedRule>() ;
      }

      if ( _userDefinedRules.Contains( rule ) ) return ;

      _userDefinedRules.Add( rule ) ;
    }

    public void RemoveUserDefinedRule( IUserDefinedRule rule )
    {
      _userDefinedRules?.Remove( rule ) ;
    }

    protected override void OnValueChanged()
    {
      if ( null != _userDefinedRules ) {
        foreach ( var rule in _userDefinedRules ) {
          _owner.Document.RegisterModifyingRule( rule, _owner, this ) ;
        }
      }

      base.OnValueChanged() ;
    }
  }

  public abstract class UserDefinedNamedPropertyWithRange : UserDefinedNamedPropertyBase, IUserDefinedRangedNamedProperty
  {
    private readonly Memento<double> _minValue ;
    private readonly Memento<double> _maxValue ;

    public double MinValue
    {
      get => _minValue.Value ;
      set => _minValue.Value = value ;
    }

    public double MaxValue
    {
      get => _maxValue.Value ;
      set => _maxValue.Value = value ;
    }
    
    protected UserDefinedNamedPropertyWithRange( IPropertiedElement owner, string propertyName, PropertyType type, double defaultValue, double minValue, double maxValue )
      : base( owner, propertyName, type, defaultValue )
    {
      _minValue = new Memento<double>( owner, minValue ) ;
      _maxValue = new Memento<double>( owner, maxValue ) ;
    }

  }

  /// <summary>
  /// ユーザー定義名前つきプロパティ クラス。
  /// </summary>
  [IO.CustomSerializer( typeof( Serializers.UserDefinedNamedPropertySerializer ) )]
  public class UserDefinedNamedProperty : UserDefinedNamedPropertyWithRange
  {
    public UserDefinedNamedProperty( IPropertiedElement owner, string propertyName, PropertyType type, double defaultValue )
      : this( owner, propertyName, type, defaultValue, -double.MaxValue, +double.MaxValue )
    {
    }

    public UserDefinedNamedProperty( IPropertiedElement owner, string propertyName, PropertyType type, double defaultValue, double minValue, double maxValue )
      : base( owner, propertyName, type, defaultValue, minValue, maxValue )
    {
    }

    protected override bool SetValue( double value )
    {
      value = Math.Max( MinValue, Math.Min( MaxValue, value ) ) ;

      var result = ! MementoEqualityComparer<double>.Equals( _value.Value, value ) ;
      _value.Value = value ;
      return result ;
    }

    public override IUserDefinedNamedProperty Clone( IPropertiedElement newOwner, bool copyRules )
    {
      var prop = new UserDefinedNamedProperty( newOwner, PropertyName, Type, _value.Value, MinValue, MaxValue ) ;
      if ( copyRules ) prop.CloneUserDefinedRules( this ) ;
      return prop ;
    }

    public override void ModifyFrom( IUserDefinedNamedProperty another )
    {
      if ( another is UserDefinedNamedProperty udprop ) {
        MinValue = udprop.MinValue ;
        MaxValue = udprop.MaxValue ;
        Value = udprop.Value ;
      }
    }

    public static IEnumerable<KeyValuePair<string, double>> ToNameValueList<TEnum>() where TEnum : struct, Enum
    {
      foreach ( var name in Enum.GetNames( typeof( TEnum ) ) ) {
        if ( Enum.TryParse( name, out TEnum value ) ) {
          yield return new KeyValuePair<string, double>( DisplayName<TEnum>( name ), Convert.ToDouble( value ) ) ;
        }
      }
    }

    private static string DisplayName<TEnum>( string name ) where TEnum : struct, Enum
    {
      var fieldInfo = typeof( TEnum ).GetField( name ) ;
      var attr = fieldInfo.GetCustomAttributes<DescriptionAttribute>( false ).FirstOrDefault() ;
      if ( null != attr ) {
        return attr.Description ;
      }
      else {
        return name ;
      }
    }
  }

  /// <summary>
  /// ユーザー定義名前つきプロパティ クラス(enum版)。
  /// </summary>
  [IO.CustomSerializer( typeof( Serializers.UserDefinedEnumNamedPropertySerializer ) )]
  public class UserDefinedEnumNamedProperty : UserDefinedNamedPropertyBase
  {
    private readonly Dictionary<string, double> _dic ;

    public UserDefinedEnumNamedProperty( IPropertiedElement owner, string propertyName, PropertyType type, double defaultValue, IEnumerable<KeyValuePair<string, double>> enums )
      : base( owner, propertyName, type, defaultValue )
    {
      _dic = new Dictionary<string, double>() ;

      foreach ( var pair in enums ) {
        if ( ! _dic.ContainsKey( pair.Key ) ) {
          _dic.Add( pair.Key, pair.Value ) ;
        }
      }

      SetValue( defaultValue ) ;
    }

    public override IDictionary<string, double> EnumValues => _dic ;

    protected override bool SetValue( double value )
    {
      var minDiff = double.MaxValue ;
      double trueValue = value ;
      foreach ( var v in _dic.Values ) {
        var d = Math.Abs( value - v ) ;
        if ( d < minDiff ) {
          trueValue = v ;
          minDiff = d ;
        }
      }

      var result = ! MementoEqualityComparer<double>.Equals( _value.Value, trueValue ) ;
      _value.Value = trueValue ;
      return result ;
    }

    public override IUserDefinedNamedProperty Clone( IPropertiedElement newOwner, bool copyRules )
    {
      var prop = new UserDefinedEnumNamedProperty( newOwner, PropertyName, Type, _value.Value, _dic ) ;
      if ( copyRules ) prop.CloneUserDefinedRules( this ) ;
      return prop ;
    }

    public override void ModifyFrom( IUserDefinedNamedProperty another )
    {
      Value = another.Value ;
    }
  }

  /// <summary>
  /// ユーザー定義名前つきプロパティ クラス(stepつき版)。
  /// </summary>
  [IO.CustomSerializer( typeof( Serializers.UserDefinedSteppedNamedPropertySerializer ) )]
  public class UserDefinedSteppedNamedProperty : UserDefinedNamedPropertyWithRange
  {
    private readonly Memento<double> _step ;
    
    public override double Step => _step.Value ;

    public UserDefinedSteppedNamedProperty( IPropertiedElement owner, string propertyName, PropertyType type, double defaultValue, double minValue, double maxValue, double step )
      : base( owner, propertyName, type, defaultValue,minValue, maxValue )
    {
      _step = new Memento<double>( owner, step ) ;

      SetValue( defaultValue ) ;
    }

    protected override bool SetValue( double value )
    {
      var step = Step ;
      value = Math.Min( Math.Floor( Math.Max( value, MinValue ) / step + 0.5 ) * step, MaxValue ) ;

      var result = ! MementoEqualityComparer<double>.Equals( _value.Value, value ) ;
      _value.Value = value ;
      return result ;
    }

    public override IUserDefinedNamedProperty Clone( IPropertiedElement newOwner, bool copyRules )
    {
      var prop = new UserDefinedSteppedNamedProperty( newOwner, PropertyName, Type, _value.Value, MinValue, MaxValue, Step ) ;
      if ( copyRules ) prop.CloneUserDefinedRules( this ) ;
      return prop ;
    }

    public override void ModifyFrom( IUserDefinedNamedProperty another )
    {
      if ( another is UserDefinedSteppedNamedProperty stepped ) {
        MinValue = stepped.MinValue ;
        MaxValue = stepped.MaxValue ;
        _step.Value = stepped.Step ;
        Value = stepped.Value ;
      }
    }
  }

  /// <summary>
  /// 名前つきプロパティ クラス。
  /// </summary>
  public class NamedProperty : NamedPropertyBase
  {
    private readonly Func<double> _getter ;
    private readonly Action<double> _setter ;
    private double? _lastValue ;

    protected override Document Document { get ; }

    /// <summary>
    /// getterとsetterを指定するプロパティ。
    /// </summary>
    /// <param name="propertyName">プロパティ名。</param>
    /// <param name="type">プロパティ型。</param>
    /// <param name="getter">getter。</param>
    /// <param name="setter">setter。</param>
    public NamedProperty( Document doc, string propertyName, PropertyType type, Func<double> getter, Action<double> setter )
      : base( propertyName, type )
    {
      Document = doc ;
      _getter = getter ;
      _setter = setter ;
    }

    public override void ForceTriggerChange()
    {
      var value = _getter() ;
      var result = ( _lastValue == null || ! MementoEqualityComparer<double>.Equals( _lastValue.Value, value ) ) ;

      _lastValue = value ;
      if ( result ) {
        OnValueChanged() ;
      }
    }

    protected override double GetValue()
    {
      return _getter() ;
    }

    protected override bool SetValue( double value )
    {
      var result = ( _lastValue == null || ! MementoEqualityComparer<double>.Equals( _lastValue.Value, value ) ) ;

      _lastValue = value ;
      _setter( value ) ;
      return result ;
    }
  }

  /// <summary>
  /// 名前つきプロパティの遅延設定クラス
  /// </summary>
  public class DelayedNamedProperty : INamedProperty
  {
    private readonly Func<double> _getter ;
    private readonly Action<double> _setter ;
    private Action<INamedProperty> _onPropertyRegister ;
    private INamedProperty _baseProperty ;

    private readonly Document _document ;

    /// <summary>
    /// getterとsetterを指定するプロパティ。
    /// </summary>
    /// <param name="doc">ドキュメント。</param>
    /// <param name="propertyName">プロパティ名。</param>
    /// <param name="type">プロパティ型。</param>
    /// <param name="getter">getter。</param>
    /// <param name="setter">setter。</param>
    /// <param name="onPropertyRegister">プロパティ実体化時の処理。</param>
    public DelayedNamedProperty( Document doc, string propertyName, PropertyType type, Func<double> getter, Action<double> setter, Action<INamedProperty> onPropertyRegister )
    {
      _document = doc ;
      PropertyName = propertyName ;
      Type = type ;
      _getter = getter ;
      _setter = setter ;
      _onPropertyRegister = onPropertyRegister ;
    }

    public string PropertyName { get ; }

    public PropertyType Type { get ; }

    public double Value
    {
      get => BaseNamedProperty.Value ;
      set => BaseNamedProperty.Value = value ;
    }

    public IDictionary<string, double> EnumValues => null ;

    public bool IsDelaying => ( null == _baseProperty ) ;

    public void AddAffectingRule( IRule rule )
    {
      BaseNamedProperty.AddAffectingRule( rule ) ;
    }

    public void ForceTriggerChange()
    {
      _baseProperty?.ForceTriggerChange() ;
    }

    public bool RemoveAffectingRule( IRule rule )
    {
      if ( null == _baseProperty ) return false ;
      return _baseProperty.RemoveAffectingRule( rule ) ;
    }

    private INamedProperty BaseNamedProperty
    {
      get
      {
        if ( null == _baseProperty ) {
          _baseProperty = new NamedProperty( _document, PropertyName, Type, _getter, _setter ) ;
          _onPropertyRegister?.Invoke( _baseProperty ) ;
          _onPropertyRegister = null ;
        }

        return _baseProperty ;
      }
    }
  }

  /// <summary>
  /// 名前つきプロパティ クラス（Memento版）。
  /// </summary>
  public class NamedMementoProperty : NamedPropertyBase
  {
    private readonly Memento<double> _memento ;

    protected override Document Document { get ; }

    /// <summary>
    /// getterとsetterを指定するプロパティ。
    /// </summary>
    /// <param name="doc">ドキュメント。</param>
    /// <param name="propertyName">プロパティ名。</param>
    /// <param name="type">プロパティ型。</param>
    /// <param name="memento">Memento。</param>
    public NamedMementoProperty( Document doc, string propertyName, PropertyType type, Memento<double> memento )
      : base( propertyName, type )
    {
      Document = doc ;
      _memento = memento ;
      _memento.AfterNewlyValueChanged += ( sender, e ) =>
      {
        if ( MementoEqualityComparer<double>.Equals( e.OldValue, e.NewValue ) ) return ;
        OnValueChanged() ;
      } ;
    }

    protected override double GetValue()
    {
      return _memento.Value ;
    }

    protected override bool SetValue( double value )
    {
      var result = ! MementoEqualityComparer<double>.Equals( _memento.Value, value ) ;
      _memento.Value = value ;
      return result ;
    }
  }
}