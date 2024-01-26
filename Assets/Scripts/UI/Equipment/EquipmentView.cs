using System ;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Topology ;
using Chiyoda.UI ;
using Chiyoda.UI.Dialog ;
using Importer.BlockPattern.Equipment ;
using Importer.BlockPattern.Equipment.EndTopTypePump ;
using Importer.BlockPattern.Equipment.SideSideTypePump ;
using Importer.BlockPattern.Equipment.TopTopTypePump ;
using UnityEngine ;

public class EquipmentView : MonoBehaviour {
  [SerializeField] EndTopTypePumpSelectDialogTemp _endTopTypePumpSelectDialogTemp ;
  [SerializeField] SideSideTypePumpSelectDialogTemp _sidesideTypePumpSelectDialogTemp;
  [SerializeField] TopTopTypePumpSelectDialogTemp _topTopTypePumpSelectDialogTemp;
  [SerializeField] HeatExchangerSelectDialogTemp _heatExchangerSelectDialogTemp;

  bool isOnDown = false;
  bool isOnCreate = false;

  private void LateUpdate()
  {    
    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
    {
      isOnDown = true;
    } else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
    {
      if (isOnDown && !isOnCreate) {
        Finish( null ) ;
      }
      isOnDown = false;
    }
  }
  
  #region TargetRegion

  private RegionHitInfo HitInfo { get ; set ; }

  public IDisposable TargetRegion( RegionHitInfo hitInfo )
  {
    return new TempTargetRegion( this, hitInfo ) ;
  }

  private class TempTargetRegion : IDisposable
  {
    private readonly EquipmentView _owner ;
    
    public TempTargetRegion( EquipmentView owner, RegionHitInfo hitInfo )
    {
      _owner = owner ;
      owner.HitInfo = hitInfo ;
    }

    ~TempTargetRegion()
    {
      throw new InvalidOperationException( "CopyObjectStorage is disposed without Dispose()." );
    }

    public void Dispose()
    {
      _owner.HitInfo = null ;
      GC.SuppressFinalize( this );
    }
  }
  
  #endregion

  private void Finish( Edge edge )
  {
    isOnCreate = false ;
    isOnDown = false ;

    if ( null != edge && null != HitInfo ) {
      // 配置
      edge.LocalCod = new LocalCodSys3d( HitInfo.HitPos, edge.LocalCod ) ;
    }

    Hide() ;
  }

  //[Placement( RegionType.Document, RequiredExtentRadius = 10 )]  // TODO
  public void TopTopTypePumpCreate()
  {
    //isOnCreate = true ;
    //TopTopTypePumpBlockPatternImporter.Import( TopTopTypePumpBlockPatternImporter.PumpType.TT_A1_S_G_N, Finish ) ;
    _topTopTypePumpSelectDialogTemp.Show("Select pump type", "");
    if (_topTopTypePumpSelectDialogTemp.OKClickedHandler == null)
    {
      _topTopTypePumpSelectDialogTemp.OKClickedHandler += (s, e) =>
      {
        isOnCreate = true;
        TopTopTypePumpBlockPatternImporter.Import(_topTopTypePumpSelectDialogTemp.GetPumpType(), Finish);
      };
    }
  }

  public void EndTopTypePumpCreate()
  {
    _endTopTypePumpSelectDialogTemp.Show( "Select pump type", "" ) ;
    if ( _endTopTypePumpSelectDialogTemp.OKClickedHandler == null ) {
      _endTopTypePumpSelectDialogTemp.OKClickedHandler += ( s, e ) =>
      {
        isOnCreate = true ;
        EndTopTypePumpBlockPatternImporter.Import( _endTopTypePumpSelectDialogTemp.GetPumpType(), Finish ) ;
      } ;
    }
  }

  public void SideSideTypePumpCreate()
  { 
    _sidesideTypePumpSelectDialogTemp.Show("Select pump type", "");
    if (_sidesideTypePumpSelectDialogTemp.OKClickedHandler == null)
    {
      _sidesideTypePumpSelectDialogTemp.OKClickedHandler += (s, e) =>
      {
        isOnCreate = true;
        SideSideTypePumpBlockPatternImporter.Import(_sidesideTypePumpSelectDialogTemp.GetPumpType(), Finish);
      };
     }
   }

  public void SkirtTypeVerticalVesselCreate()
  {
    isOnCreate = true;
    SkirtTypeVesselBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
  }

  public void LegTypeVerticalVesselCreate()
  {
    isOnCreate = true;
    LegTypeVesselBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
  }

  public void SphericalTypeTankCreate()
  {
    isOnCreate = true;
    SphericalTypeTankBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
  }

  public void ColumnCreate()
  {
    isOnCreate = true;
    ColumnBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
  }

  public void ConeRoofTypeTankCreate()
  {
    isOnCreate = true;
    ConeRoofTypeTankBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
  }

  public void HorizontalVesselCreate()
  {
    isOnCreate = true;
    HorizontalVesselBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
}

  public void HorizontalHeatExchangerCreate()
  {
    _heatExchangerSelectDialogTemp.Show("Select heat exchanger Type", "");
    if (_heatExchangerSelectDialogTemp.OKClickedHandler == null)
    {
      _heatExchangerSelectDialogTemp.OKClickedHandler += (s, e) =>
      {
        isOnCreate = true;
        HorizontalHeatExchangerBlockPatternImporter.Import(_heatExchangerSelectDialogTemp.GetHeatExchangerType(), Finish);
      };
    }
  }

  public void KettleTypeHeatExchangerCreate()
  {
    isOnCreate = true;
    KettleTypeHeatExchangerBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
  }

  public void VerticalTypeHeatExchangerCreate()
  {
    isOnCreate = true;
    VerticalHeatExchangerBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
  }

  public void PlateTypeHeatExchangerCreate()
  {
    isOnCreate = true;
    PlateTypeHeatExchangerBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
  }

  public void AirFinCoolerCreate()
  {
    isOnCreate = true;
    AirFinCoolerBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
  }

  public void VerticalPumpCreate()
  {
    isOnCreate = true;
    VerticalPumpBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
  }

  public void GenericEquipmentCreate()
  {
    isOnCreate = true;
    GenericEquipmentBlockPattenImporter.Import(edge => {
      Finish( edge );
    });
  }

  public void ChillerCreate()
  {
    isOnCreate = true;
    ChillerBlockPatternImporter.Import(edge => {
      Finish( edge );
    });
  }
  

  public void Cancel()
  {
    Hide();
  }

  void Hide()
  {
    this.gameObject.SetActive(false);
  }

  public void Show()
  {
    this.gameObject.SetActive(true);
  }

}
