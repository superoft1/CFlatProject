using System;
using System.Collections.Generic;
using System.Linq;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
	[Entity(EntityType.Type.FixHeatExchanger)]
	public class FixHeatExchanger : Equipment
	{
    // Filterのノズル種別
    public enum NozzleKind
    {
    }

    private readonly Memento<double> lengthOfTube;
    private readonly Memento<double> diameterOfTube;
    private readonly Memento<double> diameterOfShellCover;
    private readonly Memento<double> lengthOfFlange;

    private readonly MementoList<NozzleKind> nozzleKindList;

    public FixHeatExchanger( Document document ) : base( document )
    {
      EquipmentName = "FixHeatExchanger";

      lengthOfTube = CreateMementoAndSetupValueEvents(0.0);
      diameterOfTube = CreateMementoAndSetupValueEvents(0.0);
      diameterOfShellCover = CreateMementoAndSetupValueEvents(0.0);
      lengthOfFlange = CreateMementoAndSetupValueEvents(0.0);

      nozzleKindList = new MementoList<NozzleKind>(this);
    }

    public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
    {
      base.CopyFrom(another, storage);

      var entity = another as FixHeatExchanger;
      lengthOfTube.CopyFrom( entity.lengthOfTube.Value );
      diameterOfTube.CopyFrom( entity.diameterOfTube.Value );
      diameterOfShellCover.CopyFrom( entity.diameterOfShellCover.Value );
      lengthOfFlange.CopyFrom( entity.lengthOfFlange.Value );

      nozzleKindList.AddRange(entity.nozzleKindList);
    }

    public override Bounds GetBounds()
    {
      var xSize = (float)Math.Max(DiameterOfFlange, LengthOfSaddle);
      var ySize = (float)(LengthOfTube + 2.0 * LengthOfFlange + 2.0 * LengthOfShellCover);
      var height = (float)(HeightOfSaddle + DiameterOfTube + 0.5 * (DiameterOfFlange - DiameterOfTube));
      var sizeOfBounds = new Vector3(xSize, ySize, height);
      var centerOfBounds = new Vector3(0.0f, (float)(0.5 * LengthOfTube - DistanceOf1stSaddle), (float)(0.5 * height));
      return new Bounds(centerOfBounds, sizeOfBounds);
    }

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfTube", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LengthOfTube
    {
      get => lengthOfTube.Value;
      set => lengthOfTube.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfTube", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DiameterOfTube
    {
      get => diameterOfTube.Value;
      set => diameterOfTube.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "DiameterOfShellCover", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double DiameterOfShellCover
    {
      get => diameterOfShellCover.Value;
      set => diameterOfShellCover.Value = value;
    }

    [UI.Property(UI.PropertyCategory.BaseData, "LengthOfFlange", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
    public double LengthOfFlange
    {
      get => lengthOfFlange.Value;
      set => lengthOfFlange.Value = value;
    }

    public double DistanceOf1stSaddle => LengthOfTube / 3.0;
    public double DistanceOf2ndSaddle => 2.0 * LengthOfTube / 3.0;
    public double WidthOfSaddle => 0.25; // TODO: 固定値だが不明 KettleTypeHEを参考に
    public double HeightOfSaddle => 1.0; // TODO: 固定値だが不明 KettleTypeHEを参考に
    public double LengthOfSaddle => 0.5 * DiameterOfTube;
    public double LengthOfShellCover => 0.25 * DiameterOfTube;
    public double DiameterOfFlange => DiameterOfTube + 0.1;

    public override Vector3d GetNozzleDirection(Nozzle nozzle)
    {
      throw new NotImplementedException();
    }

    public override Vector3d GetNozzleOriginPosition(Nozzle nozzle)
    {
      throw new NotImplementedException();
    }

	}
}