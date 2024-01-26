using System.Collections;
using System.Collections.Generic;
using Chiyoda.UI ;
using UnityEngine;

public class EquipmentUI : MonoBehaviour {

  [SerializeField]
  SubmenuView instrumentView;

  public void InstrumentButtonClicked()
  {
    instrumentView.Show();
  }
}
