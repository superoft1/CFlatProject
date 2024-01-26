using UnityEngine;

using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Routing;
using Chiyoda.CAD.Plotplan;
using Chiyoda.CAD.Model.Structure;
using Chiyoda.CAD.Topology;

namespace Chiyoda.CAD.Body
{

  public class BodyPrefabAccessor : MonoBehaviour
  {
    [SerializeField]
    GameObject gateValve = null;

    [SerializeField]
    GameObject checkValve = null;
    
    [SerializeField]
    GameObject pressureGauge = null;

    [SerializeField]
    GameObject restrictorPlate = null;
    
    [SerializeField]
    GameObject controlValve = null;

    [SerializeField] 
    GameObject pressureReliefValve = null ;
    
    [SerializeField]
    GameObject instrumentAngleControlValve = null;

    [SerializeField]
    GameObject actuatorControlValve = null;
    
    [SerializeField]
    GameObject orificePlate = null;
    
    [SerializeField]
    GameObject instrument = null;
    
    [SerializeField]
    GameObject graduatedControlValve = null;
    
    [SerializeField]
    GameObject pipingTee = null;

    [SerializeField]
    GameObject pipingLateralTee = null;

    [SerializeField]
    GameObject pipingElbow45 = null;

    [SerializeField]
    BodyRatePrefabSetList elbowList;

    [SerializeField]
    BodyRatePrefabSetList concentricPipingReducerList;

    [SerializeField]
    BodyRatePrefabSetList eccentricPipingReducerList;

    [SerializeField]
    GameObject openSpectacleBlank = null;

    [SerializeField]
    GameObject threeWayInstrumentRootValve = null;

    [SerializeField]
    GameObject pipe = null;

    [SerializeField]
    GameObject flange = null;

    [SerializeField]
    GameObject weldNeckFlange = null;

    [SerializeField]
    GameObject arrowAndCube = null;

    [SerializeField]
    GameObject pipeShoeSupport = null;

    [SerializeField]
    GameObject trunnionSupport = null;

    [SerializeField]
    GameObject ttypeSupport = null;

    [SerializeField]
    GameObject leafUnit = null;

    [SerializeField]
    GameObject halfVertex = null;

    public static BodyPrefabAccessor Instance()
    {
      return instance;
    }

    static BodyPrefabAccessor instance = null;

    private void Awake()
    {
      if (instance == null)
      {
        instance = this;
      }
      elbowList.Initialize();
      concentricPipingReducerList.Initialize();
      eccentricPipingReducerList.Initialize();
    }

    public GameObject Create(Entity entity)
    {
      var prefab = GetPrefab(entity);
      if ( null == prefab ) return null;
      var go = Instantiate(prefab) as GameObject;
      var renderes = go.GetComponentsInChildren<MeshRenderer>();
      foreach (var render in renderes) {
        var col = render.GetComponent<MeshCollider>();
        if (col == null) {
          render.gameObject.AddComponent<MeshCollider>();
        }
      }
      return go;
    }

    GameObject GetPrefab(Entity entity)
    {
      switch ( entity ) {
        case PipingElbow90 _ :
          return GetElbowPrefab(entity);
        case GateValve _ :
          return gateValve;
        case ButterflyValve _ :
          return gateValve;
        case CheckValve _ :
          return checkValve;
        case PressureGauge _ :
          return pressureGauge;
        case RestrictorPlate _ :
          return restrictorPlate;
        case Instrument _ :
          return instrument;
        case ControlValve _ :
          return controlValve;
        case PressureReliefValve _:
          return pressureReliefValve ;
        case GraduatedControlValve _ :
          return graduatedControlValve ;
        case InstrumentAngleControlValve _ :
          return instrumentAngleControlValve;
        case ActuatorControlValve _ :
          return actuatorControlValve;  
        case OrificePlate _ :
          return orificePlate;
        case BallValve _ :
          return gateValve;
        case GlobeValve _:
          return gateValve ;
        case ConcentricPipingReducerCombination _ :
//          return GetConcentricPipingReducerPrefab(entity);
          return null ;
        case EccentricPipingReducerCombination _ :
//          return GetEccentricPipingReducerPrefab(entity);
          return null ;
        case PipingTee _ :
          return pipingTee;
        case PipingLateralTee _ :
          return pipingLateralTee;
        case PipingElbow45 _ :
          return pipingElbow45;
        case OpenSpectacleBlank _ :
          return openSpectacleBlank;
        case BlankSpectacleBlank _ :
          return openSpectacleBlank;
        case AngleTStrainer _ :
          return pipingTee;
        case ThreeWayInstrumentRootvalve _ :
          return threeWayInstrumentRootValve;
        case Flange _ :
          return flange;
        case BlindFlange _ :
          return flange;
        case SlipOnFlange _ :
          return flange;
        case WeldNeckFlange _ :
          return weldNeckFlange;
        case Equipment _ :
          return null ;
        case SubEquipment _:
          return null ;
        case Model.Structure.CommonEntities.PlacementEntity _ :
          return null;
        case Point _ :
          return arrowAndCube;
        case Model.Support support :
          return GetSupportPrefab(support);
        case LeafUnit _:
          return leafUnit;
        case HalfVertex _:
          return halfVertex;
        default :
          return pipe;
      }
    }

    GameObject GetElbowPrefab(Entity entity)
    {
      var elbow = (PipingElbow90)entity;
      var rate = (elbow.Term1 - elbow.Origin).magnitude / elbow.Diameter;
      return elbowList.GetPrefab((float)rate);
    }

    GameObject GetConcentricPipingReducerPrefab(Entity entity)
    {
      var reducer = (ConcentricPipingReducerCombination)entity;
      var rate = reducer.LargeDiameter / reducer.SmallDiameter;
      return concentricPipingReducerList.GetPrefab((float)rate);
    }

    GameObject GetEccentricPipingReducerPrefab(Entity entity)
    {
      var reducer = (EccentricPipingReducerCombination)entity;
      var rate = reducer.LargeDiameter / reducer.SmallDiameter;
      return eccentricPipingReducerList.GetPrefab((float)rate);
    }

    private GameObject GetSupportPrefab( Model.Support support )
    {
      if ( !support.Enabled ) return null;

      switch ( support.SupportType ) {
        case SupportType.PipeShoe: return pipeShoeSupport;
        case SupportType.Trunnion: return trunnionSupport;
        case SupportType.TType: return ttypeSupport;
        default: return null;
      }
    }
  }
}