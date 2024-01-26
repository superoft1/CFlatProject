using System ;
using Chiyoda.CAD.Model ;

namespace Chiyoda.CAD.Core
{
  public abstract class MemorableObjectBase : IMemorableObject
  {
    public event EventHandler AfterNewlyValueChanged ;
    public event EventHandler AfterHistoricallyValueChanged ;
    public event EventHandler AfterValueChanged
    {
      add
      {
        AfterNewlyValueChanged += value ;
        AfterHistoricallyValueChanged += value ;
      }
      remove
      {
        AfterNewlyValueChanged -= value ;
        AfterHistoricallyValueChanged -= value ;
      }
    }
    public abstract History History { get ; }

    protected Memento<T> CreateMemento<T>( T value )
    {
      return new Memento<T>( this, value ) ;
    }
    protected Memento<T> CreateMemento<T>()
    {
      return new Memento<T>( this, default ) ;
    }
    protected Memento<T> CreateMementoAndSetupValueEvents<T>( T value )
    {
      var memento = new Memento<T>( this, value ) ;
      memento.AfterNewlyValueChanged += OnAfterNewlyValueChanged ;
      memento.AfterHistoricallyValueChanged += OnAfterHistoricallyValueChanged ;
      return memento ;
    }
    protected Memento<T> CreateMementoAndSetupValueEvents<T>()
    {
      var memento = new Memento<T>( this, default ) ;
      memento.AfterNewlyValueChanged += OnAfterNewlyValueChanged ;
      memento.AfterHistoricallyValueChanged += OnAfterHistoricallyValueChanged ;
      return memento ;
    }
    protected MementoList<T> CreateMementoListAndSetupValueEvents<T>()
    {
      var memento = new MementoList<T>( this ) ;
      memento.AfterNewlyItemChanged += OnAfterNewlyValueChanged ;
      memento.AfterHistoricallyItemChanged += OnAfterHistoricallyValueChanged ;
      return memento ;
    }

    protected void SetupEvents( IMemorableObject mo )
    {
      mo.AfterNewlyValueChanged += OnAfterNewlyValueChanged ;
      mo.AfterHistoricallyValueChanged += OnAfterHistoricallyValueChanged ;
    }
    
    private void OnAfterNewlyValueChanged<T>( object sender, ValueChangedEventArgs<T> e )
    {
      OnAfterNewlyValueChanged() ;
    }
    private void OnAfterHistoricallyValueChanged<T>( object sender, ValueChangedEventArgs<T> e )
    {
      OnAfterHistoricallyValueChanged() ;
    }
    private void OnAfterNewlyValueChanged( object sender, EventArgs e )
    {
      OnAfterNewlyValueChanged() ;
    }
    private void OnAfterHistoricallyValueChanged( object sender, EventArgs e )
    {
      OnAfterHistoricallyValueChanged() ;
    }
    protected void OnAfterNewlyValueChanged()
    {
      AfterNewlyValueChanged?.Invoke( this, EventArgs.Empty ) ;
    }
    protected  void OnAfterHistoricallyValueChanged()
    {
      AfterHistoricallyValueChanged?.Invoke( this, EventArgs.Empty ) ;
    }
  }
}