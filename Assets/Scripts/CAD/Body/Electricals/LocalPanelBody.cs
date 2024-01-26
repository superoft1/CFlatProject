using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class LocalPanelBody : Body
  {
    [SerializeField]
    GameObject panelBody;

    public GameObject PanelBody
    {
      get
      {
        return panelBody;
      }

      set
      {
        panelBody = value;
      }
    }
  }
}
