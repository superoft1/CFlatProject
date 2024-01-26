using System;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.FlatTypeRearEnd)]
  public class FlatTypeRearEnd : RearEnd
	{
    private readonly Memento<double> length;

    public FlatTypeRearEnd( Document document ) : base( document )
		{
      length = CreateMementoAndSetupValueEvents(0.0);
		}

    [UI.Property(UI.PropertyCategory.BaseData, "Length", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double Length
    {
      get => length.Value;
      set => length.Value = value;
    }

    public override double LengthOfRearEnd => Length;
  }
}
