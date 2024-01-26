using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.CAD.Core
{
  public class DocumentEventArgs : EventArgs
  {
    public Document Document { get ; }

    public DocumentEventArgs( Document doc )
    {
      Document = doc ;
    }
  }

  public class DocumentCollection
  {
    private static DocumentCollection _instance;
    public static DocumentCollection Instance
    {
      get
      {
        if ( null == _instance ) _instance = new DocumentCollection();
        return _instance;
      }
    }

    public event EventHandler CurrentDocumentChanged ;
    public event EventHandler<DocumentEventArgs> DocumentCreated ;
    public event EventHandler<DocumentEventArgs> DocumentClosed ;

    private void SetCurrentDocument( Document current )
    {
      if ( _current == current ) return ;

      _current = current ;
      CurrentDocumentChanged?.Invoke( this, EventArgs.Empty ) ;
    }
    

    private Document _current = null;

    public Document CreateNew()
    {
      if ( null != _current ) {
        throw new InvalidOperationException( "Open new Document after closing old one." );
      }

      var doc = Document.Create() ;
      DocumentCreated?.Invoke( this, new DocumentEventArgs( doc ) ) ;
      SetCurrentDocument( doc ) ;
      return _current;
    }

    public Document Current => _current ;

    public void Close( Document document )
    {
      if ( _current != document ) {
        throw new InvalidOperationException( "Can close only current Document." );
      }
      if ( !document.Close() ) return;

      SetCurrentDocument( null ) ;

      DocumentClosed?.Invoke( this, new DocumentEventArgs( document ) ) ;
    }
  }
}
