using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;

namespace IDF
{

  public class IDFTeeImporter : IDFEntityImporter
  {
    EntityType.Type TeeType{ get; }


    public IDFTeeImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements, Vector3d _standard, UnitOption option)
      : base(_type, _legType, _elements, _standard, option)
    {
      TeeType = GetTeeType( _elements[ 11 ] ) ;
    }

    public override LeafEdge Import(Chiyoda.CAD.Core.Document doc)
    {
      // このタイミングではLateralTeeかどうかの判断は出来ないので、Build時に変更する
      var entity = doc.CreateEntity(TeeType);
      LeafEdge = doc.CreateEntity<LeafEdge>();
      LeafEdge.PipingPiece = entity as PipingPiece;
      return LeafEdge;
    }

    public override ErrorPosition Build( LeafEdge prev, IDFEntityImporter next )
    {
      if (!elementsDictionary.ContainsKey(IDFRecordType.LegType.FirstBranchLeg))
      {
        return ErrorPosition.Other;
      }
      
      var terms = new List<(Vector3d pos, double diameter)>
      {
        (GetStartPoint(elementsDictionary[IDFRecordType.LegType.InLeg]), GetDiameter(elementsDictionary[IDFRecordType.LegType.InLeg]).OutsideMeter),
        (GetEndPoint(elementsDictionary[IDFRecordType.LegType.OutLeg]), GetDiameter(elementsDictionary[IDFRecordType.LegType.OutLeg]).OutsideMeter),
        (GetEndPoint(elementsDictionary[IDFRecordType.LegType.FirstBranchLeg]), GetDiameter(elementsDictionary[IDFRecordType.LegType.FirstBranchLeg]).OutsideMeter)
      };

      var origin = GetEndPoint(elementsDictionary[IDFRecordType.LegType.InLeg]);

      var firstBranchLegIndex = JudgeFirstBranchLegFromDiameter(terms[0].diameter, terms[1].diameter, terms[2].diameter) ??
                                JudgeFirstBranchLegFromCenter(terms[0].pos, terms[1].pos, terms[2].pos, origin) ;
      if ( firstBranchLegIndex.HasValue ) {
        var temp = terms[ firstBranchLegIndex.Value ] ;
        terms[ firstBranchLegIndex.Value ] = terms[ 2 ] ;
        terms[ 2 ] = temp ;
      }
      else {
        return ErrorPosition.Other;
      }
      
      if (IsSamePoint(terms[0].pos, terms[1].pos) || IsSamePoint(terms[0].pos, terms[2].pos) || IsSamePoint(terms[1].pos, terms[2].pos))
      {
        if ( TeeType != EntityType.Type.StubInReinforcingWeld ) {
          Debug.LogError("Tee is StubInReinforcingWeld.");
          return DifferentDiameterPosition();
        }
        var stub = Document.CreateEntity(EntityType.Type.StubInReinforcingWeld) as StubInReinforcingWeld;
        LeafEdge.PipingPiece = stub ;
        stub.LocalizeeDgeneratedComponent(origin);
        stub.Diameter = terms[2].diameter;
        stub.MainTerm1ConnectPoint.Diameter = DiameterFactory.FromOutsideMeter( terms[0].diameter );
        stub.MainTerm2ConnectPoint.Diameter = DiameterFactory.FromOutsideMeter( terms[1].diameter );
        return ErrorPosition.None; 
      }

      var axis = ( origin - terms[ 0 ].pos ).normalized ;
      var reference = (origin - terms[2].pos).normalized;

   
      
      if ( IsLateralTee( terms, origin ) ) {
        var lateralTee = Document.CreateEntity(EntityType.Type.PipingLateralTee) as PipingLateralTee;
        LeafEdge.PipingPiece = lateralTee ;
        LeafEdgeCodSysUtils.LocalizeLateralTeeComponent(LeafEdge, origin, axis, reference);
        lateralTee.Length1 = (terms[0].pos - origin).magnitude;
        lateralTee.Length2 = (terms[1].pos - origin).magnitude;
        lateralTee.MainDiameter = terms[0].diameter;
        lateralTee.LateralLength = (terms[2].pos - origin).magnitude;
        lateralTee.LateralDiameter = terms[2].diameter;
        lateralTee.LateralAxis = LeafEdge.LocalCod.LocalizeVector((terms[2].pos - origin).normalized);
      }
      else {
        LeafEdgeCodSysUtils.LocalizeTeeComponent(LeafEdge, origin, axis, reference);
        var tee = Entity as PipingTee;
        var mainDia = DiameterFactory.FromOutsideMeter(terms[0].diameter) ;
        var branchDia = DiameterFactory.FromOutsideMeter(terms[2].diameter) ;
        tee.MainDiameter = mainDia.OutsideMeter ;
        tee.BranchDiameter = branchDia.OutsideMeter ;
        tee.MainLength = PipingTee.GetDefaultMainLength(mainDia, branchDia) ;
        tee.BranchLength = PipingTee.GetDefaultBranchLength(mainDia, branchDia) ;
      }
      
      return ErrorPosition.None; 
    }

    /// <summary>
    /// 一つだけ径が異なるものが存在すればそれがFirstBranchLegになる
    /// </summary>
    /// <param name="inLegDiameter"></param>
    /// <param name="outLegDiameter"></param>
    /// <param name="firstBranchDiameter"></param>
    /// <returns></returns>
    private int? JudgeFirstBranchLegFromDiameter(double inLegDiameter, double outLegDiameter, double firstBranchDiameter)
    {
      if ( Math.Abs( inLegDiameter - outLegDiameter ) < Tolerance.DoubleEpsilon && Math.Abs( inLegDiameter - firstBranchDiameter ) < Tolerance.DoubleEpsilon ) {
        return null ;
      }
      if ( Math.Abs( inLegDiameter - outLegDiameter ) < Tolerance.DoubleEpsilon ) {
        return 2 ;
      }
      return Math.Abs( inLegDiameter - firstBranchDiameter ) < Tolerance.DoubleEpsilon ? 1 : 0 ;
    }
    
    /// <summary>
    /// centerはInLegとOutLegの直線上に乗っているはずなので乗っていないものがFirstBranchLegになる
    /// </summary>
    /// <param name="inLeg"></param>
    /// <param name="outLeg"></param>
    /// <param name="firstBranch"></param>
    /// <param name="center"></param>
    /// <returns></returns>
    private int? JudgeFirstBranchLegFromCenter(Vector3d inLeg, Vector3d outLeg, Vector3d firstBranch, Vector3d center)
    {
      double Dist( Vector3d p1, Vector3d p2 ) => Vector3d.Distance(p1, p2) ;

      bool IsOnLine( Vector3d p1, Vector3d p2, Vector3d checkP ) =>
        Math.Abs( Dist( p1, p2 ) - Dist( p1, checkP ) - Dist( p2, checkP ) ) < Tolerance.DoubleEpsilon ;
      
      if ( IsOnLine(inLeg, outLeg, center) ) {
        return 2 ;
      }
      if ( IsOnLine(inLeg, firstBranch, center)) {
        return 1 ;
      }
      if (IsOnLine(outLeg, firstBranch, center) ) {
        return 0 ;
      }
      // データが正しければここにはこないはず
      Debug.LogError("JudgeFirstBranchLegFromCenter Failed.");
      return null ;
    }

    private bool IsLateralTee( List<(Vector3d pos, double diameter)> terms, Vector3d center )
    {
      var a = (terms[ 0 ].pos - terms[ 1 ].pos).normalized ;
      var b = (terms[ 2 ].pos - center).normalized ;
      var c = Vector3d.Dot( a, b ) ;
      return Math.Abs( c ) > Tolerance.FloatEpsilon*10 ;//FloatEpsilonでは厳しすぎるのでゆるくする
    }
    

    private ErrorPosition DifferentDiameterPosition()
    {
      var diaIn = GetDiameter(elementsDictionary[IDFRecordType.LegType.InLeg]).OutsideMeter; 
      var diaOut = GetDiameter(elementsDictionary[IDFRecordType.LegType.OutLeg]).OutsideMeter; 
      var diaFirstLeg = GetDiameter(elementsDictionary[IDFRecordType.LegType.FirstBranchLeg]).OutsideMeter;
      if (Math.Abs(diaIn - diaOut) < Tolerance.DistanceTolerance)
      {
        return ErrorPosition.Other;
      }

      if (Math.Abs(diaIn - diaFirstLeg) < Tolerance.DistanceTolerance)
      {
        return ErrorPosition.Out;
      }

      return ErrorPosition.In;
    }
    

    protected override IDFEntityImporter UpdateImpl(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType, string[] columns)
    {
      if (fittingType != FittingType)
      {
        return this;
      }
      if (entityState == EntityState.Out)
      {
        return this;
      }
      elementsDictionary.Add(legType, columns);
      switch (legType)
      {
        case IDFRecordType.LegType.FirstBranchLeg:
          entityState = EntityState.FirstLeg;
          break;
        case IDFRecordType.LegType.OutLeg:
          entityState = EntityState.Out;
          break;
      }
      return this;
    }

    private static EntityType.Type GetTeeType( string symbolKey )
    {
      if ( symbolKey.StartsWith( "TESO" ) ) return EntityType.Type.StubInReinforcingWeld ;
      if ( symbolKey.StartsWith( "TERF" ) ) return EntityType.Type.StubInReinforcingWeld ;
      return EntityType.Type.PipingTee ;// PipingLateralTeeの可能性もあり
    }
  }

}
