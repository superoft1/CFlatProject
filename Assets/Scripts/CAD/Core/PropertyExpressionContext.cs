using System ;
using System.Collections.Generic ;

namespace Chiyoda.CAD.Core
{
  public class PropertyExpressionContext
  {
    private readonly Dictionary<string, double> _variableValues = new Dictionary<string, double>() ;
    
    public double? GetVariable( string variableName )
    {
      if ( _variableValues.TryGetValue( variableName, out var value ) ) return value ;

      return null ;
    }

    public IDisposable SetVariable( string variableName, double value )
    {
      return new TemporarySetVariable( this, variableName, value ) ;
    }

    private class TemporarySetVariable : IDisposable
    {
      private readonly PropertyExpressionContext _context ;
      private readonly string _variableName ;
      private readonly bool _hasOldValue ;
      private readonly double _oldValue ;
      
      public TemporarySetVariable( PropertyExpressionContext context, string variableName, double value )
      {
        _context = context ;
        _variableName = variableName ;
        _hasOldValue = _context._variableValues.TryGetValue( variableName, out _oldValue ) ;
        if ( _hasOldValue ) {
          _context._variableValues[ variableName ] = value ;
        }
        else {
          _context._variableValues.Add( variableName, value ) ;
        }
      }

      ~TemporarySetVariable()
      {
        throw new InvalidProgramException() ;
      }

      public void Dispose()
      {
        GC.SuppressFinalize( this ) ;

        if ( _hasOldValue ) {
          _context._variableValues[ _variableName ] = _oldValue ;
        }
        else {
          _context._variableValues.Remove( _variableName ) ;
        }
      }
    }
  }
}