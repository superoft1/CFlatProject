using System ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;

namespace Chiyoda.CAD.Core
{
  public interface IElement : IBoundary, ICopyable, IMemorableObject
  {
    event EventHandler<ItemChangedEventArgs<IElement>> AfterNewlyChildrenChanged ;
    event EventHandler<ItemChangedEventArgs<IElement>> AfterHistoricallyChildrenChanged ;
    event EventHandler VisibilityChanged ;

    Document Document { get ; }

    IElement Parent { get ; }

    IEnumerable<IElement> Children { get ; }

    bool IsVisible { get ; set ; }

    bool HasError { get ; } // エラーがある場合、ツリーなどの表示で把握できるようにする

    string Name { get ; }

    void OnRemovedFromParent() ;
    void OnAddedIntoNewParent( IElement element ) ;
  }
}