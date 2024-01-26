using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD;
using System;
using System.Linq ;
using Chiyoda ;
using Chiyoda.CAD.Topology;
using Component = Chiyoda.CAD.Model.Component;

namespace IDF
{

  public class IDFReducerImporter : IDFEntityImporter
  {

    private EntityType.Type ReducerEntityType { get; set; }
    private string BeforeRecordColumn7 { get; set; }
    public IDFReducerImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements, Vector3d _standard, UnitOption option)
      : base(_type, _legType, _elements, _standard, option)
    {
      entityState = EntityState.MustLookZeroRecord;
    }

    public override LeafEdge Import(Chiyoda.CAD.Core.Document doc)
    {
      var elements = elementsDictionary[IDFRecordType.LegType.InLeg];
      ReducerEntityType = elements[11] == "RCBW" ? EntityType.Type.ConcentricPipingReducerCombination : EntityType.Type.EccentricPipingReducerCombination;

      var entity = doc.CreateEntity(ReducerEntityType);
      LeafEdge = doc.CreateEntity<LeafEdge>();
      LeafEdge.PipingPiece = entity as PipingPiece;
      return LeafEdge;
    }

    public override ErrorPosition Build( LeafEdge prev, IDFEntityImporter next )
    {
      var elements = elementsDictionary[IDFRecordType.LegType.InLeg];
      ReducerEntityType = elements[11] == "RCBW" ? EntityType.Type.ConcentricPipingReducerCombination : EntityType.Type.EccentricPipingReducerCombination;

      var largeDiameterElements = new[] { "0", "0", "0", "0", "0", "0", "0", BeforeRecordColumn7 };
      if (ReducerEntityType == EntityType.Type.ConcentricPipingReducerCombination)
      {
        var diameter1 = GetDiameter(largeDiameterElements);
        var diameter2 = GetDiameter(elements);
        var smallTerm = GetStartPoint(elements);
        var largeTerm = GetEndPoint(elements);
        if (IsSamePoint(smallTerm, largeTerm))
        {
          return ErrorPosition.Other;
        }
        var smallDiameter = diameter2;
        var largeDiameter = diameter1;
        if(diameter1.OutsideMeter < diameter2.OutsideMeter)
        {
          smallTerm = GetEndPoint(elements);
          largeTerm = GetStartPoint(elements);
          smallDiameter = diameter1;
          largeDiameter = diameter2;
          ConnectPointOrder = new List<int>{1,0,2};
        }
        var origin = (smallTerm + largeTerm) * 0.5d;


        var direction = largeTerm - smallTerm;

        LeafEdgeCodSysUtils.LocalizeStraightComponent(LeafEdge, origin, direction);

        var reducer = Entity as ConcentricPipingReducerCombination;
        reducer.LargeDiameter = largeDiameter.OutsideMeter;
        reducer.SmallDiameter = smallDiameter.OutsideMeter;
        reducer.Length = (largeTerm - smallTerm).magnitude;
        return ErrorPosition.None;
      }
      else
      {
        // CreateEccentricPipingReducerで設定する
        return ErrorPosition.None;
      }
    }

    protected override IDFEntityImporter UpdateImpl(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType, string[] columns)
    {
      if (entityState == EntityState.Out)
      {
        return this;
      }
      if (fittingType == IDFRecordType.FittingType.BoreRecord && entityState == EntityState.MustLookZeroRecord)
      {
        BeforeRecordColumn7 = columns[7];
        entityState = EntityState.Out;
      }
      return this;
    }

    public Entity CreateEccentricPipingReducer(LeafEdge connectedEdge)
    {
      var elements = elementsDictionary[IDFRecordType.LegType.InLeg];
      var largeDiameterElements = new [] { "0", "0", "0", "0", "0", "0", "0", BeforeRecordColumn7 };
      var diameter1 = GetDiameter(largeDiameterElements);
      var diameter2 = GetDiameter(elements);
      var smallTerm = GetStartPoint(elements);
      var largeTerm = GetEndPoint(elements);
      var smallDiameter = diameter2;
      var largeDiameter = diameter1;
      if(diameter1.OutsideMeter < diameter2.OutsideMeter)
      {
        smallTerm = GetEndPoint(elements);
        largeTerm = GetStartPoint(elements);
        smallDiameter = diameter1;
        largeDiameter = diameter2;
        ConnectPointOrder = new List<int>{1,0,2};
      }
      var origin = (smallTerm + largeTerm) * 0.5d;
      
      var axis = new Vector3d(0, 0, 0);
      if (connectedEdge.PipingPiece is EccentricPipingReducerCombination)
      {
        axis = connectedEdge.GlobalCod.GlobalizeVector(((Component) connectedEdge.PipingPiece).Axis);
      }
      else {
        var globalOrg = connectedEdge.GlobalCod.GlobalizePoint( ( (Component) connectedEdge.PipingPiece ).Origin ) ;
        var sdDiff = connectedEdge.PipingPiece.ConnectPoints.Max( p => Math.Abs( p.Diameter.OutsideMeter - smallDiameter.OutsideMeter ) ) ;
        var ldDiff = connectedEdge.PipingPiece.ConnectPoints.Max( p => Math.Abs( p.Diameter.OutsideMeter - largeDiameter.OutsideMeter ) ) ;
        // 径のサイズが近い方に接続していると判断する
        // Teeが相手の場合はうまく行かない可能性があるので、問題が発生したら対処する
        if ( sdDiff < ldDiff ) {
          axis = globalOrg - smallTerm;
        }
        else {
          axis = globalOrg - largeTerm;
        }
      }

      axis = axis.normalized;
      var line = largeTerm - smallTerm;
      line = line.normalized;
      if ( axis.IsParallelTo( line ) ) {
        // この場合はaxisと直行するベクトルが得られれば何でも良い
        line += new Vector3d( 1, 1, 1 ) ;
      }
      var outer = Vector3d.Cross(axis, line);
      var reference = Vector3d.Cross(outer, axis).normalized;
      axis = axis.normalized;
      //! Axisが逆になっている場合があるので、座標からチェック
      if (Vector3d.Dot(axis, (smallTerm - largeTerm)) > 0)
      {
        axis *= -1;
      }

      LeafEdgeCodSysUtils.LocalizeEccentricPipingReducerComponent(LeafEdge, origin, axis, reference);

      var reducer = Entity as EccentricPipingReducerCombination;
      if (reducer != null)
      {
        reducer.LargeDiameter = largeDiameter.OutsideMeter;
        reducer.SmallDiameter = smallDiameter.OutsideMeter;
        reducer.Length = (largeTerm - smallTerm).magnitude;
      }
      return reducer;
    }
  }

}