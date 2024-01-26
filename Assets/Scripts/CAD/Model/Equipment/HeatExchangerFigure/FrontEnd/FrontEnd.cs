using System;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  public abstract class FrontEnd : SubEquipment
  {
    protected FrontEnd( Document document ) : base( document )
    {
    }

    public HorizontalHeatExchanger HeatExchanger { get; set; }

    public override Bounds GetBounds()
    {
      var centerOfFrontEnd = new Vector3(0.0f, (float)(-0.5 * LengthOfFrontEnd), (float)HeatExchanger.Shell.HeightOfFrontEndCenter);
      var sizeOfFrontEnd = new Vector3((float)(HeatExchanger.Shell.DiameterOfFrontEndFlange), (float)LengthOfFrontEnd, (float)HeatExchanger.Shell.DiameterOfFrontEndFlange);
      return new Bounds(centerOfFrontEnd, sizeOfFrontEnd);
    }

    public abstract double LengthOfFrontEnd
    {
      get;
    }

    internal void UpdateForcibly()
    {
      OnAfterNewlyValueChanged();
    }
  }
}
