namespace Chiyoda.CAD.Model.Routing
{
  public interface IRoutingEdge
  {
    IRoutingConstraint From { get ; }
    IRoutingConstraint To { get ; }
  }
}