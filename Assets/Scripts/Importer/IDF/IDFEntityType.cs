using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace IDF
{

  public class IDFEntityType
  {
    public static EntityType.Type GetType(IDFRecordType.FittingType fittingType)
    {
      switch (fittingType)
      {
        case IDFRecordType.FittingType.AngleValve : return EntityType.Type.AngleTStrainer;
        case IDFRecordType.FittingType.Bend : return EntityType.Type.PipeBend;
        case IDFRecordType.FittingType.BlindFlange : return EntityType.Type.BlindFlange;
        case IDFRecordType.FittingType.Cap : return EntityType.Type.PipingCap;
        case IDFRecordType.FittingType.Coupling : return EntityType.Type.PipingCoupling;
        case IDFRecordType.FittingType.Elbow : return EntityType.Type.PipingElbow90;
        case IDFRecordType.FittingType.Flange : return EntityType.Type.WeldNeckFlange;
        case IDFRecordType.FittingType.Olet : return EntityType.Type.WeldOlet;
        case IDFRecordType.FittingType.Pipe : return EntityType.Type.Pipe;
        case IDFRecordType.FittingType.FixedLengthPipe : return EntityType.Type.Pipe;
        case IDFRecordType.FittingType.Reducer : return EntityType.Type.ConcentricPipingReducerCombination;
        case IDFRecordType.FittingType.Tee : return EntityType.Type.PipingTee;
        case IDFRecordType.FittingType.Valve : return EntityType.Type.GateValve;
        case IDFRecordType.FittingType.Pcom : return EntityType.Type.OpenSpectacleBlank;
        case IDFRecordType.FittingType.Instrument: return EntityType.Type.CheckValve;
        case IDFRecordType.FittingType.Union: return EntityType.Type.Union;
        case IDFRecordType.FittingType.PipeHanger: return EntityType.Type.Support;
      }
      return EntityType.Type.NoEntity;
    }

  }

}