using System;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.StraightTypeShell)]
  public class StraightTypeShell : Shell
  {
    private readonly Memento<double> length;
    private readonly Memento<double> diameter;

    public StraightTypeShell( Document document ) : base( document )
    {
      length = CreateMementoAndSetupValueEvents(0.0);
      diameter = CreateMementoAndSetupValueEvents(0.0);
    }

    public override Bounds GetBounds()
    {
      var centerOfShell = new Vector3(0.0f, (float)(0.5 * LengthOfShell), (float)(0.5 * (HeightOfSaddle + 0.5 * Diameter + 0.5 * DiameterOfFlange)));
      var sizeOfShell = new Vector3((float)DiameterOfFlange, (float)LengthOfShell, (float)(HeightOfSaddle + 0.5 * Diameter + 0.5 * DiameterOfFlange));
      return new Bounds(centerOfShell, sizeOfShell);
    }

    [UI.Property(UI.PropertyCategory.BaseData, "Length", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double Length
    {
      get => length.Value;
      set => length.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "Diameter", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double Diameter
    {
      get => diameter.Value;
      set => diameter.Value = value;
    }

    public double DiameterOfFlange => Diameter + 0.1;// TODO: 0.1は適当

    public override double LengthOfShell => Length;
    public override double HeightOfFrontEndCenter => HeightOfSaddle + 0.5 * Diameter;
    public override double HeightOfRearEndCenter => HeightOfSaddle + 0.5 * Diameter;
    public override double DiameterOfFrontEnd => Diameter;
    public override double DiameterOfRearEnd => Diameter;
    public override double DiameterOfFrontEndFlange => DiameterOfFlange;
    public override double DiameterOfRearEndFlange => DiameterOfFlange;
  }
}
