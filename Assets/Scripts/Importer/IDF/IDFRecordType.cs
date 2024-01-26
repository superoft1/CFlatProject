using System;

namespace IDF
{
  public class IDFRecordType
  {
    public enum FittingType
    {
      Bend,
      Elbow,
      Olet,
      Tee,
      Cross,
      Reducer,
      TeeReducer,
      ReducingFlange,
      TeeBend,
      AngleValve,
      ThreeWayValve,
      FourWayValve,
      Instrument, //! CheckValve
      Pcom, //! OpenSpectacleBlank
      Pipe,
      FixedLengthPipe,
      PipeBlockFixedLength,
      PipeBlockVariableLength,
      Flange,
      LapJointStubEnd,
      BlindFlange,
      Gasket,
      Bolt,
      Weld,
      Cap,
      Coupling,
      Union,
      Valve,
      Trap,
      Vent,
      Filter,
      UserPositionedComment,
      PipeHanger,
      BoreRecord,
      EndOfFileMarker,
      Standard,
      Unknown
    }

    public enum LegType
    {
      InLeg,
      FirstBranchLeg,
      SecondBranchLeg,
      OutLeg,
      NoLeg
    }

    public enum PurposeType
    {
      OverflowTextRecord,
      PipelineReference,
      PipingSpecificationName,
      ItemCode,
      Unknown
    }

    public static FittingType GetFittingType( int recordNumber )
    {
      switch ( recordNumber ) {
        case 30: return FittingType.Bend;
        case 31: return FittingType.Bend;
        case 35: return FittingType.Elbow;
        case 36: return FittingType.Elbow;
        case 40: return FittingType.Olet;
        case 41: return FittingType.Olet;
        case 42: return FittingType.Olet;
        case 45: return FittingType.Tee;
        case 46: return FittingType.Tee;
        case 47: return FittingType.Tee;
        case 50: return FittingType.Cross;
        case 51: return FittingType.Cross;
        case 52: return FittingType.Cross;
        case 53: return FittingType.Cross;
        case 55: return FittingType.Reducer;
        case 60: return FittingType.TeeReducer;
        case 61: return FittingType.TeeReducer;
        case 62: return FittingType.TeeReducer;
        case 65: return FittingType.ReducingFlange;
        case 70: return FittingType.TeeBend;
        case 71: return FittingType.TeeBend;
        case 72: return FittingType.TeeBend;
        case 75: return FittingType.AngleValve;
        case 76: return FittingType.AngleValve;
        case 80: return FittingType.ThreeWayValve;
        case 81: return FittingType.ThreeWayValve;
        case 82: return FittingType.ThreeWayValve;
        case 85: return FittingType.FourWayValve;
        case 86: return FittingType.FourWayValve;
        case 87: return FittingType.FourWayValve;
        case 88: return FittingType.FourWayValve;
        case 90: return FittingType.Instrument;
        case 91: return FittingType.Instrument;
        case 92: return FittingType.Instrument;
        case 93: return FittingType.Instrument;
        case 95: return FittingType.Pcom;
        case 96: return FittingType.Pcom;
        case 100: return FittingType.Pipe;
        case 101: return FittingType.FixedLengthPipe;
        case 102: return FittingType.PipeBlockFixedLength;
        case 103: return FittingType.PipeBlockVariableLength;
        case 105: return FittingType.Flange;
        case 106: return FittingType.LapJointStubEnd;
        case 107: return FittingType.BlindFlange;
        case 110: return FittingType.Gasket;
        case 115: return FittingType.Bolt;
        case 120: return FittingType.Weld;
        case 125: return FittingType.Cap;
        case 126: return FittingType.Coupling;
        case 127: return FittingType.Union;
        case 130: return FittingType.Valve;
        case 132: return FittingType.Trap;
        case 134: return FittingType.Vent;
        case 136: return FittingType.Filter;
        case 149: return FittingType.UserPositionedComment;
        case 150: return FittingType.PipeHanger;
        case 0: return FittingType.BoreRecord;
        case 999: return FittingType.EndOfFileMarker;
        case 300: return FittingType.Standard;
      }

      return FittingType.Unknown;
    }

    public static LegType GetLegType( int recordNumber )
    {
      switch ( recordNumber ) {
        case 30: return LegType.InLeg;
        case 35: return LegType.InLeg;
        case 40: return LegType.InLeg;
        case 45: return LegType.InLeg;
        case 50: return LegType.InLeg;
        case 55: return LegType.InLeg;
        case 60: return LegType.InLeg;
        case 65: return LegType.InLeg;
        case 70: return LegType.InLeg;
        case 75: return LegType.InLeg;
        case 80: return LegType.InLeg;
        case 85: return LegType.InLeg;
        case 90: return LegType.InLeg;
        case 95: return LegType.InLeg;
        case 100: return LegType.InLeg;
        case 101: return LegType.InLeg;
        case 102: return LegType.InLeg;
        case 103: return LegType.InLeg;
        case 105: return LegType.InLeg;
        case 106: return LegType.InLeg;
        case 107: return LegType.InLeg;
        case 110: return LegType.InLeg;
        case 115: return LegType.InLeg;
        case 120: return LegType.InLeg;
        case 125: return LegType.InLeg;
        case 126: return LegType.InLeg;
        case 127: return LegType.InLeg;
        case 130: return LegType.InLeg;
        case 132: return LegType.InLeg;
        case 134: return LegType.InLeg;
        case 136: return LegType.InLeg;
        case 149: return LegType.InLeg;
        case 150: return LegType.InLeg;
        case 0: return LegType.InLeg;
        case 999: return LegType.InLeg;
        case 41: return LegType.FirstBranchLeg;
        case 46: return LegType.FirstBranchLeg;
        case 51: return LegType.FirstBranchLeg;
        case 61: return LegType.FirstBranchLeg;
        case 71: return LegType.FirstBranchLeg;
        case 81: return LegType.FirstBranchLeg;
        case 86: return LegType.FirstBranchLeg;
        case 91: return LegType.FirstBranchLeg;
        case 52: return LegType.SecondBranchLeg;
        case 87: return LegType.SecondBranchLeg;
        case 92: return LegType.SecondBranchLeg;
        case 31: return LegType.OutLeg;
        case 36: return LegType.OutLeg;
        case 42: return LegType.OutLeg;
        case 47: return LegType.OutLeg;
        case 53: return LegType.OutLeg;
        case 62: return LegType.OutLeg;
        case 72: return LegType.OutLeg;
        case 76: return LegType.OutLeg;
        case 82: return LegType.OutLeg;
        case 88: return LegType.OutLeg;
        case 93: return LegType.OutLeg;
        case 96: return LegType.OutLeg;
      }

      return LegType.NoLeg;
    }

    public static PurposeType GetPurposeType( int recordNumber )
    {
      switch ( recordNumber ) {
        case -1: return PurposeType.OverflowTextRecord;
        case -6: return PurposeType.PipelineReference;
        case -11: return PurposeType.PipingSpecificationName;
        case -20: return PurposeType.ItemCode;
      }

      return PurposeType.Unknown;
    }

    public static FittingType GetFittingType( string recordNumber )
    {
      if ( recordNumber.Contains( "-" ) ) return FittingType.Unknown;

      int result;
      if ( int.TryParse( recordNumber, out result ) ) return GetFittingType( result );

      return FittingType.Unknown;
    }

    public static LegType GetLegType( string recordNumber )
    {
      if ( recordNumber.Contains( "-" ) ) return LegType.NoLeg;

      int result;
      if ( int.TryParse( recordNumber, out result ) ) return GetLegType( result );

      return LegType.NoLeg;
    }

    public static PurposeType GetPurposeType( string recordNumber )
    {
      int result;
      if ( int.TryParse( recordNumber, out result ) ) return GetPurposeType( result );

      return PurposeType.Unknown;
    }

    public static bool IsOverflowTextRecord( string line )
    {
      int status = 0;
      for ( int i = 0, n = Math.Min(6, line.Length) ; i < n ; ++i ) {
        switch ( line[i] ) {
          case ' ':
          case '\t':
            continue;

          case '-':
            if ( 0 != status ) return false;  // 途中に-があった場合はフォーマット不正 (OverflowTextRecordではない)
            ++status;
            break;

          case '1':
            if ( 1 != status ) return false;  // -の直後でなければ OverflowTextRecord ではない
            ++status;
            break;

          default: return false;  // -1以外は OverflowTextRecord ではない
        }
      }

      return true; // -1だったので OverflowTextRecord
    }
  }
}