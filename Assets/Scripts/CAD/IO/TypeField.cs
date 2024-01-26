using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq.Expressions ;
using System.Reflection ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.IO
{
  public abstract class TypeField
  {
    public static TypeField Create( FieldInfo field )
    {
      if ( null != field.GetCustomAttribute<NonSerializedAttribute>() ) return null ;

      var fieldType = field.FieldType ;
      if ( typeof( Delegate ).IsAssignableFrom( fieldType ) ) return null ;

      if ( typeof( IMemento ).IsAssignableFrom( fieldType ) ) {
        return CreateForMemento( field ) ;
      }

      if ( typeof( ISerializableObject ).IsAssignableFrom( fieldType ) ) {
        return CreateForNonMemento( field ) ;
      }
      if ( typeof( ISerializableList ).IsAssignableFrom( fieldType ) ) {
        return CreateForNonMemento( field ) ;
      }
      if ( null != fieldType.GetCustomAttribute<CustomSerializerAttribute>() ) {
        return CreateForNonMemento( field ) ?? CreateForReadOnlyNonMemento( field ) ;
      }

      if ( null != field.GetCustomAttribute<AutoSerializedAttribute>() ) {
        if ( typeof( IEnumerable ).IsAssignableFrom( fieldType ) ) {
          return CreateForEnumerable( field ) ;
        }
        else {
          return CreateForNonMemento( field ) ;
        }
      }

      return null ;
    }

    private static TypeField CreateForNonMemento( FieldInfo field )
    {
      if ( field.IsInitOnly ) return null ;

      return new SingleTypeField( field ) ;
    }

    private static TypeField CreateForReadOnlyNonMemento( FieldInfo field )
    {
      return new SingleTypeField( field ) ;
    }

    private static TypeField CreateForEnumerable( FieldInfo field )
    {
      if ( field.IsInitOnly ) {
        if ( field.FieldType.IsArray ) return null ; // readonly配列は保存不可

        return new ReadOnlyEnumerableTypeField( field ) ;
      }

      return new SingleTypeField( field ) ;
    }

    private static TypeField CreateForMemento( FieldInfo field )
    {
      if ( typeof( ISerializableList ).IsAssignableFrom( field.FieldType ) ) {
        return new MementoCollectionField( field ) ;
      }
      return new MementoTypeField( field ) ;
    }

    private static Func<object, object> GetFieldValue( FieldInfo fieldInfo )
    {
      var arg = Expression.Parameter( typeof( object ), "arg" ) ;
      var getter = Expression.Convert( Expression.Field( Expression.Convert( arg, fieldInfo.DeclaringType ), fieldInfo ), typeof( object ) ) ;
      return Expression.Lambda<Func<object, object>>( getter, arg ).Compile() ;
    }

    private static Action<object, object> SetFieldValue( FieldInfo fieldInfo )
    {
      if ( fieldInfo.IsInitOnly ) return null ;
      
      var arg = Expression.Parameter( typeof( object ), "arg" ) ;
      var value = Expression.Parameter( typeof( object ), "value" ) ;
      var setter = Expression.Assign( Expression.Field( Expression.Convert( arg, fieldInfo.DeclaringType ), fieldInfo ), Expression.Convert( value, fieldInfo.FieldType ) ) ;
      return Expression.Lambda<Action<object, object>>( setter, arg, value ).Compile() ;
    }



    private static readonly char[] SPLITTER = { '_' } ;

    public string Name { get ; }

    public bool IsLateSerialize { get ; }

    public abstract Type SerializeType { get ; }
    
    public abstract object GetValue( object obj ) ;
    
    public abstract bool DeserializeWithSetValue { get ; }

    public abstract bool SetValue( object obj, object value ) ;

    private TypeField( FieldInfo fieldInfo )
    {
      IsLateSerialize = ( null != fieldInfo.GetCustomAttribute<LateSerializeAttribute>() ) ;
      Name = GetFieldName( fieldInfo.Name ) ;
    }

    private static string GetFieldName( string name )
    {
      var propName = GetFrontPropertyNameOfBackingField( name ) ;
      if ( null != propName ) name = propName ;
      return string.Join( "", Array.ConvertAll( name.Split( SPLITTER ), FirstLetterToUpperCase ) ) ;
    }

    private static string GetFrontPropertyNameOfBackingField( string name )
    {
      if ( '<' != name[ 0 ] ) return null ;

      var index = name.IndexOf( '>' ) ;
      if ( index < 0 ) throw new ArgumentException( "Bad backing field name." ) ;

      return name.Substring( 1, index - 1 ) ;
    }

    private static string FirstLetterToUpperCase( string str )
    {
      if ( string.IsNullOrEmpty( str ) ) return string.Empty ;
      return char.ToUpperInvariant( str[ 0 ] ) + str.Substring( 1 ) ;
    }



    #region Subclasses

    private class MementoTypeField : TypeField
    {
      private readonly Type _mementoValueType ;
      private readonly Func<object, object> _fieldGetter ;
      
      public MementoTypeField( FieldInfo fieldInfo ) : base( fieldInfo )
      {
        _mementoValueType = fieldInfo.FieldType.GenericTypeArguments[ 0 ] ;
        _fieldGetter = GetFieldValue( fieldInfo ) ;
      }

      public override Type SerializeType => _mementoValueType ;

      public override object GetValue( object obj ) => ((IMementoValue)_fieldGetter( obj )).Value ;

      public override bool DeserializeWithSetValue => true ;

      public override bool SetValue( object obj, object value )
      {
        if ( false == _mementoValueType.IsInstanceOfType( value ) ) return false ;

        ( (IMementoValue) _fieldGetter( obj ) ).Value = value ;
        return true ;
      }
    }

    private class MementoCollectionField : TypeField
    {
      private readonly Func<object, object> _fieldGetter ;

      public MementoCollectionField( FieldInfo fieldInfo ) : base( fieldInfo )
      {
        _fieldGetter = GetFieldValue( fieldInfo ) ;
        SerializeType = fieldInfo.FieldType ;
      }

      public override Type SerializeType { get ; }

      public override object GetValue( object obj ) => _fieldGetter( obj ) ;

      public override bool DeserializeWithSetValue => true ;

      public override bool SetValue( object obj, object value )
      {
        var fieldValue = _fieldGetter( obj ) as ISerializableList;
        if ( null == fieldValue ) return false ;

        fieldValue.Clear() ;
        if ( null == value ) return true ;

        if ( ! ( value is IEnumerable enu ) ) return false ;

        foreach ( var item in enu ) {
          if ( false == fieldValue.ItemType.IsInstanceOfType( item ) ) return false ;
          fieldValue.Add( item ) ;
        }

        return true ;
      }
    }
    
    private class SingleTypeField : TypeField
    {
      private readonly Func<object, object> _fieldGetter ;
      private readonly Action<object, object> _fieldSetter ;

      public SingleTypeField( FieldInfo fieldInfo ) : base( fieldInfo )
      {
        SerializeType = fieldInfo.FieldType ;
        _fieldGetter = GetFieldValue( fieldInfo ) ;
        _fieldSetter = SetFieldValue( fieldInfo ) ;
      }

      public override Type SerializeType { get ; }

      public override object GetValue( object obj ) => _fieldGetter( obj ) ;

      public override bool DeserializeWithSetValue => ( null != _fieldSetter ) ;

      public override bool SetValue( object obj, object value )
      {
        if ( false == DeserializeWithSetValue ) throw new NotSupportedException() ;
        
        if ( false == SerializeType.IsInstanceOfType( value ) ) return false ;

        _fieldSetter( obj, value ) ;
        return true ;
      }
    }

    private class ReadOnlyEnumerableTypeField : TypeField
    {
      private readonly Func<object, object> _fieldGetter ;

      public ReadOnlyEnumerableTypeField( FieldInfo fieldInfo ) : base( fieldInfo )
      {
        _fieldGetter = GetFieldValue( fieldInfo ) ;
        SerializeType = fieldInfo.FieldType ;
      }

      public override Type SerializeType { get ; }

      public override bool DeserializeWithSetValue => false ;

      public override object GetValue( object obj ) => _fieldGetter( obj ) ;

      public override bool SetValue( object obj, object value )
      {
        throw new NotSupportedException() ;
      }
    }

    #endregion
  }
}