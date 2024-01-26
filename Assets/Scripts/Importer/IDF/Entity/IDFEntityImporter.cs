using System;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using UnityEngine.UI;
using Component = Chiyoda.CAD.Model.Component;

namespace IDF
{

  public abstract class IDFEntityImporter
  {
    private Vector3d standard;

    public Entity Entity => LeafEdge.PipingPiece ;

    public LeafEdge LeafEdge
    {
      get ;
      protected set ;
    }

    public enum EntityState
    {
      In,
      FirstLeg,
      SecondLeg,
      MustLookZeroRecord,
      Out,
    }
    public EntityState entityState { get;set;}
    

    private UnitOption UnitOption { get; set; }

    /// <summary>
    /// ConnectPointを設定している順序
    /// In, Out, (FirstLeg)の順であれば0,1,2が返る
    /// 径のサイズを小さい方から設定するなどの理由でOut, In, (FirstLeg)の順になれば1,0,2を返す
    /// </summary>
    /// <returns></returns>
    public List<int> ConnectPointOrder { get; set; } // TODO: 活用されていないので不要かもしれない

    protected Dictionary<IDFRecordType.LegType, string[]> elementsDictionary = new Dictionary<IDFRecordType.LegType, string[]>();

    const double CoordScale = 0.00001d;

    public IDFRecordType.FittingType FittingType
    {
      get;
      protected set;
    }
    public IGroup Group { get; internal set; }
    public Document Document { get; internal set; }

    public IDFEntityImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements, Vector3d _standard, UnitOption option)
    {
      FittingType = _type;
      standard = _standard;
      elementsDictionary.Add(_legType, _elements);
      entityState = EntityState.In;
      ConnectPointOrder = new List<int>{0,1,2};
      UnitOption = option;
    }


    public abstract LeafEdge Import(Chiyoda.CAD.Core.Document doc);

    /// <summary>
    /// コンポーネントの作成に失敗したときに径が異なる側の場所を取得する(Teeの場合に有用)
    /// </summary>
    public enum ErrorPosition
    {
      In,
      Out,
      Other,
      None,
    }
    public abstract ErrorPosition Build( LeafEdge prev, IDFEntityImporter next );


    protected Vector3d GetStartPoint(string[] elements)
    {
      return new Vector3d(-double.Parse(elements[1]), double.Parse(elements[2]), double.Parse(elements[3])) * CoordScale + standard;
    }

    public virtual Vector3d GetStartPoint()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// 終端位置が正常か確認
    /// </summary>
    /// <param name="elements"></param>
    /// <returns>true: 問題なし, false: 問題あるので特別対応が必要</returns>
    protected bool IsValidEndPoint(string[] elements)
    {
      var p = new Vector3d( -double.Parse( elements[ 4 ] ), double.Parse( elements[ 5 ] ),
        double.Parse( elements[ 6 ] ) ) ;
      return Vector3d.Distance( p, Vector3d.zero ) > Tolerance.DoubleEpsilon ;
    }

    protected Vector3d GetEndPoint(string[] elements, bool? valueCheckResult = null)
    {
      return new Vector3d(-double.Parse(elements[4]), double.Parse(elements[5]), double.Parse(elements[6])) * CoordScale + standard;
    }

    protected Diameter GetDiameter(string[] elements)
    {
      var nps = double.Parse(elements[7]);
      switch (UnitOption)
      {
        case UnitOption.inch_16x:
          return DiameterFactory.FromNpsInch( nps / 16d ) ;
        case UnitOption.mm:
          return DiameterFactory.FromNpsMm( nps ) ;
      }
      throw new InvalidOperationException("Can not find unit option.");
    }
    
    protected string UnitName()
    {
      switch (UnitOption)
      {
        case UnitOption.inch_16x:
          return "in";
        case UnitOption.mm:
          return "mm";
      }

      return "in";
    }


    protected abstract IDFEntityImporter UpdateImpl(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType, string[] columns);

    public bool Update(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType, string[] columns)
    {
      if (CheckUpdated(fittingType, legType))
      {
        return false;
      }
      UpdateImpl(fittingType, legType, columns);
      return true;
    }

    /// <summary>
    /// 最後までUpdateが終了しているかチェック
    /// 必要になるケース
    /// TeeのOutLegが二回連続で呼ばれたときに、二回目のOutlegは自身のTeeではなく、根本のTeeであるため区別するために使用する
    /// </summary>
    /// <param name="fittingType"></param>
    /// <param name="legType"></param>
    /// <returns></returns>
    private bool CheckUpdated(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType)
    {
      if (fittingType == FittingType && legType == IDFRecordType.LegType.OutLeg && entityState == EntityState.Out)
      {
        return true;
      }

      return false;
    }

    public void AddStockNumber(List<string> stockNumbers)
    {
      var index = int.Parse(elementsDictionary[IDFRecordType.LegType.InLeg][8]);
      if (Entity is Component comp)
      {
        // stocknumberが不正なものがある事への対応
        comp.StockNumber = (stockNumbers.Count > index - 1 && index > 0) ? stockNumbers[index - 1] : "Unknown";
      }
    }

    protected string SymbolKey( string[] elements )
    {
      return elements[ 11 ] ;
    }
    
    protected bool IsSamePoint(Vector3d p1, Vector3d p2)
    {
      // PipingPieceに座標を設定するときに中央のOriginalに対して値を設定しているの0.5倍する
      if (Math.Abs(Vector3d.Distance(p1, p2)*0.5) < Tolerance.DistanceTolerance)
      {
        return true;
      }

      return false;
    }
  }
}