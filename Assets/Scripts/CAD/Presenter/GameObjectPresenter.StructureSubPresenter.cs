using System ;
using System.Linq ;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Structure;
using Chiyoda.CAD.Model.Structure.CommonEntities ;
using UnityEngine ;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter
  {
    private class StructureSubPresenter : SubPresenter<PlacementEntity>
    {
      public StructureSubPresenter( GameObjectPresenter basePresenter ) : base( basePresenter )
      {
      }

      protected override bool IsRaised( PlacementEntity src )
      {
        return ( src is Entity structure ) && BodyMap.ContainsBody( structure ) ;
      }

      protected override void Raise( PlacementEntity localCodEntity ) {}

      protected override void Update( PlacementEntity entity )
      {
        if ( ! ( entity is IStructure ) ) {
          SyncVisibility( entity );
          return ;
        }
        
        UpdateBody( entity );
        foreach ( var child in entity.Children.OfType<PlacementEntity>() ) {
          UpdateBody( child ) ;
          foreach ( var connector in child.Children.OfType<PlacementEntity>() ) {
            UpdateBody( connector ) ;
          }
        }
      }

      private void UpdateBody( PlacementEntity entity )
      {
        Body.Body structureObject ;
        if ( false == BodyMap.TryGetBody( entity, out var body ) ) {
          structureObject = BodyFactory.CreateBody( entity ) ;
          BodyMap.Add( entity, structureObject ) ;
        }
        else {
          var oldStructureObject = body as Body.Body ;
          structureObject = BodyFactory.UpdateBody( entity, oldStructureObject ) ;
          if ( structureObject != oldStructureObject ) {
            Destroy( entity ) ;
            BodyMap.Add( entity, structureObject ) ;
          }
        }

        var go = structureObject.gameObject ;
        if ( BodyMap.TryGetBody( entity.Parent as PlacementEntity, out var parentBody ) ) {
          go.transform.parent = ( (Body.Body) parentBody ).transform ;
          go.transform.SetLocalCodSys( entity.LocalCod ) ;
        }
        else {
          go.transform.parent = RootGameObject.transform ;
          go.transform.SetLocalCodSys( entity.LocalCod ) ;
        }
      }

      protected override void TransformUpdate( PlacementEntity structure )
      {
        if ( ! BodyMap.TryGetBody( structure, out var body ) ) {
          return ;
        }

        var go = ( body as Body.Body ).gameObject ;
        go.transform.localPosition = (Vector3) structure.LocalCod.Origin ;
        go.transform.localRotation = structure.LocalCod.Rotation ;
      }

      protected override void Destroy( PlacementEntity structure )
      {
        if ( BodyMap.TryGetBody( structure, out var body ) ) {
          BodyMap.Remove( structure ) ;
        }

        body.RemoveFromView() ;
      }
      
      private void SyncVisibility( Entity entity )
      {
        if ( !BodyMap.TryGetBody( entity, out var b ) || !(b is Body.Body body) ) {
          return ;
        }
        body.gameObject.SetActive( entity.IsVisible );
      }
    }
  }
}