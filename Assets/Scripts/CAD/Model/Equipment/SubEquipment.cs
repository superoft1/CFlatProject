using System;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Manager;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  public abstract class SubEquipment : Entity
  {
    protected SubEquipment( Document document ) : base( document )
    {
    }

    public abstract Bounds GetBounds();
  }
}