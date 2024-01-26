using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Chiyoda.CAD.Core
{
  /// <summary>
  /// アプリケーションの共通設定クラス。
  /// </summary>
  [PlayerPrefsSerializable( "ApplicationConfig" )]
  class ApplicationConfig
  {
    private static ApplicationConfig _current;
    private static PlayerPrefsSerializer _serializer;

    public static ApplicationConfig Current
    {
      get
      {
        if ( null == _current ) {
          _serializer = PlayerPrefsSerializableAttribute.Get( typeof( ApplicationConfig ) );
          _current = new ApplicationConfig();
        }
        return _current;
      }
    }


    [PlayerPrefsFieldSerializable]
    public LengthUnitType PositionUnit { get; set; }

    [PlayerPrefsFieldSerializable]
    public LengthUnitType DimensionUnit { get; set; }

    [PlayerPrefsFieldSerializable]
    public LengthUnitType DiameterUnit { get; set; }


    private ApplicationConfig()
    {
      PositionUnit = LengthUnitType.Meter;
      DimensionUnit = LengthUnitType.Meter;
      DiameterUnit = LengthUnitType.Meter;

      Load();
    }

    public void Load()
    {
      _serializer.Load( this );
    }

    public void Save()
    {
      _serializer.Save( this );
    }
  }
}
