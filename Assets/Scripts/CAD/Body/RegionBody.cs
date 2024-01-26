using UnityEngine ;

namespace Chiyoda.CAD.Body
{
  public class RegionBody : Body
  {
    public enum BodyVisibility
    {
      None,
      Enabled,
      OutOfRange,
      Disabled,
    }

    private BodyVisibility _regionVisibility = BodyVisibility.None ;

    public BodyVisibility RegionVisibility
    {
      get => _regionVisibility ;
      set
      {
        if ( _regionVisibility == value ) return ;
        _regionVisibility = value ;

        if ( null == MainObject ) return ;

        var material = BodyMaterialAccessor.Instance().GetRegionMaterial( _regionVisibility ) ;
        foreach ( var render in MainObject.GetComponentsInChildren<MeshRenderer>() ) {
          render.material = material ;
        }
      }
    }
  }
}