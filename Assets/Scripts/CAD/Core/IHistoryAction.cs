using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;

namespace Chiyoda.CAD.Core
{
  interface IHistoryAction
  {
    IMemento Memento { get ; }

    void Undo() ;
    void Redo() ;

    bool Merge( IHistoryAction action ) ;
  }

  abstract class HistoryActionBase : IHistoryAction
  {
    public IMemento Memento { get ; }

    protected HistoryActionBase( IMemento memento )
    {
      if ( memento.History.IsUndoing || memento.History.IsRedoing ) {
        throw new InvalidOperationException() ;
      }
      
      Memento = memento ;
    }

    public abstract void Undo() ;
    public abstract void Redo() ;

    public abstract bool Merge( IHistoryAction action ) ;
  }
}