using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chiyoda.CAD.Core
{
  /// <summary>
  /// 長さ単位を保持するクラスです。
  /// </summary>
  public class LengthUnitCollection : MemorableObjectBase, ICopyable
  {
    private readonly Document _doc;

    private readonly Memento<LengthUnitType> _position;
    private readonly Memento<LengthUnitType> _dimension;
    private readonly Memento<LengthUnitType> _diameter;

    public override History History => _doc.History ;

    /// <summary>
    /// 敷地サイズ・座標に使用する長さ単位です。
    /// </summary>
    [UI.Property( UI.PropertyCategory.Dimension, "Area Unit" )]
    public LengthUnitType PositionUnit { get { return _position.Value; } set { _position.Value = value; } }

    /// <summary>
    /// 機器の大きさ・長さ・クリアランスに使用する長さ単位です。
    /// </summary>
    [UI.Property( UI.PropertyCategory.Dimension, "Instrument Size Unit" )]
    public LengthUnitType DimensionUnit { get { return _dimension.Value; } set { _dimension.Value = value; } }

    /// <summary>
    /// 配管径に使用する長さ単位です。
    /// </summary>
    [UI.Property( UI.PropertyCategory.Dimension, "Diameter Unit" )]
    public LengthUnitType DiameterUnit { get { return _diameter.Value; } set { _diameter.Value = value; } }

    internal LengthUnitCollection( Document document )
    {
      _doc = document ;

      _position = CreateMementoAndSetupValueEvents( ApplicationConfig.Current.PositionUnit ) ;
      _dimension = CreateMementoAndSetupValueEvents( ApplicationConfig.Current.DimensionUnit ) ;
      _diameter = CreateMementoAndSetupValueEvents( ApplicationConfig.Current.DiameterUnit ) ;
    }

    public void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      var lu = another as LengthUnitCollection;
      _position.CopyFrom( lu._position.Value );
      _dimension.CopyFrom( lu._dimension.Value );
      _diameter.CopyFrom( lu._diameter.Value );
    }
  }
}
