using System ;
using System.Collections.Generic ;

namespace Chiyoda.CAD.IO
{
  public class AllItemReader
  {
    private bool _ended = false ;
    private bool _closed = false ;
    private int _doneCount = 0 ;
    private readonly List<(bool, object)> _readers = new List<(bool, object)>() ;
    private readonly Action<object[]> _onEnd ;
  
    public AllItemReader( Action<object[]> onEnd )
    {
      _onEnd = onEnd ;
    }

    public Action<object> ReaderAt( int index )
    {
      while ( _readers.Count <= index ) _readers.Add( ( false, null ) ) ;
      return obj =>
      {
        if ( _readers[ index ].Item1 ) return ;
        _readers[ index ] = ( true, obj ) ;
        ++_doneCount ;
        CheckClose() ;
      } ;
    }

    public void End()
    {
      _ended = true ;
      CheckClose() ;
    }

    private void CheckClose()
    {
      if ( false == _ended ) return ;
      if ( true == _closed ) return ;
      if ( _doneCount != _readers.Count ) return ;

      _onEnd( Array.ConvertAll( _readers.ToArray(), pair => pair.Item2 ) ) ;
      _closed = true ;
    }
  }
}