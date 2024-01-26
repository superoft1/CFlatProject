using System ;
using System.Collections.Generic ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Topology
{
  partial class CompositeBlockPattern
  {
    public static class UserDefinedRules
    {
      private static readonly Dictionary<string, IUserDefinedRule> _changeJointTypeRules = new Dictionary<string, IUserDefinedRule>() ;
      
      
      public static IUserDefinedRule ApplyToChildBlockPatternRule { get ; } = new ApplyToBlockPatterns() ;

      public static IUserDefinedRule GetChangeJointTypeRule( string name )
      {
        if ( ! _changeJointTypeRules.TryGetValue( name, out var rule ) ) {
          rule = new ChangeJointType( name ) ;
          _changeJointTypeRules.Add( name, rule ) ;
        }

        return rule ;
      }
      
      private class ApplyToBlockPatterns : IUserDefinedRule
      {
        public void Run( IPropertiedElement owner, IUserDefinedNamedProperty property )
        {
          OnPropertyChanged( owner, property ) ;
        }
      }
      
      public class ChangeJointType : IUserDefinedRule
      {
        private readonly string _jointName ;
        
        public ChangeJointType( string jointName )
        {
          _jointName = jointName ;
        }
        
        public void Run( IPropertiedElement owner, IUserDefinedNamedProperty property )
        {
          var cbp = ( owner as CompositeBlockPattern ) ?? ( ( owner as BlockPattern )?.Parent as CompositeBlockPattern ) ;
          cbp.JoinByName( _jointName, (JoinType)Enum.ToObject( typeof( JoinType ), (int)property.Value ) );
        }
      }
    }
  }
}