using System ;
using System.Collections.Generic ;
using Chiyoda.CAD.Body ;
using Chiyoda.CAD.Model ;

namespace Chiyoda.CAD.Manager
{
  public class BodyMap
  {
    private static BodyMap _instance ;

    public static BodyMap Instance
    {
      get
      {
        if ( null == _instance ) _instance = new BodyMap() ;
        return _instance ;
      }
    }

    private readonly Dictionary<Entity, IBody> _entityBodyMap = new Dictionary<Entity, IBody>() ;

    private BodyMap()
    {
    }

    public void Add( Entity entity, IBody body )
    {
      if ( _entityBodyMap.ContainsKey( entity ) ) {
        throw new InvalidOperationException( "entity is already registered" ) ;
      }

      if ( null != body ) {
        if ( null != body.Entity ) {
          throw new InvalidOperationException( "body.Entity is not null" ) ;
        }

        body.Entity = entity ;
      }

      _entityBodyMap.Add( entity, body ) ;
    }

    public void Remove( Entity entity )
    {
      if ( null == entity ) {
        throw new ArgumentNullException( nameof( entity ) ) ;
      }

      if ( ! _entityBodyMap.TryGetValue( entity, out var body ) ) {
        throw new InvalidOperationException( "entity not found" ) ;
      }

      body?.RemoveFromView() ;

      _entityBodyMap.Remove( entity ) ;
    }

    public bool TryGetBody( Entity entity, out IBody body )
    {
      if ( null == entity ) {
        body = null ;
        return false ;
      }

      return _entityBodyMap.TryGetValue( entity, out body ) ;
    }

    public bool ContainsBody( Entity entity )
    {
      TryGetBody( entity, out var body ) ;
      return ( null != body ) ;
    }
  }
}