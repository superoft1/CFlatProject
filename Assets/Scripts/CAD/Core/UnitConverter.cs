using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Chiyoda.CAD.Core
{
  using Unit = KeyValuePair<double, string[]>;

  public enum LengthUnitType
  {
    Meter,
    YardPond,
  }

  static class UnitConverter
  {
    public static double? StringToDegree( string str )
    {
      return GetValue( str, 1, new Unit( 1, new[] { "°" } ), new Unit( 1 / 60.0, new[] { "'", "′" } ), new Unit( 1 / 360.0, new[] { "\"", "″" } ) );
    }

    public static string DegreeToString( double degree )
    {
      return string.Format( "{0:0.0}°", ToDegree( degree ) );
    }
    private static double ToDegree( double value )
    {
      value *= 1 / (Math.PI * 2);
      value -= Math.Floor( value );

      var degree = (int)Math.Floor( value * 3600 + 0.5 );
      if ( 3600 == degree ) degree = 0;
      return degree * 0.1;
    }


    public static double? StringToLength( string str, LengthUnitType unitType )
    {
      if ( LengthUnitType.YardPond == unitType ) {
        return GetValue(
                   str,
                   0.0254,
                   new Unit( 0.9144, new[] { "yd", "yard", "yards" } ),
                   new Unit( 0.3048, new[] { "ft", "foot", "feet", "'", "′" } ),
                   new Unit( 0.0254, new[] { "in", "inch", "inches", "\"", "″" } )
                 )
            ?? GetValue(
                   str,
                   1,
                   new Unit( 1, new[] { "m", "meter", "meters", "metre", "metres" } ),
                   new Unit( 0.01, new[] { "cm", "centimeter", "centimeters", "centimetre", "centimetres" } ),
                   new Unit( 0.001, new[] { "mm", "millimeter", "millimeters", "millimetre", "millimetres" } )
                 );
      }
      else {
        return GetValue(
                   str,
                   1,
                   new Unit( 1, new[] { "m", "meter", "meters", "metre", "metres" } ),
                   new Unit( 0.01, new[] { "cm", "centimeter", "centimeters", "centimetre", "centimetres" } ),
                   new Unit( 0.001, new[] { "mm", "millimeter", "millimeters", "millimetre", "millimetres" } )
                 )
            ?? GetValue(
                   str,
                   0.0254,
                   new Unit( 0.9144, new[] { "yd", "yard", "yards" } ),
                   new Unit( 0.3048, new[] { "ft", "foot", "feet", "'", "′" } ),
                   new Unit( 0.0254, new[] { "in", "inch", "inches", "\"", "″" } )
                 );
      }
    }

    public static string LengthToString( double length, LengthUnitType unitType )
    {
      if ( LengthUnitType.YardPond == unitType ) {
        int centiInches = (int)Math.Floor( length / 0.0254 * 100 + 0.5 );
        int yards = centiInches / 3600;
        int feet = (centiInches % 3600) / 1200;
        double inches = (centiInches % 1200) / 100;
        if ( 0 < yards ) {
          return string.Format( "{0:0}yd {1:0}ft {2:0.00}in", yards, feet, inches );
        }
        if ( 0 < feet ) {
          return string.Format( "{0:0}ft {1:0.00}in", feet, inches );
        }
        return string.Format( "{0:0.00}in", inches );
      }
      else {
        if ( 1 <= length ) {
          return string.Format( "{0:0.000}m", length );
        }
        else if ( 0.01 <= length ) {
          return string.Format( "{0:0.0}cm", length / 100 );
        }
        else {
          return string.Format( "{0:0}mm", length / 1000 );
        }
      }
    }



    private static readonly Dictionary<string[], Regex> _unitRegexDic = new Dictionary<string[], Regex>();

    private static Regex GetRegex( string[] units )
    {
      Regex regex;
      if ( !_unitRegexDic.TryGetValue( units, out regex ) ) {
        regex = new Regex( @"^\s*(-?[0-9]+(?:\.[0-9]*)?)\s*(" + string.Join( "|", Array.ConvertAll( units, Regex.Escape ) ) + @")?\s*(.*)$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant );
        _unitRegexDic.Add( units, regex );
      }
      return regex;
    }

    private static double? GetValue( string str, double defaultUnit, Unit firstUnit, params Unit[] units )
    {
      double value = 0;

      var match0 = GetRegex( firstUnit.Value ).Match( str );
      if ( !match0.Success ) {
        // エラー
        return null;
      }

      value += double.Parse( match0.Groups[1].Value );
      if ( 0 == match0.Groups[2].Length ) {
        // 単位がなければデフォルト単位
        return value * defaultUnit;
      }
      if ( 0 == match0.Groups[3].Length ) {
        // 単位1つのみ
        return value * firstUnit.Key;
      }

      // 複合単位
      str = match0.Groups[3].Value;
      foreach ( var unit in units ) {
        var match = GetRegex( unit.Value ).Match( str );
        if ( !match.Success ) {
          // 失敗したらそこは無視
          return value;
        }
        value += double.Parse( match.Groups[1].Value ) * unit.Key;
        if ( 0 == match.Groups[2].Length ) {
          // 単位がなくなったら終了
          return value;
        }
        if ( 0 == match0.Groups[3].Length ) {
          // 後続がなくなったら終了
          return value;
        }

        str = match0.Groups[3].Value;
      }

      return value;
    }
  }
}
