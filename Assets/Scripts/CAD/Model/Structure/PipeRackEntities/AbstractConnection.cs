using System ;
using System.Collections.Generic ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model.Structure.CommonEntities ;
using Chiyoda.CAD.Util ;

namespace Chiyoda.CAD.Model.Structure.Entities
{
  internal abstract  class AbstractConnection : EmbodyStructure, IWeightChangedEventEntity
  {
    protected abstract IEnumerable<IStructurePart> CreateStructureElements() ;
    
    private bool _isEventActive ;

    protected  AbstractConnection( Document document ) : base( document )
    {
      _isEventActive = false ;
    }

    public event EventHandler WeightChanged ;
    
    public IDisposable ActivateWeightChangedEvent()
    {
      _isEventActive = true ;
      return new DisposableAction( () => _isEventActive = false ) ;
    }
    
    protected void TryFireValueChangedEvent()
    {
      if ( !_isEventActive ) {
        return ;
      }
      WeightChanged?.Invoke( this, EventArgs.Empty );
    }

    public override IEnumerable<IStructurePart> StructureElements => CreateStructureElements() ;
  }
}