using System.Collections.Generic;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter
  {
    private class DocumentSubPresenter : SubPresenter<Document>
    {
      public DocumentSubPresenter( GameObjectPresenter basePresenter ) : base( basePresenter ) { }

      protected override bool IsRaised( Document document )
      {
        return (null != BasePresenter._roots.GetComponent<DocumentBody>());
      }

      protected override void Raise( Document document )
      {
        var body = BasePresenter._roots.AddComponent<DocumentBody>();
        body.Document = document;
      }

      protected override void Update( Document document )
      {
      }
      protected override void TransformUpdate( Document document )
      {
      }

      protected override void Destroy( Document document )
      {
        var body = BasePresenter._roots.GetComponent<DocumentBody>();
        GameObject.DestroyImmediate( body );
      }
    }
  }
}
