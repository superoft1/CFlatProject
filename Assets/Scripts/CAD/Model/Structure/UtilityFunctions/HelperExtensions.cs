using System ;
using Chiyoda.CAD.Core ;

namespace Chiyoda.CAD.Model.Structure
{
  internal static class HelperExtensions
  {
    public static void TryChangeValue( this Memento<double> member, double value, Action onValueChanged )
    {
      if ( Math.Abs( member.Value - value ) < Tolerance.DoubleEpsilon ) {
        return ;
      }

      member.Value = value ;
      onValueChanged() ;
    }
    public static void TryChangeValue( this Memento<float> member, float value, Action onValueChanged )
    {
      if ( Math.Abs( member.Value - value ) < Tolerance.FloatEpsilon ) {
        return ;
      }

      member.Value = value ;
      onValueChanged() ;
    }

    public static void TryChangeValue( this Memento<bool> member, bool value, Action onValueChanged )
    {
      if ( member.Value == value ) {
        return ;
      }

      member.Value = value ;
      onValueChanged() ;
    }

    public static void TryChangeValue( this Memento<IStructuralMaterial> member,
      IStructuralMaterial m, Action onValueChanged )
    {
      if ( member == null || Equals( member.Value, m ) ) {
        return;
      }
      member.Value = m ;
      onValueChanged() ;
    }

    private static bool Equals( IStructuralMaterial m0, IStructuralMaterial m1 )
    {
      if ( m0 == m1 ) {
        return true ;
      }
      if ( m0 == null || m1 == null ) {
        return false ;
      }
      return m0.Name == m1.Name
             && m0.IsSteel == m1.IsSteel
             && m0.ShapeType == m1.ShapeType
             && m0.Rotation == m1.Rotation ;
    }
    
/*
    public static void TryChangeValue<T>( this Memento<T> member, T value, Action onValueChanged )
      where T : class
    {
      if ( member.Value == value ) {
        return ;
      }

      member.Value = value ;
      onValueChanged() ;
    }
*/

    
  }
}