using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRoutingUI : MonoBehaviour {

  [SerializeField]
  AutoRoutingView autoRoutingView;

  public void AutoRoutingButtonClicked()
  {
    autoRoutingView.Show();
  }
}
