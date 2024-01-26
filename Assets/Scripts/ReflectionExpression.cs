using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using System.Linq.Expressions ;

namespace Chiyoda
{
  public static class ReflectionExpression
  {
    private static readonly Dictionary<Type, Func<object>> _constructors = new Dictionary<Type, Func<object>>() ;

    public static object GetDefaultValue( this Type type )
    {
      if ( false == type.IsValueType ) return null ;

      return (type.GetDefaultConstructor())() ;
    }

    public static Type GetEnumerableItemType( this Type type )
    {
      var interfaces = type.FindInterfaces( ( t, x ) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof( IEnumerable<> ), null ) ;
      if ( 1 <= interfaces.Length ) return interfaces[ 0 ].GenericTypeArguments[ 0 ] ;

      if ( typeof( IEnumerable ).IsAssignableFrom( type ) ) return typeof( object ) ;

      return null ;
    }

    
    
    public static bool HasDefaultConstructor( this Type type )
    {
      return ( null != type.GetDefaultConstructor() ) ;
    }

    public static Func<object> GetDefaultConstructor( this Type type )
    {
      if ( _constructors.TryGetValue( type, out var action ) ) return action ;

      if ( type.IsValueType ) {
        action = Expression.Lambda<Func<object>>( Expression.Convert( Expression.New( type ), typeof( object ) ) ).Compile() ;
      }
      else {
        var ctor = type.GetConstructor( Array.Empty<Type>() ) ;
        if ( null != ctor ) {
          action = Expression.Lambda<Func<object>>( Expression.Convert( Expression.New( ctor ), typeof( object ) ) ).Compile() ;
        }
      }

      _constructors.Add( type, action ) ;

      return action ;
    }
  }
}