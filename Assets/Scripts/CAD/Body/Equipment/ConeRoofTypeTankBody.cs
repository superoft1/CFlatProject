using UnityEngine;

namespace Chiyoda.CAD.Body
{
  public class ConeRoofTypeTankBody : Body
  {
    [SerializeField]
    GameObject cylinderBody;

    [SerializeField]
    GameObject capBody;

    public GameObject CylinderBody
    {
      get
      {
        return cylinderBody;
      }

      set
      {
        cylinderBody = value;
      }
    }

    public GameObject CapBody
    {
      get
      {
        return capBody;
      }

      set
      {
        capBody = value;
      }
    }
  }
}
