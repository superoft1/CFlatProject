using Chiyoda.CAD.Topology ;

namespace Chiyoda
{
  public static class EnumExtensions
  {
    public static HalfVertex.FlowType Opposite( this HalfVertex.FlowType flow )
    {
      switch ( flow ) {
        case HalfVertex.FlowType.FromThisToAnother : return HalfVertex.FlowType.FromAnotherToThis ;
        case HalfVertex.FlowType.FromAnotherToThis : return HalfVertex.FlowType.FromThisToAnother ;
        case HalfVertex.FlowType.Undefined :return HalfVertex.FlowType.Undefined ;
        default :
          return HalfVertex.FlowType.Undefined ;
      }
    }
  }
}
