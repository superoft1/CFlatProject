using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Presenter ;
using Chiyoda.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Chiyoda.CAD.Core
{
  class ChiyodaEventSystem : EventSystem
  {
    [SerializeField]
    DocumentTreeView documentTreeView;

    [SerializeField]
    PresenterManager presenterManager;

    private new void Update()
    {
      base.Update();

      if ( null != documentTreeView ) {
        documentTreeView.Maintain() ;
      }
      
      if ( null != presenterManager ) {
        presenterManager.ExecuteUpdate();
      }
    }
  }
}
