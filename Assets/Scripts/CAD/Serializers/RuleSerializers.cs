using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.IO ;

namespace Chiyoda.CAD.Serializers
{
  internal class ObjectPropertyRuleSerializer : AbstractSerializer<ObjectPropertyRule>
  {
    public override bool CanRefer => true ;

    protected override bool Write( SerializationContext con, in ObjectPropertyRule obj )
    {
      if ( false == string.IsNullOrEmpty( obj.ObjectName ) ) {
        if ( false == con.WriteValue( "object", obj.ObjectName ) ) return false ;
      }
      if ( false == con.WriteValue( "property", obj.PropertyName ) ) return false ;
      if ( false == con.WriteValue( "expression", obj.Expression.ToString() ) ) return false ;

      return true ;
    }

    protected override bool Read( DeserializationContext con, Action<ObjectPropertyRule> onRead )
    {
      string objName = null, propName = null, expression = null ;
      while ( con.GetNextElementName( out var name ) ) {
        switch ( name ) {
          case "object" :
            if ( null != objName ) return false ;
            if ( false == con.ReadString( "object", out objName ) ) return false ;
            break ;

          case "property" :
            if ( null != propName ) return false ;
            if ( false == con.ReadString( "property", out propName ) ) return false ;
            break ;

          case "expression" :
            if ( null != expression ) return false ;
            if ( false == con.ReadString( "expression", out expression ) ) return false ;
            break ;

          default : return false ;
        }
      }

      if ( null == propName ) return false ;

      onRead( new ObjectPropertyRule( (IPropertiedElement) con.Parent, objName, propName, expression.ParseExpression() ) ) ;
      return true ;
    }
  }

  internal class UserDefinedNamedPropertyRangeRuleSerializer : AbstractSerializer<UserDefinedNamedPropertyRangeRule>
  {
    public override bool CanRefer => true ;

    protected override bool Write( SerializationContext con, in UserDefinedNamedPropertyRangeRule obj )
    {
      if ( false == con.WriteValue( "name", obj.PropertyName ) ) return false ;
      if ( null != obj.MinExpression ) {
        if ( false == con.WriteValue( "min", obj.MinExpression.ToString() ) ) return false ;
      }
      if ( null != obj.MinExpression ) {
        if ( false == con.WriteValue( "max", obj.MaxExpression.ToString() ) ) return false ;
      }

      return true ;
    }

    protected override bool Read( DeserializationContext con, Action<UserDefinedNamedPropertyRangeRule> onRead )
    {
      string propName = null, minExpression = null, maxExpression = null ;
      while ( con.GetNextElementName( out var name ) ) {
        switch ( name ) {
          case "name" :
            if ( null != propName ) return false ;
            if ( false == con.ReadString( "name", out propName ) ) return false ;
            break ;

          case "min" :
            if ( null != minExpression ) return false ;
            if ( false == con.ReadString( "min", out minExpression ) ) return false ;
            break ;

          case "max" :
            if ( null != maxExpression ) return false ;
            if ( false == con.ReadString( "max", out maxExpression ) ) return false ;
            break ;

          default : return false ;
        }
      }

      if ( null == propName ) return false ;

      onRead( new UserDefinedNamedPropertyRangeRule( (IPropertiedElement) con.Parent, propName, minExpression.ParseExpression(), maxExpression.ParseExpression() ) ) ;
      return true ;
    }
  }

  internal class SynchronizedRangeRuleSerializer : AbstractSerializer<UserDefinedNamedPropertyRangeRule.SynchronizedRangeRule>
  {
    public override bool CanRefer => true ;

    protected override bool Write( SerializationContext con, in UserDefinedNamedPropertyRangeRule.SynchronizedRangeRule obj )
    {
      return con.WriteReferenceOnly( "target", obj.TargetRangeRule ) ;
    }

    protected override bool Read( DeserializationContext con, Action<UserDefinedNamedPropertyRangeRule.SynchronizedRangeRule> onRead )
    {
      var parent = (IPropertiedElement) con.Parent ;

      return con.ReadValue(
        "target",
        typeof( UserDefinedNamedPropertyRangeRule ),
        ( UserDefinedNamedPropertyRangeRule rule ) => onRead( (UserDefinedNamedPropertyRangeRule.SynchronizedRangeRule)rule.CreateSynchronizedRangeRule( parent ) ) ) ;
    }
  }
}