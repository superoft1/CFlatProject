using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Electricals ;
using Chiyoda.CAD.Model.Structure;
using Chiyoda.CableRouting ;

namespace Chiyoda.CAD.Manager
{
  public class CableRoutingMgr
  {
    public void Test(Document doc, (IList<ICable> , IList<ICablePathAvailable>) field)
    {
      // ラックのcablePathを収集(内部ではエンティティを作成している)
      var cablePathList = CablePathUtil.CollectFrom(field.Item2);
      var cableList = field.Item1 ;
      
      var router = new CableRouter();
      var branchCablePathList = router.GetOrCreateBranchPath(cableList , cablePathList );
      
      // 更新する -> 存在しなければ、CADに要素作成
      foreach ( var branchCablePath in branchCablePathList ) {
        var entity = ElectricalsFactory.CreateCablePath( doc, branchCablePath ) ;
      }
    }

    public (IList<ICable>, IList<ICablePathAvailable>) SetupTestField( Document doc )
    {
// 機器類を配置
      var subStation = ElectricalsFactory.Create( doc, ElectricalDeviceType.Substation ) ;
      subStation.Position = new UnityEngine.Vector3d( 105, 85 ) ;
      subStation.Size = new UnityEngine.Vector3d( 10, 5, 3 ) ;

      var localPanel1 = ElectricalsFactory.Create( doc, ElectricalDeviceType.LocalPanel ) ;
      localPanel1.Position = new UnityEngine.Vector3d( 95, 60 ) ;
      localPanel1.Size = new UnityEngine.Vector3d( 1, 1.5, 2 ) ;

      var localPanel2 = ElectricalsFactory.Create( doc, ElectricalDeviceType.LocalPanel ) ;
      localPanel2.Position = new UnityEngine.Vector3d( 110, 40 ) ;
      localPanel2.Size = new UnityEngine.Vector3d( 1, 1.5, 2 ) ;

      var localPanel3 = ElectricalsFactory.Create( doc, ElectricalDeviceType.LocalPanel ) ;
      localPanel3.Position = new UnityEngine.Vector3d( 130, 55 ) ;
      localPanel3.Size = new UnityEngine.Vector3d( 1, 1.5, 2 ) ;
      localPanel3.Rotation = 90.0 ;

      // ケーブルを作成
      var cableList = new List<ICable>() ;
      var cableEntity1 = ElectricalsFactory.CreateCable( doc ) ;
      cableEntity1.SetFromTo( subStation, localPanel1 ) ;
      cableList.Add( cableEntity1 as ICable ) ;

      var cableEntity2 = ElectricalsFactory.CreateCable( doc ) ;
      cableEntity2.SetFromTo( subStation, localPanel2 ) ;
      cableList.Add( cableEntity2 as ICable ) ;

      var cableEntity3 = ElectricalsFactory.CreateCable( doc ) ;
      cableEntity3.SetFromTo( subStation, localPanel3 ) ;
      cableList.Add( cableEntity3 as ICable ) ;

      // ラックを配置
      var cablePathAvailableList = new List<ICablePathAvailable>() ;
      var rack1 = StructureFactory.CreatePipeRack( doc, PipeRackFrameType.Single ) ;
      rack1.FloorCount = 2 ;
      rack1.IntervalCount = 10 ;
      rack1.Position = new UnityEngine.Vector3d( 100, 20 ) ;
      cablePathAvailableList.Add( rack1 as ICablePathAvailable ) ;

      var rack2 = StructureFactory.CreatePipeRack( doc, PipeRackFrameType.Single ) ;
      rack2.FloorCount = 2 ;
      rack2.IntervalCount = 5 ;
      rack2.Position = new UnityEngine.Vector3d( 138, 45 ) ;
      rack2.Rotation = 90.0 ;
      cablePathAvailableList.Add( rack2 as ICablePathAvailable ) ;
      return (cableList, cablePathAvailableList) ;
    }

    public void Execute()
    {
      // ケーブルルーティングの実施

      // トレイの自動配置
    }
  }
}