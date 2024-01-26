using System;
using Chiyoda.CAD.Core;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.CapTypeFrontEnd)]
  public class CapTypeFrontEnd : FrontEnd
	{
    private readonly Memento<double> lengthOfTube;

    public CapTypeFrontEnd( Document document ) : base( document )
		{
      lengthOfTube = CreateMementoAndSetupValueEvents(0.0);
		}

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfTube", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LengthOfTube
    {
      get => lengthOfTube.Value;
      set => lengthOfTube.Value = value;
    }

    public double CapLength => HeatExchanger.Shell.DiameterOfRearEnd / 4.0;
    public override double LengthOfFrontEnd => CapLength + LengthOfTube;
  }
}