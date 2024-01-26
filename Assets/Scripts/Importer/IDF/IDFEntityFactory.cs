using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Chiyoda.CAD.Model;

namespace IDF
{
  public class IDFEntityFactory
  {
    public static IDFEntityImporter CreateEntityImporter(IDFRecordType.FittingType _type, IDFRecordType.LegType _legType, string[] elements, Vector3d standard, UnitOption option)
    {
      switch (_type)
      {
        case IDFRecordType.FittingType.Pipe: return new IDFPipeImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.FixedLengthPipe: return new IDFPipeImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Union: return new IDFUnionImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Elbow: return new IDFElbowImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Bend: return new IDFElbowImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Tee: return new IDFTeeImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Olet: return new IDFOutletImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Flange: return new IDFFlangeImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Cap: return new IDFCapImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Valve: return new IDFValveImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Coupling: return new IDFCouplingImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Pcom: return new IDFPcomImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Instrument: return new IDFInstrumentImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.Reducer: return new IDFReducerImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.BlindFlange: return new IDFBlindFlangeImporter(_type, _legType, elements, standard, option);
        case IDFRecordType.FittingType.PipeHanger: return new IDFPipeSupportImpoter(_type, _legType, elements, standard, option);
      }

      return null;
    }

  }
}