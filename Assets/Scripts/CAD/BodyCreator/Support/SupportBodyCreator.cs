using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body.Support
{
  internal static class SupportBodyCreator
  {
    public static BodyCreator Create( Model.Support support )
    {
      if ( !support.Enabled ) return null;

      switch ( support.SupportType ) {
        case SupportType.PipeShoe: return new PipeShoeSupportBodyCreator( support );
        case SupportType.Trunnion: return new TrunnionSupportBodyCreator( support );
        case SupportType.TType: return new TTypeSupportBodyCreator( support );
        default: return null;
      }
    }
  }

  internal abstract class SupportBodyCreator<TBodyImpl> : BodyCreator<Model.Support, Body>
    where TBodyImpl : MonoBehaviour
  {
    protected SupportBodyCreator( Model.Support support )
      : base( support )
    { }

    protected sealed override void SetupGeometry( Body body, Model.Support entity )
    {
      SetupGeometry( body.gameObject, body.MainObject.GetComponent<TBodyImpl>(), entity );
    }

    protected abstract void SetupGeometry( GameObject gameObject, TBodyImpl bodyImpl, Model.Support entity );
  }
}
