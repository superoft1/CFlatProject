using System.Collections.Generic ;
using System.Linq ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Structure ;
using Routing ;
using UnityEngine;
using Routing.Model;

namespace VtpAutoRouting
{
  internal static class AutoRoutingModelFactory
  {
    public static IList<APipeRack> GetCurrentAllRacks()
    {
      return CreateRackData( DocumentCollection.Instance.Current.Structures.OfType<IPipeRack>() ) ;
    }
    
    private static IList<APipeRack> CreateRackData( IEnumerable<IPipeRack> pipeRacks )
    {
      var rackAutoRoutingRackPairs = pipeRacks.Select( p =>
        {
          var routingPipeRack = Get( p );
          if (routingPipeRack == null)
          {
            Debug.LogError("Failed to create Auto Rouging Pipe RackUnit");
            return (p, null);
          }
          return (p, routingPipeRack);
        }).Where( tuple => tuple.routingPipeRack != null ).ToList();
      ConnectPipeRackEachOthers( rackAutoRoutingRackPairs ) ;
      return rackAutoRoutingRackPairs.Select( item => item.routingPipeRack ).ToList() ;
    }
    
    private static void ConnectPipeRackEachOthers( IList<(IPipeRack, APipeRack)> pairs )
    {
      foreach ( var (pipeRack1, rack1) in pairs ) {
        foreach ( var (pipeRack2, rack2) in pairs ) {
          if ( pipeRack1 == pipeRack2 ) continue ;

          // すでにリンク解決済み
          if ( rack1.ConnectedRacks.Contains( rack2 ) ) {
            continue ;
          }

          // 各ラック、長手方向に2xPipeInterval分だけBBを伸ばし、交差したら、リンクしているとする
          // 4つ又の場合、向いあうラックが離れているので、２ｘビームインターバル程度必要
          rack1.TryLinking( rack2, 2f * (float) pipeRack1.BeamInterval, 2f * (float) pipeRack2.BeamInterval ) ;
        }

        // 他と何も接続できない場合は、横方向にも拡張する
        if ( ! rack1.ConnectedRacks.Any() ) {
          foreach ( var (pipeRack2, rack2) in pairs ) {
            if ( pipeRack1 == pipeRack2 ) continue ;
            
            // すでにリンク解決済み
            if ( rack1.ConnectedRacks.Contains( rack2 ) ) {
              continue ;
            }

            // 各ラック、長手方向にPipeInterval, 幅方向にラック幅の半分だけBBを伸ばし、交差したら、リンクしているとする
            rack1.TryLinking( rack2,
              new Vector2( (float) pipeRack1.BeamInterval, 0.5f * (float) pipeRack1.Width ),
              new Vector2( (float) pipeRack2.BeamInterval, 0.5f * (float) pipeRack2.Width ) ) ;
          }
        }
        /* 何かリンク関係がおかしい場合ログ表示する
        var msg = $"{pipeRack1.AutoRoutingPipeRack.Name} is connected to ..." + System.Environment.NewLine;
        foreach ( var conRack in pipeRack1.AutoRoutingPipeRack.ConnectedRacks ) {
          msg += conRack.ToString() + System.Environment.NewLine;
        }
        Debug.Log( msg );
        */
      }
    }
    
    private static APipeRack Get( IPipeRack rack )
    {
      var length = (float) rack.BeamInterval * rack.IntervalCount;
      var condition = GetCondition(length, rack.Rotation );
      var name = ( rack is Entity e ) ? e.Name : "Anonymous Rack" ;
      
      if (condition.vec == Vector3.zero) {
        return Factory.CreateAutoRoutingPipeRack( name ) ;
      }

      var l = (float) rack.BeamInterval * rack.IntervalCount;
      var w = (float) rack.Width;
      var dim = (condition.useX) ? new Vector2(l, w) : new Vector2(w, l);
      var center = CalcCenterPosition( rack.Position, condition, dim);
      var size = new Vector3(dim.x, dim.y, (float) rack.BeamHeight);
      var layers = Enumerable.Range( 0, rack.FloorCount ).Select( i =>
        Routing.Factory.CreateAutoRoutingLayer( rack.GetFloorHeight( i ), center, condition, size  ) ) ;
      return Routing.Factory.CreateAutoRoutingPipeRack( name, layers,true ) ;
    }
    

    private static Vector2 CalcCenterPosition(
      Vector3d position, (bool useX, bool isReverse, Vector3 vec) condition, Vector2 dim)
    {
      var sign = condition.isReverse ? -1.0f : 1.0f;
      var offsetSign = (condition.useX) ? new Vector2(-1.0f, 1.0f) : new Vector2(1.0f, 1.0f);

      var offset = new Vector2(sign * offsetSign.x * dim.x, sign * offsetSign.y * dim.y);
      var center = new Vector2(
        (float) (position.x + 0.5f * offset.x),
        (float) (position.y + 0.5f * offset.y));
      return center;
    }

    private static (bool useX, bool isReverse, Vector3 vec) GetCondition( double length, double rotation )
    {
      const float tol = 1.0e-2f;
      switch (rotation)
      {
        case double v when ((v%360) < tol):
          return (false, false, Vector3.up);
        case double v when (((v-90)%360 < tol)):
          return (true, false, Vector3.right);
        case double v when (((v-180)%360 < tol)):
          return (false, true, Vector3.down);
        case double v when (((v-270)%360 < tol)):
          return (true, true, Vector3.left);
        default:
          return (false, false, Vector3.zero);      
      }
    }
  }
}