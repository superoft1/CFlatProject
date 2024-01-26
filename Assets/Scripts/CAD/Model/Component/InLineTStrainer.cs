using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.InLineTStrainer )]
  public class InLineTStrainer : Component
  {
    public InLineTStrainer( Document document ) : base( document )
    {
      ComponentName = "InLineTStrainer";
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      base.ChangeSizeNpsMm(connectPointNumber, newDiameterNpsMm);
    }
  }
}
