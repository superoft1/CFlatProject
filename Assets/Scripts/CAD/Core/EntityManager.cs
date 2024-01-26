using System;
using System.Collections ;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Chiyoda.CAD.Model ;
using UnityEngine ;

namespace Chiyoda.CAD.Core
{
  static class EntityManager
  {
    private delegate void Setter<T>( T t );

    private delegate Model.Entity Generator( Document doc );

    private static readonly Dictionary<int, Generator> _generators = new Dictionary<int, Generator>();  // Enum キーだと1ケタ遅いので、Model.EntityType.Type ではなく int をキーに
    private static readonly Dictionary<Type, Generator> _generatorsForClass = new Dictionary<Type, Generator>();


    public static bool IsRegistered( Type t )
    {
      return _generatorsForClass.ContainsKey( t ) ;
    }

    public static Entity CreateEntity( Model.EntityType.Type entityType, Document doc )
    {
      if ( !_generators.TryGetValue( (int)entityType, out var generator ) ) {
        throw new ArgumentOutOfRangeException();
      }

      return CreateEntityFromGenerator( generator, doc ) ;
    }

    public static Entity CreateEntity( Type t, Document doc )
    {
      if ( !_generatorsForClass.TryGetValue( t, out var generator ) ) {
        throw new ArgumentOutOfRangeException();
      }

      return CreateEntityFromGenerator( generator, doc ) ;
    }

    public static T CreateEntity<T>( Document doc ) where T : Model.Entity
    {
      return CreateEntity( typeof( T ), doc ) as T;
    }

    internal static Entity CopyEntity( Entity entity, CopyObjectStorage storage, Document doc )
    {
      if ( !_generatorsForClass.TryGetValue( entity.GetType(), out var generator ) ) {
        throw new ArgumentOutOfRangeException();
      }

      var copied = generator( doc );
      storage.Register( entity, copied );

      using ( doc.History.SuppressRegister() ) {
        copied.CopyFrom( entity, storage );
        copied.RegisterNonMementoMembersFromDefaultObjects();
      }

      return copied;
    }

    private static Entity CreateEntityFromGenerator( Generator generator, Document doc )
    {
      var entity = generator( doc ) ;

      TestNoMementoObjects( entity ) ;
      
      using ( doc.History.SuppressRegister() ) {
        entity.InitializeDefaultObjects() ;
        entity.RegisterNonMementoMembersFromDefaultObjects() ;
      }

      return entity ;
    }

    /// <summary>
    /// ファイル読み込みのため、コンストラクタのみ実行。データ読み込み後、必ず必要に応じて<see cref="Entity.RegisterNonMementoMembersFromDefaultObjects"/>を呼び出すこと。
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static Entity CreateEntityRaw( Document doc, Type type )
    {
      if ( ! _generatorsForClass.TryGetValue( type, out var generator ) ) {
        throw new ArgumentOutOfRangeException() ;
      }

      return generator( doc ) ;
    }


    private static readonly HashSet<Type> _TestNoMementoObjects = new HashSet<Type>() ;

    [System.Diagnostics.Conditional( "UNITY_EDITOR" )]
    private static void TestNoMementoObjects( IMemorableObject entity )
    {
      if ( ! _TestNoMementoObjects.Add( entity.GetType() ) ) return ;

      // コンストラクタがMementoオブジェクトを作成していないかチェック

      for ( var type = entity.GetType() ; type != typeof( object ) ; type = type.BaseType ) {
        foreach ( var field in type.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) ) {
          if ( typeof( IMemorableObject ).IsAssignableFrom( field.FieldType ) && null != field.GetValue( entity ) ) {
            if ( typeof( Entity ) == type && field.Name == "_document" ) continue ;
            Debug.LogError( $"`{field.FieldType} {type}.{field.Name}' is initialized by a constructor. Use `InitializeDefaultObjects()' method!" ) ;
          }

          if ( typeof( IMemento ).IsAssignableFrom( field.FieldType ) && typeof( IEnumerable ).IsAssignableFrom( field.FieldType ) ) {
            foreach ( var value in (IEnumerable) field.GetValue( entity ) ) {
              if ( value is IMemorableObject ) {
                Debug.LogError( $"`{field.FieldType} {type}.{field.Name}' is initialized by a constructor. Use `InitializeDefaultObjects()' method!" ) ;
              }
              else if ( value.GetType().IsConstructedGenericType && typeof( KeyValuePair<,> ) == value.GetType().GetGenericTypeDefinition() ) {
                foreach ( var arg in value.GetType().GetGenericArguments() ) {
                  if ( typeof( IMemorableObject ).IsAssignableFrom( arg ) ) {
                    Debug.LogError( $"`{field.FieldType} {type}.{field.Name}' is initialized by a constructor. Use `InitializeDefaultObjects()' method!" ) ;
                  }
                }
              }

              break ;
            }
          }
        }
      }
    }


    static EntityManager()
    {
      foreach ( var type in Assembly.GetCallingAssembly().GetTypes() ) {
        if ( type.IsAbstract ) continue ;
        if ( ! type.IsSubclassOf( typeof( Entity ) ) ) continue ;
        
        var generator = CreateGenerator( type );
        if ( null == generator ) continue;

        _generatorsForClass.Add( type, generator );

        var attrs = type.GetCustomAttributes( typeof( EntityAttribute ), false );
        foreach ( EntityAttribute attr in attrs ) {
          var entityTypeInt = (int)attr.EntityType;
          if ( _generators.ContainsKey( entityTypeInt ) ) continue;

          _generators.Add( entityTypeInt, generator );
        }
      }
    }

    private static Generator CreateGenerator( Type classType )
    {
      var ctor = GetEntityConstructor( classType );
      if ( null == ctor ) return null;

      // TODO: Mementoの自動設定

      return ctor;
    }


    
    private static readonly Type[] ENTITY_CONSTRUCTOR_TYPES = { typeof( Document ) } ;

    private static Generator GetEntityConstructor( Type classType )
    {
      var ctor = classType.GetConstructor( ENTITY_CONSTRUCTOR_TYPES );
      if ( null == ctor ) return null;

      var parameters = Array.ConvertAll( ctor.GetParameters(), pi => Expression.Parameter( pi.ParameterType, pi.Name ) );

      return Expression.Lambda<Generator>(
                Expression.New( ctor, parameters ),
                parameters
            ).Compile();
    }

    private static Setter<T> GetSetter<T>( Type classType, string propName )
    {
      var prop = classType.GetProperty( propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
      if ( null == prop ) return null;

      if ( !prop.PropertyType.IsInstanceOfType( typeof( T ) ) ) return null;

      var methodInfo = prop.GetSetMethod( true );
      if ( null == methodInfo ) return null;

      return (Setter<T>)Delegate.CreateDelegate( typeof( Setter<T> ), methodInfo );
    }
  }
}
