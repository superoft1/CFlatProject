using System.Collections.Generic ;
using Chiyoda ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Topology ;
using UnityEngine ;
using Component = Chiyoda.CAD.Model.Component ;

namespace IDF
{

  public class IDFInstrumentImporter : IDFEntityImporter
  {
    EntityType.Type InstrumentType{ get; }

    public IDFInstrumentImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] _elements, Vector3d _standard, UnitOption option)
      : base(_type, _legType, _elements, _standard, option)
    {
      InstrumentType = GetInstrumentType( _elements[ 11 ] ) ;
    }

    public override LeafEdge Import(Document doc)
    {
      var entity = doc.CreateEntity(InstrumentType);
      LeafEdge = doc.CreateEntity<LeafEdge>();
      LeafEdge.PipingPiece = entity as PipingPiece;
      return LeafEdge;
    }

    public override Vector3d GetStartPoint()
    {
      return GetStartPoint( elementsDictionary[IDFRecordType.LegType.InLeg] ) ;
    }

    public override ErrorPosition Build( LeafEdge prev, IDFEntityImporter next )
    {
      if ( InstrumentType == Chiyoda.CAD.Model.EntityType.Type.PressureGauge ) {
        return BuildPressureGauge(prev, next) ;
      }

      return BuildInstruments() ;
    }

    private Vector3d? GetTerm2Direction( Vector3d term1, LeafEdge leafEdge )
    {
      if ( leafEdge?.PipingPiece == null ) {
        return null ;
      }

      var comp = (Component) leafEdge.PipingPiece ;
      var dir = ( term1 - leafEdge.GlobalCod.Origin ) ;
      var globalAxis = leafEdge.GlobalCod.GlobalizeVector( comp.Axis ).normalized ;
      if ( ! globalAxis.IsParallelTo( dir ) ) {
        return null ;
      }

      if ( Vector3d.Dot( dir, globalAxis ) < 0 ) {
        globalAxis *= -1 ;
      }

      return globalAxis ;
    }

    private ErrorPosition BuildPressureGauge(LeafEdge prev, IDFEntityImporter next)
    {
      var firstElements = elementsDictionary[IDFRecordType.LegType.InLeg];
      var lastElements = firstElements;
      var term1 = GetStartPoint() ;
      var term2 = Vector3d.zero;
      if ( IsValidEndPoint( lastElements ) ) {
        term2 = GetEndPoint(lastElements);
      }
      else {
        const double coef = 3 ;// 根拠はないが、径の３倍にしておく
        var axisDir = GetTerm2Direction(term1, prev) ?? GetTerm2Direction( term1, next?.LeafEdge );

        if ( axisDir.HasValue ) {
          term2 = term1 + axisDir.Value * GetDiameter( firstElements ).OutsideMeter * coef ;
        }
      }

      if (IsSamePoint(term1, term2))
      {
        return ErrorPosition.Other;
      }
      var direction = (term2 - term1).normalized;
      var origin = (term1 + term2) * 0.5d;
      LeafEdgeCodSysUtils.LocalizeStraightComponent(LeafEdge, origin, direction);

      var pressureGauge = Entity as PressureGauge;
      pressureGauge.Diameter = GetDiameter(firstElements).OutsideMeter;
      pressureGauge.Length = (term2 - term1).magnitude;
      return ErrorPosition.None;  
    }

    private ErrorPosition BuildInstruments()
    {
      var firstElements = elementsDictionary[IDFRecordType.LegType.InLeg];
      var term1 = GetStartPoint(firstElements);
      var term2 = GetEndPoint(firstElements);

      if ( elementsDictionary.ContainsKey( IDFRecordType.LegType.OutLeg ) ) {
        var lastElements = elementsDictionary[IDFRecordType.LegType.OutLeg];
        term2 = GetEndPoint(lastElements);
      }

      if (IsSamePoint(term1, term2))
      {
        return ErrorPosition.Other;
      }
      var direction = (term2 - term1).normalized;
      var origin = (term1 + term2) * 0.5d;
      LeafEdgeCodSysUtils.LocalizeStraightComponent(LeafEdge, origin, direction);

      var diameter = GetDiameter( firstElements ).OutsideMeter ;
      switch ( Entity ) {
        case RestrictorPlate restrictorPlate :
          restrictorPlate.Diameter = diameter ;
          restrictorPlate.Length = ( term2 - term1 ).magnitude ;
          break ;
        case ControlValve controlValve :
          controlValve.Diameter = diameter ;
          controlValve.Length = ( term2 - term1 ).magnitude ;
          controlValve.DiaphramDiameter = controlValve.Diameter * 2 ;
          controlValve.DiaphramLength = controlValve.Length * 2 ;
          break ;
        case InstrumentAngleControlValve instrumentAngleControlValve :
          instrumentAngleControlValve.Diameter = diameter ;
          instrumentAngleControlValve.Length = ( term2 - term1 ).magnitude ;
          break ;
        case OrificePlate orificePlate :
          orificePlate.Diameter = diameter ;
          orificePlate.Length = ( term2 - term1 ).magnitude ;
          break ;
        case GraduatedControlValve graduatedControlValve :
          graduatedControlValve.Diameter = diameter ;
          graduatedControlValve.Length = ( term2 - term1 ).magnitude ;
          break ;
        case Instrument instrument :
          instrument.Diameter = diameter ;
          instrument.Length = ( term2 - term1 ).magnitude ;
          break ;
        default:
          Debug.LogError("IDF Instrument Missing.");
          break ;
      }

      return ErrorPosition.None;
    }

    protected override IDFEntityImporter UpdateImpl(IDFRecordType.FittingType fittingType, IDFRecordType.LegType legType, string[] columns)
    {
      if (entityState == EntityState.Out)
      {
        return this;
      }
      if (OnlyHave90RecordType())
      {
        if (entityState == EntityState.In)
        {
          entityState = EntityState.Out;
        }
        return this;
      }

      if (fittingType != FittingType)
      {
        return this;
      }

      if (legType == IDFRecordType.LegType.OutLeg)
      {
        elementsDictionary.Add(legType, columns);
        entityState = EntityState.Out;
        return this;
      }
      return this;
    }

    private static EntityType.Type GetInstrumentType(string symbolKey)
    {
      if ( symbolKey.StartsWith( "IDPL" ) ) return EntityType.Type.PressureGauge ;
      if ( symbolKey.StartsWith( "PR" ) ) return EntityType.Type.RestrictorPlate ;
      if ( symbolKey.StartsWith( "CV" ) ) return EntityType.Type.ControlValve ;
      if ( symbolKey.StartsWith( "SA" ) ) return EntityType.Type.InstrumentAngleControlValve ;
      if ( symbolKey.StartsWith( "OP" ) ) return EntityType.Type.OrificePlate ;
      if ( symbolKey.StartsWith( "IG" ) ) return EntityType.Type.GraduatedControlValve ;
      if ( symbolKey.StartsWith( "II" ) ) return EntityType.Type.Instrument ;
      // 該当なしもInstrumentに含める
      return EntityType.Type.Instrument ;
    }

    private bool OnlyHave90RecordType()
    {
      return InstrumentType == Chiyoda.CAD.Model.EntityType.Type.OrificePlate ;
    }
  }

}