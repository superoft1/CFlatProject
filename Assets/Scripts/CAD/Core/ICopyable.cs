using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chiyoda.CAD.Topology;

namespace Chiyoda.CAD.Core
{
  public interface ICopyable
  {
    void CopyFrom( ICopyable another, CopyObjectStorage storage );
  }

  public class CopyObjectStorage : IDisposable
  {
    private readonly Dictionary<object, object> _storage = new Dictionary<object, object>();
    private readonly Dictionary<INamedProperty, INamedProperty> _propMap = new Dictionary<INamedProperty, INamedProperty>();
    private readonly List<RuleList> _rulesToBind = new List<RuleList>();
    private Action _afterAll = null ;

    public void Register( object from, object to )
    {
      _storage.Add( from, to );
    }

    public void RegisterRuleList( RuleList ruleList )
    {
      _rulesToBind.Add( ruleList );
    }

    public T Get<T>( T oldObject )
      where T : class
    {
      if ( null == oldObject ) return null;

      if ( !_storage.TryGetValue( oldObject, out var another ) ) {
        return null;
      }

      return another as T;
    }

    public void RegisterAfterAll( Action action )
    {
      _afterAll += action ;
    }

    ~CopyObjectStorage()
    {
      throw new InvalidOperationException( "CopyObjectStorage is disposed without Dispose()." );
    }

    public void Dispose()
    {
      Dispose( true );
      GC.SuppressFinalize( this );
    }

    private void Dispose( bool disposing )
    {
      _afterAll?.Invoke() ;
      RePartnerVertices() ;
      BindRules();
    }

    private void RePartnerVertices()
    {
      foreach ( var pair in _storage ) {
        if ( ! ( pair.Key is HalfVertex v1 ) ) continue ;
        if ( null == v1.Partner ) continue ;

        if ( ! _storage.TryGetValue( v1.Partner, out var partner2 ) ) continue ;
        ( pair.Value as HalfVertex ).Partner = partner2 as HalfVertex ;
      }
    }

    private void BindRules()
    {
      foreach ( var rule in _rulesToBind ) {
        rule.BindChangeEvents( false );
      }
    }
  }


  public static class CopyableExtension
  {
    public static T Clone<T>( this T entity, CopyObjectStorage storage ) where T : class, IElement
    {
      if ( null == entity ) return null;

      if ( !( entity is Model.Entity source ) ) {
        throw new ArgumentException( $"`{entity.GetType().FullName}' is not a `Model.Entity`!" );
      }

      return source.Document.CopyEntity( source, storage ) as T;
    }

    public static T GetCopyObject<T>( this T entity, CopyObjectStorage storage ) where T : class
    {
      if ( null == entity ) return null;
      return storage.Get( entity );
    }

    public static T GetCopyObjectOrClone<T>( this T entity, CopyObjectStorage storage ) where T : class, IElement
    {
      if ( null == entity ) return null;
      return entity.GetCopyObject( storage ) ?? entity.Clone( storage );
    }
    
    public static T[] Clone<T>( this T[] entities, CopyObjectStorage storage ) where T : class, IElement
    {
      return Array.ConvertAll( entities, e => e.Clone( storage ) ) ;
    }

    public static T[] GetCopyObjectArray<T>( this T[] entities, CopyObjectStorage storage ) where T : class
    {
      if ( null == entities ) return null;
      return Array.ConvertAll( entities, e => e.GetCopyObject( storage ) ) ;
    }

    public static T[] GetCopyObjectOrClone<T>( this T[] entities, CopyObjectStorage storage ) where T : class, IElement
    {
      if ( null == entities ) return null;
      return Array.ConvertAll( entities, e => e.GetCopyObjectOrClone( storage ) ) ;
    }

    public static void CloneFrom<T>( this MementoList<T> list, IEnumerable<T> another, CopyObjectStorage storage ) where T : class, IElement
    {
      list.CopyFrom( another.Select( s => s.Clone( storage ) ) );
    }
    public static void SetCopyObjectFrom<T>( this MementoList<T> list, IEnumerable<T> another, CopyObjectStorage storage ) where T : class
    {
      list.CopyFrom( another.Select( s => s.GetCopyObject( storage ) ) );
    }
    public static void SetCopyValueFrom<T>( this MementoList<T> list, IEnumerable<T> another ) where T : struct
    {
      list.CopyFrom( another );
    }
    public static void SetCopyObjectOrCloneFrom<T>( this MementoList<T> list, IEnumerable<T> another, CopyObjectStorage storage ) where T : class, IElement
    {
      list.CopyFrom( another.Select( s => s.GetCopyObjectOrClone( storage ) ) );
    }

    public static void CloneFrom<T>( this MementoSet<T> set, IEnumerable<T> another, CopyObjectStorage storage ) where T : class, IElement
    {
      set.CopyFrom( another.Select( s => s.Clone( storage ) ) );
    }
    public static void SetCopyObjectFrom<T>( this MementoSet<T> set, IEnumerable<T> another, CopyObjectStorage storage ) where T : class
    {
      set.CopyFrom( another.Select( s => s.GetCopyObject( storage ) ) );
    }
    public static void SetCopyObjectFrom<T>( this MementoSet<T> set, IEnumerable<T> another ) where T : struct
    {
      set.CopyFrom( another );
    }
    public static void SetCopyObjectOrCloneFrom<T>( this MementoSet<T> set, IEnumerable<T> another, CopyObjectStorage storage ) where T : class, IElement
    {
      set.CopyFrom( another.Select( s => s.GetCopyObjectOrClone( storage ) ) );
    }

    public static void CloneFrom<TKey, TValue>( this MementoDictionary<TKey, TValue> dic, IEnumerable<KeyValuePair<TKey, TValue>> another, CopyObjectStorage storage ) where TValue : class, IElement
    {
      dic.CopyFrom( another.Select( pair => new KeyValuePair<TKey, TValue>( pair.Key, pair.Value.Clone( storage ) ) ) ) ;
    }

    public static void SetCopyObjectFrom<TKey, TValue>( this MementoDictionary<TKey, TValue> dic, IEnumerable<KeyValuePair<TKey, TValue>> another, CopyObjectStorage storage ) where TValue : class
    {
      dic.CopyFrom( another.Select( pair => new KeyValuePair<TKey, TValue>( pair.Key, pair.Value.GetCopyObject( storage ) ) ) ) ;
    }
    
    public static void SetCopyValueFrom<TKey, TValue>( this MementoDictionary<TKey, TValue> dic, IEnumerable<KeyValuePair<TKey, TValue>> another ) where TValue : struct
    {
      dic.CopyFrom( another ) ;
    }

    public static void SetCopyObjectOrCloneFrom<TKey, TValue>( this MementoDictionary<TKey, TValue> dic, IEnumerable<KeyValuePair<TKey, TValue>> another, CopyObjectStorage storage ) where TValue : class, IElement
    {
      dic.CopyFrom( another.Select( pair => new KeyValuePair<TKey, TValue>( pair.Key, pair.Value.GetCopyObjectOrClone( storage ) ) ) ) ;
    }
  }
}
