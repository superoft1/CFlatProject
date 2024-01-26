using System ;

namespace Chiyoda.CAD.Model.Structure
{
  internal interface IWeightChangedEventEntity
  {
    event EventHandler WeightChanged ; 
    IDisposable ActivateWeightChangedEvent() ;
  }
}