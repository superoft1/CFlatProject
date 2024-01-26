using Routing ;

namespace VtpAutoRouting.BridgeEntities
{
  internal class AutoRoutingCondition : IAutoRoutingCondition
  {
  private readonly string _type ;
  private readonly string _fluidPhase ;

  public AutoRoutingCondition( string name, string type, string fluidPhase, bool fixDirection, bool routingOnPipeRacks,
    double temperature, string insulationType )
  {
    Name = name ;
    _type = type ;
    FluidPhase = fluidPhase ;
    IsDirectionFix = fixDirection ;
    IsRoutingOnPipeRacks = routingOnPipeRacks ;
    Temperature = temperature ;
    InsulationType = insulationType ;
  }

  public string Name { get ; set ; }

  public bool IsDirectionFix { get ; }
  public bool IsRoutingOnPipeRacks { get ; }

  public double Temperature { get ; }

  public string InsulationType { get ; }

  public string FluidPhase { get ; }

  public LineType Type
  {
    get
    {
      switch ( _type ) {
        case "P" : return LineType.Process ;
        case "U" : return LineType.Utility ;
        case "Flare" : return LineType.Flare ;
        default :
          return LineType.Unknown ;
      }
    }
  }

  public bool IsSlope => _type.EndsWith( "Slope" ) ;
  }
}