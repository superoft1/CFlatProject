using System;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  public abstract class Shell : SubEquipment
  {
    private readonly Memento<double> lengthOfSaddle;
    private readonly Memento<double> widthOfSaddle;
    private readonly Memento<double> heightOfSaddle;
    private readonly Memento<double> distanceOf1stSaddle;
    private readonly Memento<double> distanceOf2ndSaddle;

    protected Shell( Document document ) : base( document )
    {
      lengthOfSaddle = CreateMementoAndSetupValueEvents(0.0);
      widthOfSaddle = CreateMementoAndSetupValueEvents(0.0);
      heightOfSaddle = CreateMementoAndSetupValueEvents(0.0);
      distanceOf1stSaddle = CreateMementoAndSetupValueEvents(0.0);
      distanceOf2ndSaddle = CreateMementoAndSetupValueEvents(0.0);
    }

    public HorizontalHeatExchanger HeatExchanger { get; set; }

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfSaddle", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LengthOfSaddle
    {
      get => lengthOfSaddle.Value;
      set => lengthOfSaddle.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "WidthOfSaddle", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double WidthOfSaddle
    {
      get => widthOfSaddle.Value;
      set => widthOfSaddle.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "HeightOfSaddle", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double HeightOfSaddle
    {
      get => heightOfSaddle.Value;
      set => heightOfSaddle.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DistanceOf1stSaddle", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DistanceOf1stSaddle
    {
      get => distanceOf1stSaddle.Value;
      set => distanceOf1stSaddle.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DistanceOf2ndSaddle", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DistanceOf2ndSaddle
    {
      get => distanceOf2ndSaddle.Value;
      set => distanceOf2ndSaddle.Value = value;
    }

    public abstract double LengthOfShell
    {
      get;
    }

    public abstract double HeightOfFrontEndCenter
    {
      get;
    }

    public abstract double HeightOfRearEndCenter
    {
      get;
    }

    public abstract double DiameterOfFrontEnd
    {
      get;
    }

    public abstract double DiameterOfRearEnd
    {
      get;
    }

    public abstract double DiameterOfFrontEndFlange
    {
      get;
    }

    public abstract double DiameterOfRearEndFlange
    {
      get;
    }

    public double ThicknessOfFlange => 0.150; // TODO : 適当な値
  }
}
