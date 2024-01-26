using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity( EntityType.Type.UNKNOWN_COMPONENT )]
  public class UnknownComponent : Component
  {
    public enum ConnectPointType
    {
      Start,
      End,
    }

    public ConnectPoint StartConnectPoint => GetConnectPoint( (int)ConnectPointType.Start ) ;
    public ConnectPoint EndConnectPoint => GetConnectPoint( (int)ConnectPointType.End ) ;

    public UnknownComponent( Document document ) : base( document )
    {
      ComponentName = "UnknownComponent" ;
    }
    protected internal override void InitializeDefaultObjects()
    {
      base.InitializeDefaultObjects();
      AddNewConnectPoint( (int) ConnectPointType.Start );
      AddNewConnectPoint( (int) ConnectPointType.End );
    }

    public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
    {
      base.CopyFrom( another, storage );

      var entity = another as UnknownComponent;
    }

    public override void ChangeSizeNpsMm(int connectPointNumber, int newDiameterNpsMm)
    {
      base.ChangeSizeNpsMm(connectPointNumber, newDiameterNpsMm);
    }
  }
}