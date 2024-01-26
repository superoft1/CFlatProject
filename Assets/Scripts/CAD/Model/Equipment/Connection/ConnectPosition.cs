using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core ;
using Chiyoda.CAD.Model ;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.ConnectPosition)]
  public class ConnectPosition : Entity
  {
    public ConnectPosition( Document document ) : base( document )
    {
    }
  }

}