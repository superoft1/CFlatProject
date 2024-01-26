using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Structure;
using Chiyoda.CAD.Model.Structure.CommonEntities ;

namespace Chiyoda.UI
{
  public class SingleSpanPipeRackIcon : PipeRackIcon
  {
    private IPipeRack _rack ;

    protected override void CreateInitialElement( Document document, Action<PlacementEntity> onFinish )
    {
      _rackSettingDialog.SetInitialValue( 10, 4, 6.0, 6.0 ) ;
      _rack = StructureFactory.CreatePipeRack( document, PipeRackFrameType.Single ) ;
      _rack.SetStandardBraces() ;

      onFinish( _rack as PlacementEntity ) ;
    }

    protected override void AddValueChangedEventHandler()
    {
      if ( _rackSettingDialog.ValueChangeHandler != null ) {
        return ;
      }

      _rackSettingDialog.ValueChangeHandler += ( s, e ) => 
      {
        ( (Entity) _rack ).Name = _rackSettingDialog.StructureId ;
        if ( _rackSettingDialog.HasError ) {
          return ;
        }
        _rack.IntervalCount = _rackSettingDialog.Interval ;
        _rack.FloorCount = _rackSettingDialog.FloorCount;
        _rack.IsHalfDownSideBeam = _rackSettingDialog.HalfDownSideBeam ;
        _rack.SetWidthAndStandardMaterials( _rackSettingDialog.Width, _rackSettingDialog.BeamInterval ) ;

        _rack.SetStandardBraces() ;
      } ;
    } 
  }

}