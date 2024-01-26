using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda
{
  static class UnitExtension
  {
    private const double RAD2DEG = 180 / Math.PI;

    public static double YardOrMeter( this double d, CAD.Core.LengthUnitType type )
    {
      if ( CAD.Core.LengthUnitType.Meter == type ) return d;

      // YardPond
      return d.Yard();
    }

    public static double Yard( this double d )
    {
      return 0.9144 * d;
    }

    public static double Feet( this double d )
    {
      return 0.3048 * d;
    }

    public static double Inches( this double d )
    {
      return 0.0254 * d;
    }

    public static double Millimeters( this double d )
    {
      return 0.001 * d;
    }

    public static double Centimeters( this double d )
    {
      return 0.01 * d;
    }

    public static double Meters( this double d )
    {
      return d;
    }

    public static double Rad2Deg( this double d )
    {
      return d * RAD2DEG;
    }

    public static double Deg2Rad( this double d )
    {
      return d / RAD2DEG;
    }

    public static double ToInches( this double d )
    {
      return d / 0.0254;
    }

    public static double ToMillimeters( this double meter )
    {
      return meter / 0.001 ;
    }
  }
}
