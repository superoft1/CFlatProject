using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Reflection ;
using System.Text.RegularExpressions ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using JetBrains.Annotations ;

namespace Chiyoda.CAD.IO
{
  public static class TypeManager
  {
    private static readonly Dictionary<Type, string> _typeNames = new Dictionary<Type, string>() ;
    private static readonly Dictionary<Type, TypeField[]> _typeFields = new Dictionary<Type, TypeField[]>() ;
    private static readonly Dictionary<Type, (Func<IElement, object>, Action<IElement, object>)> _instantiators = new Dictionary<Type, (Func<IElement, object>, Action<IElement, object>)>() ;
    private static readonly Dictionary<string, Type> _nameTypes = new Dictionary<string, Type>() ;
    private static readonly Regex _toSerializeNameRegex = new Regex( @"\bChiyoda\.\b", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled ) ;
    private static readonly Regex _fromSerializeNameRegex = new Regex( @"\bVTP\.\b", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled ) ;


    public static string GetTypeName( Type type )
    {
      if ( _typeNames.TryGetValue( type, out var name ) ) {
        return name ;
      }

      name = ToSerializeName( type ) ;

      _typeNames.Add( type, name ) ;

      return name ;
    }

    public static Type FromTypeName( string name )
    {
      if ( _nameTypes.TryGetValue( name, out var type ) ) {
        return type ;
      }

      var trueName = FromSerializeName( name ) ;

      type = Type.GetType( trueName ) ;
      
      _nameTypes.Add( name, type ) ;

      return type ;
    }

    private static string ToSerializeName( Type type )
    {
      // 『Chiyoda』→『VTP』
      return _toSerializeNameRegex.Replace( type.FullName, "VTP." ) ;
    }
    private static string FromSerializeName( string name )
    {
      // 『VTP』→『Chiyoda』
      return _fromSerializeNameRegex.Replace( name, "Chiyoda." ) ;
    }
    

    public static IEnumerable<TypeField> GetFields( Type type )
    {
      if ( type.IsInterface ) return Array.Empty<TypeField>() ;

      var list = new List<TypeField>() ;
      foreach ( var t in GetAllBaseTypes( type ).Reverse() ) {
        if ( false == _typeFields.TryGetValue( t, out var value ) ) {
          value = CreateTypeFields( t ).ToArray() ;
          _typeFields.Add( t, value ) ;
        }

        list.AddRange( value ) ;
      }

      return list ;
    }

    private static IEnumerable<TypeField> CreateTypeFields( Type type )
    {
      foreach ( var field in type.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) ) {
        if ( field.DeclaringType != type ) continue ;
        if ( typeof( Document ) == field.FieldType ) continue ;

        var typeField = TypeField.Create( field ) ;
        if ( null == typeField ) continue ;
        
        yield return typeField ;
      }
    }

    private static IEnumerable<Type> GetAllBaseTypes( [NotNull] Type type )
    {
      for ( ; type != typeof( object ) ; type = type.BaseType ) {
        yield return type ;
      }
    }


    public static (Func<IElement, object>, Action<IElement, object>) GetInstantiator( Type type )
    {
      if ( false == _instantiators.TryGetValue( type, out var pair ) ) {
        pair = ( CreateInstantiator( type ), CreateAfterInstantiate( type ) ) ;
        _instantiators.Add( type, pair ) ;
      }

      return pair ;
    }

    private static Func<IElement, object> CreateInstantiator( Type type )
    {
      if ( type.IsAbstract || type.IsInterface ) return null ;
      
      if ( EntityManager.IsRegistered( type ) ) {
        return parent => EntityManager.CreateEntityRaw( parent.Document, type ) ;
      }

      if ( typeof( Document ) == type ) {
        return parent => DocumentCollection.Instance.CreateNew() ;
      }
      if ( typeof( ConnectPoint ) == type ) {
        return parent => new ConnectPoint( (PipingPiece) parent, 0 ) ;
      }

      // TODO: その他の型

      return null ;
    }

    private static Action<IElement, object> CreateAfterInstantiate( Type type )
    {
      if ( type.IsAbstract || type.IsInterface ) return null ;

      if ( EntityManager.IsRegistered( type ) ) {
        return ( doc, obj ) => ( (Entity) obj ).RegisterNonMementoMembersFromDefaultObjects() ;
      }
      
      // TODO: その他

      return null ;
    }
  }
}