using System;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
  [Entity(EntityType.Type.Compressor)]
  public class Compressor : Equipment
  {
    // Compressorのノズル情報
    public enum NozzleKind
    {
    }

    private readonly MementoList<double> dList;
    private readonly MementoList<NozzleKind> nozzleKindList;

    public Compressor( Document document ) : base( document )
    {
      EquipmentName = "Compressor";

      dList = CreateMementoListAndSetupValueEvents<double>();
      nozzleKindList = new MementoList<NozzleKind>(this);
    }

    public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
    {
      base.CopyFrom(another, storage);

      var entity = another as Compressor;
      nozzleKindList.CopyFrom(entity.nozzleKindList);
      dList.CopyFrom(entity.dList);
    }

    public override Bounds GetBounds()
    {
      var foundationBounds = new Bounds((Vector3)FoundationCenter, (Vector3)FoundationSize);
      var equip1Bounds = new Bounds((Vector3)Equip1Center, (Vector3)Equip1Size);
      var driver1Bounds = new Bounds((Vector3)Driver1Center, (Vector3)Driver1Size);
      var equip2Bounds = new Bounds((Vector3)Equip2Center, (Vector3)Equip2Size);
      var driver2Bounds = new Bounds((Vector3)Driver2Center, (Vector3)Driver2Size);
      var equip3Bounds = new Bounds((Vector3)Equip3Center, (Vector3)Equip3Size);
      foundationBounds.Encapsulate(equip1Bounds);
      foundationBounds.Encapsulate(driver1Bounds);
      foundationBounds.Encapsulate(equip2Bounds);
      foundationBounds.Encapsulate(driver2Bounds);
      foundationBounds.Encapsulate(equip3Bounds);
      return foundationBounds;
    }

    private double D(int index)
    {
      return dList[index - 1];
    }

    public Vector3d FoundationCenter => new Vector3d(0.0, 0.5 * D(1) - D(13) - D(6), 0.5 * D(3));
    public Vector3d FoundationSize => new Vector3d(D(2), D(1), D(3));
    public Vector3d Equip1Center => new Vector3d(0.0, 0.5 * (D(5) - D(6)), D(3) + 0.5 * D(7));
    public Vector3d Equip1Size => new Vector3d(D(8), D(6) + D(5), D(7));
    public Vector3d Driver1Center => new Vector3d(0.0, D(5) + 0.5 * D(10), D(3) + 0.5 * D(7));
    public Vector3d Driver1Size => new Vector3d(D(9), D(10), D(9));
    public Vector3d Equip2Center => new Vector3d(0.0, D(5) + D(10) + 0.5 * D(11), D(3) + 0.5 * D(7));
    public Vector3d Equip2Size => new Vector3d(D(12), D(11), D(7));
    public Vector3d Driver2Center => new Vector3d(0.0, D(5) + D(10) + D(11) + 0.5 * D(10), D(3) + 0.5 * D(7));
    public Vector3d Driver2Size => new Vector3d(D(9), D(10), D(9));
    public Vector3d Equip3Center => new Vector3d(0.0, D(5) + D(10) + D(11) + D(10) + 0.5 * D(15), D(3) + 0.5 * D(7));
    public Vector3d Equip3Size => new Vector3d(D(14), D(15), D(7));

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
