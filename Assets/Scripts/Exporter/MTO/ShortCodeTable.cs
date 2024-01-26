namespace MTO
{
  class ShortCodeTable
  {
    public static string ToString( IPipingComponent component )
    {
      switch ( component ) {
        case GateValveComponent _: return "GTV"; // GATE VALVE
////        case GlobeValveComponent _: return "GLV"; // GLOBE VALVE
        case BallValveComponent _: return "BAV"; // BALL VALVE
        case ButterflyValveComponent _: return "BUV"; // BUTTERFLY VALVE
////        case ThreeWayInstrumentRootvalveComponent _: return "";
        case CheckValveComponent _: return "CHV"; // CHECK VALVE
////        case SafetyValveComponent _: return "";
        // NSV: NON SLAM CHECK VALVE
        // SCV: STOP CHECK VALVE

        case ControlValveComponent _: return "COV";// CONTROL VALVE
////        case GraduatedControlValveComponent _: return "";
////        case InstrumentAngleControlValveComponent _: return "";

        case PipeComponent _: return "PIP"; // PIPE
        case PipingElbow45Component _: return "45E"; // 45 DEG ELBOW
        case PipingElbow90Component elbow90:
          if ( elbow90.ElbowType == PipingElbow90Component.Elbow90Type.Long ) return "90E"; // 90 DEG ELBOW
          else                                                                return "90S"; // 90 DEG SR ELBOW
////        case PipeBendComponent _: return "";
////        case PipingCapComponent _: return "";
////        case PipingCouplingComponent _: return "";
////        case PipingPlugComponent _: return "";

        case PipingTeeComponent tee:
          if ( tee.Diameters[0] > tee.Diameters[1] ) return "RET"; // REDUCING TEE
          else                                       return "TEE"; // TEE
////        case PipingLateralTeeComponent _: return "";
////        case SockOletComponent _: return "SLT"; // S.W. OUTLET
////        case WeldOletComponent _: return "WLT"; // B.W. OUTLET
////        case StubInReinforcingWeldComponent _: return "";
        // NLT: NIPPLE OUTLET

        case ConcentricPipingReducerComponent _: return "CRE"; // CON. REDUCER
        case EccentricPipingReducerComponent _: return "ERE"; // ECC. REDUCER

////        case FlangeComponent _: return "";
////        case SlipOnFlangeComponent _: return "";
        case WeldNeckFlangeComponent _: return "FLG"; // FLANGE // TODO: ShortCodeが適切かどうか確認
        case BlindFlangeComponent _: return "BLF"; // BLIND FLANGE
        // ORF: ORIFICE FLANGE
        // RDF: REDUCING FLANGE

////        case YStrainerComponent _: return "";
////        case AngleTStrainerComponent _: return "";
////        case InLineTStrainerComponent _: return "";

        case OpenSpectacleBlankComponent _: return "SPB"; // SPECTACLE BLIND // TODO: ShortCodeが適切かどうか確認
        case BlankSpectacleBlankComponent _: return "SPB"; // SPECTACLE BLIND // TODO: ShortCodeが適切かどうか確認
////        case OrificePlateComponent _: return "";
////        case RestrictorPlateComponent _: return "";

        // SPC: SPACER
        // BLD: BLIND
        // NPL: NIPPLE
        // TOE: NIPPLE
        // UNI: UNION
      }

      return "";
    }
  }
}
