using System;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.KettleTypeShell)]
  public class KettleTypeShell : Shell
	{
		private readonly Memento<double> lengthOfTube;
    private readonly Memento<double> lengthOfTip;
    private readonly Memento<double> diameterOfTip;
    private readonly Memento<double> diameterOfDrum;

		public KettleTypeShell( Document document ) : base( document )
		{
      lengthOfTube = CreateMementoAndSetupValueEvents(0.0);
      lengthOfTip = CreateMementoAndSetupValueEvents(0.0);

      diameterOfTip = CreateMementoAndSetupValueEvents(0.0);
      diameterOfDrum = CreateMementoAndSetupValueEvents(0.0);
		}

    public override Bounds GetBounds()
    {
      var centerOfShell = new Vector3(0.0f, (float)(0.5 * LengthOfShell), (float)(0.5 * HeightOfShell));
      var sizeOfShell = new Vector3((float)DiameterOfRearEndFlange, (float)LengthOfShell, (float)HeightOfShell);
      return new Bounds(centerOfShell, sizeOfShell);
    }

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfTube", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LengthOfTube
    {
      get => lengthOfTube.Value;
      set => lengthOfTube.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfTip", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LengthOfTip
    {
      get => lengthOfTip.Value;
      set => lengthOfTip.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfTip", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DiameterOfTip
    {
      get => diameterOfTip.Value;
      set => diameterOfTip.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfDrum", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DiameterOfDrum
    {
      get => diameterOfDrum.Value;
      set => diameterOfDrum.Value = value;
    }

    public double LengthOfCone => 0.5 * (DiameterOfDrum - DiameterOfTip) * Math.Sqrt(3.0);

    public override double LengthOfShell => LengthOfTube + LengthOfCone + LengthOfTip;
    public override double HeightOfFrontEndCenter => HeightOfSaddle + 0.5 * DiameterOfDrum;// TODO: Excelの図では少し違う(HeightOfSaddle + 0.5 * DiameterOfTip)
    public override double HeightOfRearEndCenter => HeightOfSaddle + 0.5 * DiameterOfDrum;
    public override double DiameterOfFrontEnd => DiameterOfTip;
    public override double DiameterOfRearEnd => DiameterOfDrum;
    public override double DiameterOfFrontEndFlange => DiameterOfTip + 0.1;// TODO: 0.1は適当
    public override double DiameterOfRearEndFlange => DiameterOfDrum + 0.1;// TODO: 0.1は適当

    public double HeightOfShell => HeightOfSaddle + 0.5 * DiameterOfDrum + 0.5 * DiameterOfRearEndFlange;
  }
}
