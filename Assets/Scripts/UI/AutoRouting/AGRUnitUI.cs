using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AGRUnitUI : MonoBehaviour
{
  [SerializeField]
  AGRUnitView agrUnitView;

  public void AutoRoutingButtonClicked()
  {
    agrUnitView.Show();
  }
}
