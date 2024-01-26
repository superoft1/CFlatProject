using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;

namespace Chiyoda.CAD.Body
{
  public class SupportShapeBodyCreator : BodyCreator<SupportShape, Body>
  {
    public SupportShapeBodyCreator( Entity _entity ) : base( _entity )
    {}

    protected override bool RecreateOnUpdate { get { return true; } }
    protected override void SetupMaterials( Body body, SupportShape entity )
    {
      //// TODO: 支えるpipeをみてどのサポートタイプかが決定する
      ////var impl = body.MainObject.GetComponent<PipeShoeBodyImpl>();
      ////impl.Plate.GetComponent<MeshRenderer>().material = GetMaterial( entity );
      ////impl.WallLeft.GetComponent<MeshRenderer>().material = GetMaterial( entity );
      ////impl.WallRight.GetComponent<MeshRenderer>().material = GetMaterial( entity );
      //var cache = entity.Document.SupportPointCacheManager.GetCache(entity);
      //if (cache != null)
      //{
      //  var leafEdge = cache.LeafEdge;
      //  if(leafEdge.PipingPiece is PipingElbow45 || leafEdge.PipingPiece is PipingElbow90)
      //  {
      //    //var go = body.gameObject;
      //    //go.GetComponent<MeshRenderer>().material = GetMaterial( entity );
      //    //go.GetComponent<MeshRenderer>().material = GetMaterial( entity );
      //  }
      //  else
      //  {
      //    var impl = body.MainObject.GetComponent<TTypeSupportBodyImpl>();
      //    impl.Stantion.GetComponent<MeshRenderer>().material = GetMaterial( entity );
      //    impl.Shoulder.GetComponent<MeshRenderer>().material = GetMaterial( entity );
      //  }
      //}
    }

    protected override void SetupGeometry( Body body, SupportShape entity )
    {
      var go = body.gameObject;

      //var cache = entity.Document.PipeSupportCache.GetPipingPiece(entity);
      //if (cache != null)
      //{
      //  var leafEdge = cache.LeafEdge;
      //  if(leafEdge.PipingPiece is PipingElbow90 || leafEdge.PipingPiece is PipingElbow45)
      //  {
      //    var term = cache.EndPoint;
      //    if (entity.Parent is BlockPattern)
      //    {
      //      var block = entity.Parent as BlockPattern;
      //      term = block.LocalCod.LocalizePoint(term);
      //    }
      //    go.transform.localPosition = (Vector3)term;
      //    go.transform.localRotation = Quaternion.FromToRotation(go.transform.forward, Vector3.forward);

      //    var diameter = cache.Diameter;
      //    go.transform.localScale = new Vector3((float) (diameter), (float) diameter, (float) term.z) * ModelScale;
      //  }
      //  else
      //  {
      //    var term = cache.EndPoint;
      //    go.transform.localPosition = (Vector3)term;
      //    go.transform.localRotation =  Quaternion.FromToRotation(Vector3.right, (Vector3) leafEdge.GlobalCod.DirectionX);
      //    var q = Quaternion.FromToRotation(go.transform.forward, Vector3.forward);
      //    go.transform.localRotation *= q;

      //    var impl = body.MainObject.GetComponent<TTypeSupportBodyImpl>();
      //    var diameter = cache.Diameter;
      //    impl.Stantion.transform.localScale = new Vector3((float) (diameter), (float) diameter, (float) term.z) * ModelScale;
      //    impl.Shoulder.transform.localScale = new Vector3((float) (diameter), (float) diameter, (float) diameter) * ModelScale;
      //  }
      //}
    }

    //private CacheValue GetNearestPipingPiece(SupportShape entity)
    //{
    //  ////// 探索方向に存在しているエレメントを検索
    //  ////var dir = entity.direction;
    //  //return null;
    //}
  }
}
