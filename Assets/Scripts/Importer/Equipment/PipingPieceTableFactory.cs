using System.Data ;
using Chiyoda.CAD.BP ;

namespace Importer.Equipment
{
  public static class PipingPieceTableFactory
  {
    public static PipingPieceTableImporter Create(BlockPatternType.Type type, DataSet dataSet)
    {
      switch ( type ) {
        case BlockPatternType.Type.Column:
          return new ColumnTableImporterImporter( dataSet ) ;
        case BlockPatternType.Type.AirFinCooler:
          return new AirFinCoolerTableImporter( dataSet ) ;
        case BlockPatternType.Type.GenericEquipment:
          return new GenericEquipmentTableImporter(dataSet);
        case BlockPatternType.Type.Chiller:
          return new ChillerTableImporter( dataSet ) ;
        case BlockPatternType.Type.ConeRoofTypeTank:
          return new ConeRoofTypeTankTableImporter( dataSet ) ;
        case BlockPatternType.Type.HorizontalHeatExchanger:
          return new HorizontalHeatExchangerTableImporter(dataSet);
        case BlockPatternType.Type.EndTopTypePump:
        case BlockPatternType.Type.TopTopTypePump:
        case BlockPatternType.Type.SideSideTypePump:
          return new HorizontalPumpTableImporter(dataSet);
        case BlockPatternType.Type.HorizontalVessel:
          return new HorizontalVesselTableImporter( dataSet );
        case BlockPatternType.Type.KettleTypeHeatExchanger:
          return new KettleTypeHeatExchangerTableImporter(dataSet);
        case BlockPatternType.Type.LegTypeVessel:
          return new LegTypeVesselTableImporter(dataSet);
        case BlockPatternType.Type.PlateTypeHeatExchanger:
          return new PlateTypeHeatExchangerTableImporter(dataSet);
        case BlockPatternType.Type.SkirtTypeVessel:
          return new SkirtTypeVesselTableImporter(dataSet);
        case BlockPatternType.Type.SphericalTypeTank:
          return new SphericalTypeTankTableImporter(dataSet);
        case BlockPatternType.Type.VerticalHeatExchanger:
          return new VerticalHeatExchangerTableImporter(dataSet);
        case BlockPatternType.Type.VerticalPump:
          return new VerticalPumpTableImporter(dataSet);
        case BlockPatternType.Type.Filter:
          return new FilterTableImporter(dataSet);
        case BlockPatternType.Type.PressureReliefValve:
          return new PressureReliefValveTableImporter( dataSet ) ;
        case BlockPatternType.Type.ControlValve:
          return new ControlValveTableImporter( dataSet );
        case BlockPatternType.Type.ActuatorControlValve:
          return new ActuatorControlValveTableImporter( dataSet );
        }

      return null ;
    }
  }
}