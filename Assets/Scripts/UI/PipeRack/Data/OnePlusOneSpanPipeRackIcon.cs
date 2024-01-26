using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Structure;
using Chiyoda.CAD.Model.Structure.CommonEntities ;

namespace Chiyoda.UI
{
  public class OnePlusOneSpanPipeRackIcon : PipeRackIcon
  {
    protected override void CreateInitialElement( Document document, Action<PlacementEntity> onFinish )
    {
      var rack = StructureFactory.CreatePipeRack( document, PipeRackFrameType.OnePlusOne ) ;
      rack.FloorCount = 4 ;
      rack.IntervalCount = 10 ;
      
      onFinish( rack as PlacementEntity ) ;
    }
  }
}