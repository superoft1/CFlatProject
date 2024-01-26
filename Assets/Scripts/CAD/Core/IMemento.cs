using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;

namespace Chiyoda.CAD.Core
{
  public interface IMemento
  {
    History History { get ; }
  }

  public class ValueChangedEventArgs<T> : EventArgs
  {
    public T OldValue { get ; }
    public T NewValue { get ; }

    public ValueChangedEventArgs( T oldValue, T newValue )
    {
      OldValue = oldValue ;
      NewValue = newValue ;
    }

    public ItemChangedEventArgs<U> ToItemChangedEventArgs<U>() where U : class
    {
      return new ItemChangedEventArgs<U>( OldValue as U, NewValue as U ) ;
    }
  }

  public class ItemChangedEventArgs<T> : EventArgs
  {
    public T[] RemovedItems { get ; }
    public T[] AddedItems { get ; }

    public ItemChangedEventArgs( T[] removedItems, T[] addedItems )
    {
      RemovedItems = removedItems.Clone() as T[] ;
      AddedItems = addedItems.Clone() as T[] ;
    }

    public ItemChangedEventArgs( T oldValue, T newValue )
    {
      RemovedItems = ( null != oldValue ) ? new T[ 1 ] { oldValue } : Array.Empty<T>() ;
      AddedItems = ( null != newValue ) ? new T[ 1 ] { newValue } : Array.Empty<T>() ;
    }

    public ItemChangedEventArgs<U> As<U>() where U : class
    {
      return new ItemChangedEventArgs<U>(
        Array.ConvertAll( RemovedItems, t => t as U ?? throw new InvalidCastException( "RemovedItems cannot be converted into U." ) ),
        Array.ConvertAll( AddedItems, t => t as U ?? throw new InvalidCastException( "AddedItems cannot be converted into U." ) ) ) ;
    }

    public ItemChangedEventArgs<U> Convert<U>( Func<T, U> converter ) where U : class
    {
      return new ItemChangedEventArgs<U>(
        Array.ConvertAll( RemovedItems, t => converter( t ) ?? throw new InvalidCastException( "RemovedItems cannot be converted into U." ) ),
        Array.ConvertAll( AddedItems, t => converter( t ) ?? throw new InvalidCastException( "AddedItems cannot be converted into U." ) ) ) ;
    }
  }

  public abstract class AbstractMemento : IMemento
  {
    public History History { get ; private set ; }

    protected AbstractMemento( IMemorableObject owner )
    {
      if ( null == owner ) throw new ArgumentNullException() ;

      History = owner.History ;
    }
  }
}