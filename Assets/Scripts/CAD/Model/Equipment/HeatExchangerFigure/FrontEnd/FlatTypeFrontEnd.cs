using System;
using Chiyoda.CAD.Core;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.FlatTypeFrontEnd)]
  public class FlatTypeFrontEnd : FrontEnd
  {
    private readonly Memento<double> length;

    public FlatTypeFrontEnd( Document document ) : base( document )
    {
      length = CreateMemento( 0.0 ) ;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "Length", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double Length
    {
      get => length.Value;
      set => length.Value = value;
    }

    public override double LengthOfFrontEnd => Length;
  }
}