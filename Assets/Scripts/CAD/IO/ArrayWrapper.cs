using System.Collections ;
using System.Collections.Generic ;

namespace Chiyoda.CAD.IO
{
  public abstract class ArrayWrapper
  {
    public static ArrayWrapper Create( object obj )
    {
      if ( obj is IList list ) return new ListWrapper( list ) ;
      
      return null ;
    }

    public abstract void Clear() ;
    public abstract void Add( object obj ) ;



    private class ListWrapper : ArrayWrapper
    {
      private readonly IList _list ;
    
      public ListWrapper( IList list )
      {
        _list = list ;
      }

      public override void Clear()
      {
        _list.Clear() ;
      }

      public override void Add( object obj )
      {
        _list.Add( obj ) ;
      }
    }
  }
}