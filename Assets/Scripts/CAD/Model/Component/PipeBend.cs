using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.PipeBend )]
  public class PipeBend : Component
  {
    public PipeBend( Document document ) : base( document )
    {
      ComponentName = "Bend";
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