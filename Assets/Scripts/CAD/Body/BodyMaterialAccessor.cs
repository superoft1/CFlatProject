using System;
using System.Collections.Generic;
using UnityEngine;

using Chiyoda.CAD.Model;
using Chiyoda.CAD.Plotplan;

namespace Chiyoda.CAD.Body
{

  public class BodyMaterialAccessor : MonoBehaviour
  {
    [SerializeField] Material green = null ;

    [SerializeField] Material purple = null ;

    [SerializeField] Material blue = null ;

    [SerializeField] Material white = null ;

    [SerializeField] Material foundation = null ;

    [SerializeField] Material piperack = null ;

    [SerializeField] Material green_highlighted = null ;

    [SerializeField] Material purple_highlighted = null ;

    [SerializeField] Material blue_highlighted = null ;

    [SerializeField] Material white_highlighted = null ;

    [SerializeField] Material foundation_highlighted = null ;

    [SerializeField] Material piperack_highlighted = null ;

    [SerializeField] Material nozzle = null ;

    [SerializeField] Material nozzle_highlighted = null ;

    [SerializeField] Material line = null ;

    [SerializeField]
    Material region_none = null;

    [SerializeField]
    Material region_dragging_enabled = null;

    [SerializeField]
    Material region_dragging_disabled = null;

    [SerializeField]
    Material leafUnit = null;

        public Material GetLineMaterial()
    {
      return line ;
    }


    private class MaterialInfo
    {
      public Material Normal { get ; private set ; }
      public Material Highlighted { get ; private set ; }

      public MaterialInfo( Material normal, Material highlighted )
      {
        Normal = normal ;
        Highlighted = highlighted ;
      }
    }

    private readonly Dictionary<System.Type, MaterialInfo> _materialInfo = new Dictionary<System.Type, MaterialInfo>() ;
    private MaterialInfo _defaultMaterialPair ;


    public static BodyMaterialAccessor Instance()
    {
      return instance ;
    }

    static BodyMaterialAccessor instance = null ;

    private void Awake()
    {
      if ( instance == null ) {
        instance = this ;
        SetupMaterialPair() ;
      }
    }

    private void SetupMaterialPair()
    {
      var greenInfo = new MaterialInfo( green, green_highlighted ) ;
      var purpleInfo = new MaterialInfo( purple, purple_highlighted ) ;
      var whiteInfo = new MaterialInfo( white, white_highlighted ) ;
      var blueInfo = new MaterialInfo( blue, blue_highlighted ) ;
      var piperackInfo = new MaterialInfo( piperack, piperack_highlighted ) ;
      var nozzleInfo = new MaterialInfo( nozzle, nozzle_highlighted ) ;

      // green
      _materialInfo.Add( typeof( Pipe ), greenInfo ) ;

      // purple
      _materialInfo.Add( typeof( GateValve ), purpleInfo ) ;
      _materialInfo.Add( typeof( WeldNeckFlange ), purpleInfo ) ;
      _materialInfo.Add( typeof( CheckValve ), purpleInfo ) ;
      _materialInfo.Add( typeof( ThreeWayInstrumentRootvalve ), purpleInfo ) ;
      _materialInfo.Add( typeof( BlindFlange ), purpleInfo ) ;
      _materialInfo.Add( typeof( Flange ), purpleInfo ) ;
      _materialInfo.Add( typeof( PipingCap ), purpleInfo ) ;
      _materialInfo.Add( typeof( WeldOlet ), purpleInfo ) ;
      _materialInfo.Add( typeof( SockOlet ), purpleInfo ) ;
      _materialInfo.Add( typeof( OpenSpectacleBlank ), purpleInfo ) ;
      _materialInfo.Add( typeof( BlankSpectacleBlank ), purpleInfo ) ;
      _materialInfo.Add( typeof( SlipOnFlange ), purpleInfo ) ;
      _materialInfo.Add( typeof( ButterflyValve ), purpleInfo ) ;
      _materialInfo.Add( typeof( BallValve ), purpleInfo ) ;
      _materialInfo.Add( typeof( OrificePlate ), purpleInfo ) ;
      _materialInfo.Add( typeof( VenturiTube ), purpleInfo ) ;
      _materialInfo.Add( typeof( ControlValve ), purpleInfo) ;
      
      // white
      _materialInfo.Add( typeof( EndTopTypePump ), whiteInfo ) ;

      _materialInfo.Add( typeof( Column ), whiteInfo ) ;
      _materialInfo.Add( typeof( SkirtTypeVessel ), whiteInfo ) ;
      _materialInfo.Add( typeof( LegTypeVessel ), whiteInfo ) ;
      _materialInfo.Add( typeof( SphericalTypeTank ), whiteInfo ) ;
      _materialInfo.Add(typeof(LeafUnit), whiteInfo);

      _materialInfo.Add( typeof( Model.Structure.Entities.PipeRackSingle ), piperackInfo ) ;
      _materialInfo.Add( typeof( Model.Structure.Entities.PipeRack1Plus1), piperackInfo );
      _materialInfo.Add( typeof( Model.Structure.Entities.ConnectionSingleFrame ), piperackInfo ) ;
      _materialInfo.Add( typeof( Model.Structure.Entities.Connection1Plus1 ), piperackInfo );
      _materialInfo.Add( typeof( Model.Structure.Entities.EquipmentStructure ), piperackInfo );
      _materialInfo.Add( typeof( Model.Structure.Entities.StructureRoom ), piperackInfo );
      
      _materialInfo.Add( typeof( Nozzle ), nozzleInfo ) ;
      

      // blue
      _defaultMaterialPair = blueInfo ;
    }

    public Material GetMaterial( Entity entity, bool highlight = false )
    {
      MaterialInfo pair ;
      if ( ! _materialInfo.TryGetValue( entity.GetType(), out pair ) ) {
        pair = _defaultMaterialPair ;
      }

      return highlight ? pair.Highlighted : pair.Normal ;
    }

    public Material GetFoundationMaterial( Entity entity, bool highlight = false )
    {
      return highlight ? foundation_highlighted : foundation ;
    }

    public Material GetRegionMaterial( RegionBody.BodyVisibility visibility )
    {
      switch ( visibility ) {
        case RegionBody.BodyVisibility.Enabled :
          return region_dragging_enabled ;
        case RegionBody.BodyVisibility.Disabled :
          return region_dragging_disabled ;
        case RegionBody.BodyVisibility.OutOfRange :
          return region_dragging_disabled ;
        default :
          return region_none ;
      }
    }
  }
}