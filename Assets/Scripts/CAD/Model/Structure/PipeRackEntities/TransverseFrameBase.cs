using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Util ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal class TransverseFrameBase : MemorableObjectBase
  {
    private bool _eventActive ;

    protected TransverseFrameBase( History h )
    {
      History = h ;
    }

    public event EventHandler WeightChanged ;

    public IDisposable ActivateWeightChangedEvent() 
    {
      _eventActive = true ;
      return new DisposableAction( () => _eventActive = false ) ;
    }

    protected void TryFireWeightChangedEvent()
    {
      if ( !_eventActive ) {
        return ;
      }
      WeightChanged?.Invoke( this, EventArgs.Empty );
    }

    public override History History { get ; }
  }
}