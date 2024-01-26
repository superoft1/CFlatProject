using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class SubStationBody : Body
  {
    [SerializeField]
    GameObject mainBody;

    public GameObject MainBody
    {
      get
      {
        return mainBody;
      }

      set
      {
        mainBody = value;
      }
    }
  }
}
