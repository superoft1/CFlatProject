using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using Chiyoda.UI ;
#if UNITY_STANDALONE_WIN
using InteractiveNamedPipe ;
#endif
using UnityEngine;

public class InteractiveNamedPipeClient : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
  private InteractiveNamedPipe.State _lastState = State.Disconnected ;
#endif

  public void OnClickConnectButton()
  {
#if UNITY_STANDALONE_WIN
    _clientNamedPipe.Connect();
#endif
  }

  public void OnClickDisconnectButton()
  {
#if UNITY_STANDALONE_WIN
    _clientNamedPipe.Disconnect();
#endif
  }

#if UNITY_STANDALONE_WIN
  private InteractiveNamedPipe.Client _clientNamedPipe;

  private void Start()
  {
    // 名前付きパイプによる双方向通信クライアント側クラス生成
    _clientNamedPipe = new InteractiveNamedPipe.Client("VTPProdrawPipe", 1024, OnReadPlain, OnReadInstruments, OnReadStreams);
//    _clientNamedPipe.Connect(); // 起動時には自動でコネクト待ちをしない。view操作が重くなるため。
  }

  private void OnApplicationQuit()
  {
    _clientNamedPipe.Disconnect();
  }

  private void OnDestroy()
  {
    _clientNamedPipe.Disconnect();
  }

  private void Update()
  {
    _clientNamedPipe.Polling();
    var curState = _clientNamedPipe.GetState() ;
    if ( curState != _lastState ) {
      switch ( curState ) {
        case State.Connected :
          Debug.Log( "InteractiveNamedPipeClient Connected!" ) ;
          break ;
        case State.Disconnected :
          Debug.Log( "InteractiveNamedPipeClient Disconnected!" ) ;
          break ;
        default :
          break ;
      }
    }
    _lastState = curState ;
  }

  // メッセージ受信時に呼ばれるコールバック
  private void OnReadPlain(string message)
  {
  }

  private void OnReadInstruments(List<string> list)
  {
    var doc = DocumentCollection.Instance.Current;
    if (doc == null)
    {
      return;
    }

    var elements = new List<IElement>();
    foreach (var instrument in doc.BlockPatterns.SelectMany(b => b.Equipments))
    {
      if (list.Contains(instrument.EquipNo))
      {
        elements.Add(instrument);
      }
    }

    doc.SelectElements(elements);
    DocumentTreeView.Instance().Fit(elements);
  }

  private void OnReadStreams(List<string> list)
  {
    var doc = DocumentCollection.Instance.Current;
    if (doc == null)
    {
      return;
    }

    var elements = new List<IElement>();
    foreach (var route in doc.Routes)
    {
      if (list.Contains(route.LineId))
      {
        elements.Add(route);
      }
    }

    doc.SelectElements(elements);
    DocumentTreeView.Instance().Fit(elements);
  }
#endif
}
