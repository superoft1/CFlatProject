using System;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  public abstract class RearEnd : SubEquipment
  {
    protected RearEnd( Document document ) : base( document )
    {
    }

    public HorizontalHeatExchanger HeatExchanger { get; set; }

    public override Bounds GetBounds()
    {
      if (HeatExchanger == null || HeatExchanger.Shell == null) return new Bounds();
      var centerOfRearEnd = new Vector3(0.0f, (float)(HeatExchanger.Shell.LengthOfShell + 0.5 * LengthOfRearEnd), (float)HeatExchanger.Shell.HeightOfRearEndCenter);
      var sizeOfRearEnd = new Vector3((float)HeatExchanger.Shell.DiameterOfRearEndFlange, (float)LengthOfRearEnd, (float)HeatExchanger.Shell.DiameterOfRearEndFlange);
      return new Bounds(centerOfRearEnd, sizeOfRearEnd);
    }

    public abstract double LengthOfRearEnd
    {
      get;
    }

    internal void UpdateForcibly()
    {
      OnAfterNewlyValueChanged();
    }
  }
}