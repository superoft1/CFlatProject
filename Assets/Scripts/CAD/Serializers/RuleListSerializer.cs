using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.IO ;

namespace Chiyoda.CAD.Serializers
{
  internal class RuleListSerializer : AbstractSerializer<RuleList>
  {
    public override bool CanRefer => false ;

    protected override bool Write( SerializationContext con, in RuleList obj )
    {
      foreach ( var rule in obj.Rules ) {
        if ( false == con.WriteValue( "rule", rule ) ) return false ;
      }

      return true ;
    }

    protected override bool Read( DeserializationContext con, Action<RuleList> onRead )
    {
      var parent = (IPropertiedElement) con.Parent ;
      var readCounter = new AllItemReader( array =>
      {
        var ruleList = new RuleList( parent ) ;
        foreach ( IRule rule in array ) ruleList.AddRule( rule ) ;
        onRead( ruleList ) ;

        con.RegisterPostProcess( con.Document, doc => ruleList.BindChangeEvents( false ) ) ;
      } ) ;
      int index = 0 ;
      while ( con.GetNextElementName( out var name ) && name == "rule" ) {
        if ( ! con.ReadValue( "rule", typeof( IRule ), readCounter.ReaderAt( index ) ) ) return false ;
        ++index ;
      }

      readCounter.End() ;

      return true ;
    }
  }
}
