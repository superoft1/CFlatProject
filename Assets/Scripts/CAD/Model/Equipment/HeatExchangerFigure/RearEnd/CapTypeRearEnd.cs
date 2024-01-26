using System;
using Chiyoda.CAD.Core;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.CapTypeRearEnd)]
  public class CapTypeRearEnd : RearEnd
	{
    private readonly Memento<double> lengthOfTube;

		public CapTypeRearEnd( Document document ) : base( document )
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
    public override double LengthOfRearEnd => CapLength + LengthOfTube;
  }
}