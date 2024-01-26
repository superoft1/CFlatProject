using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.YStrainer )]
  public class YStrainer : Component
  {
    public YStrainer( Document document ) : base( document )
    {
      ComponentName = "YStrainer";
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
