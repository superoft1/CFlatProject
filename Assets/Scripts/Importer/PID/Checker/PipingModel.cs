using System;
using System.Linq;
using System.Collections.Generic;

namespace PID
{
  class PipingModel
  {
    public List<PipingSystem> Systems { get; } = new List<PipingSystem>();

    public void BuildCrossPageConnections()
    {
      var symbols = Systems.SelectMany( system => system.Segments )
                           .SelectMany( segment => segment.Components )
                           .OfType<ICrossPageConnectable>();

      var parentSymbols = new List<ICrossPageConnectable>();
      var childSymbols = new List<ICrossPageConnectable>();

      foreach ( var symbol in symbols ) {
        var segment = symbol.Owner;
        if ( symbol == segment.Components.Last() &&
             symbol.ID == segment.Connection.To.id ) {
          parentSymbols.Add( symbol );
        }
        else if ( symbol == segment.Components.First() &&
                  symbol.ID == segment.Connection.From.id ) {
          childSymbols.Add( symbol );
        }
      }

      foreach ( var parentSymbol in parentSymbols ) {
        var childSymbol = childSymbols.FirstOrDefault( childSymbol_ =>
                                                       childSymbol_.ID == parentSymbol.LinkedPersistentID &&
                                                       childSymbol_.LinkedPersistentID == parentSymbol.ID );
        if ( null != childSymbol ) {
          parentSymbol.Children.Add( ( childSymbol, -1 ) );
          childSymbol.Parents.Add( ( parentSymbol, -1 ) );
        }
      }
    }
  }
}
