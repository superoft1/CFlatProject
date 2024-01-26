using System;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Electricals ;
using Chiyoda.CAD.Model.Routing;
using Chiyoda.CAD.Plotplan;
using Chiyoda.CAD.Model.Structure;
using Chiyoda.CAD.Topology;

namespace Chiyoda.CAD.Body
{
  public class BodyFactory
  {
    public static Body CreateBody( Entity entity )
    {
      var bodyCreator = CreateBodyCreator( entity );
      return bodyCreator?.Create();
    }

    public static Body UpdateBody( Entity entity, Body body )
    {
      if ( null == body ) return CreateBody( entity );

      var bodyCreator = CreateBodyCreator( body.Entity );
      return bodyCreator?.Update( body );
    }

    public static void UpdateMaterial( Body body )
    {
      var bodyCreator = CreateBodyCreator( body.Entity );
      bodyCreator?.UpdateMaterials( body );
    }

    static BodyCreator CreateBodyCreator(Entity entity) {
      switch ( entity ) {
        case Pipe _ :
          return new PipeBodyCreator(entity);
        case WeldNeckFlange _ :
          return new WeldNeckFlangeBodyCreator(entity);
        case BlindFlange _ :
          return new BlindFlangeBodyCreator(entity);
        case SlipOnFlange _ :
          return new SlipOnFlanngeBodyCreator(entity);
        case Flange _ :
          return new FlangeBodyCreator(entity);
        case PipingElbow90 _ :
          return new PipingElbow90BodyCreator(entity);
        case PipingElbow45 _ :
          return new PipingElbow45BodyCreator(entity);
        case GateValve _ :
          return new GateValveBodyCreator(entity);
        case ButterflyValve _ :
          return new ButterflyValveBodyCreator(entity);
        case CheckValve _ :
          return new CheckValveBodyCreator(entity);
        case BallValve _ :
          return new BallValveBodyCreator(entity);
        case GlobeValve _:
          return new GlobeValveBodyCreator(entity);
        case PipingTee _ :
          return new PipingTeeBodyCreator(entity);
        case PipingLateralTee _ :
          return new PipingLateralTeeBodyCreator(entity);
        case ConcentricPipingReducerCombination _ :
          return new ConcentricPipingReducerCombinationBodyCreator(entity);
        case EccentricPipingReducerCombination _ :
          return new EccentricPipingReducerCombinationBodyCreator(entity);
        case PipingCap _ :
          return new PipingCapBodyCreator(entity);
        case PipingPlug _ :
          return new PipingPlugBodyCreator(entity);
        case PipingCoupling _ :
          return new PipingCouplingBodyCreator(entity);
        case WeldOlet _ :
          return new WeldOletBodyCreator(entity);
        case SockOlet _ :
          return new SockOletBodyCreator(entity);
        case StubInReinforcingWeld _ :
          return new StubInReinforcingWeldBodyCreator(entity);
        case AngleTStrainer _ :
          return new AngleTStrainerBodyCreator(entity);
        case OpenSpectacleBlank _ :
          return new OpenSpectacleBlankBodyCreator(entity);
        case BlankSpectacleBlank _ :
          return new BlankSpectacleBlankBodyCreator(entity);
        case ThreeWayInstrumentRootvalve _ :
          return new ThreeWayInstrumentRootValveBodyCreator(entity);
        case OrificePlate _ :
          return new OrificePlateBodyCreator(entity);
        case PressureGauge _ :
          return new PressureGaugeBodyCreator(entity);
        case RestrictorPlate _ :
          return new RestrictorPlateBodyCreator(entity);
        case Instrument _ :
          return new InstrumentBodyCreator(entity);
        case InstrumentAngleControlValve _ :
          return new InstrumentAngleControlValveBodyCreator(entity);
        case ControlValve _ :
          return new ControlValveBodyCreator(entity);
        case PressureReliefValve _ :
          return new PressureReliefValveBodyCreator(entity);
        case GraduatedControlValve _ :
          return new GraduatedControlValveBodyCreator(entity);
        case VenturiTube _ :
          return new VenturiTubeBodyCreator(entity);
        case ActuatorControlValve _ :
          return new ActuatorControlValveBodyCreator(entity);
        // ----------Electrical---------- //
        case LocalPanel _ :
          return new LocalPanelBodyCreator(entity);
        case SubStation _ :
          return new SubStationBodyCreator(entity);
        case Cable _ :
          return new CableBodyCreator(entity);
        case CablePath _ :
          return new CablePathBodyCreator(entity);
        // ------------------------------ //
        case Model.Structure.CommonEntities.PlacementEntity _ :
          return new StructureBodyCreator(entity);
        case Column _ :
          return new ColumnBodyCreator(entity);
        case SkirtTypeVessel _ :
          return new SkirtTypeVesselBodyCreator(entity);
        case LegTypeVessel _ :
          return new LegTypeVesselBodyCreator(entity);
        case SphericalTypeTank _ :
          return new SphericalTypeTankBodyCreator(entity);
        case ConeRoofTypeTank _ :
          return new ConeRoofTypeTankBodyCreator(entity);
        case HorizontalVessel _ :
          return new HorizontalVesselBodyCreator(entity);
        case HorizontalHeatExchanger _ :
          return new HorizontalHeatExchangerBodyCreator(entity);
        case KettleTypeHeatExchanger _ :
          return new KettleTypeHeatExchangerBodyCreator(entity);
        case VerticalHeatExchanger _ :
          return new VerticalHeatExchangerBodyCreator(entity);
        case PlateTypeHeatExchanger _ :
          return new PlateTypeHeatExchangerBodyCreator(entity);
        case AirFinCooler _ :
          return new AirFinCoolerBodyCreator(entity);
        case HorizontalPump _ :
          return new HorizontalPumpBodyCreator(entity);
        case VerticalPump _ :
          return new VerticalPumpBodyCreator(entity);
        case GenericEquipment _ :
          return new GenericEquipmentBodyCreator(entity);
        case EndPoint _ :
          return new DirectionalPointBodyCreater(entity);
        case DirectionalPoint _ :
          return new DirectionalPointBodyCreater(entity);
        case Point _ :
          return new PointBodyCreater(entity);
        case Nozzle _ :
          return new NozzleBodyCreator(entity);
        case Chiller _ :
          return new ChillerBodyCreator(entity);
        case Filter _ :
          return new FilterBodyCreator(entity);
        case FixHeatExchanger _ :
          return new FixHeatExchangerBodyCreator(entity);
        case Compressor _ :
          return new CompressorBodyCreator(entity);

        case FlatTypeFrontEnd _:
          return new FlatTypeFrontEndBodyCreator(entity);
        case CapTypeFrontEnd _:
          return new CapTypeFrontEndBodyCreator(entity);
        case FlatTypeRearEnd _:
          return new FlatTypeRearEndBodyCreator(entity);
        case CapTypeRearEnd _:
          return new CapTypeRearEndBodyCreator(entity);
        case StraightTypeShell _:
          return new StraightTypeShellBodyCreator(entity);
        case KettleTypeShell _:
          return new KettleTypeShellBodyCreator(entity);

        case Model.Support support :
          return Support.SupportBodyCreator.Create(support);
        case LeafUnit _:
          return new LeafUnitBodyCreator(entity);

        case SquareRegion _ :
          return new Region.SquareRegionBodyCreator(entity);
        case CircularRegion _ :
          return new Region.CircularRegionBodyCreator(entity);
        case AFCNozzleArray _ :
          return null ;
        case HalfVertex _:
          return new HalfVertexBodyCreator(entity);
        default :
          throw new InvalidOperationException("CreateBodyCreator failed.");
      }
    }
  }

}