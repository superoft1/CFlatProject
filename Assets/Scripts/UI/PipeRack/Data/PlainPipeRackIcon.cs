using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Structure.CommonEntities ;

namespace Chiyoda.UI
{
  public class PlainPipeRackIcon : PipeRackIcon
  {
    protected override void CreateInitialElement( Document document, Action<PlacementEntity> onFinish )
    {
      var structure = StructureFactory.CreateEquipmentStructure( document ) ;
      
      onFinish( structure as PlacementEntity ) ;
    }
  }
}