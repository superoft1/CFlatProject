using System;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using Importer.BlockPattern.Equipment;

using UnityEngine;

namespace Chiyoda.UI
{
  public class PressureReliefValveEquipmentIcon : EquipmentIcon
  {
    protected override void CreateInitialElement(Document document, Action<Edge> onFinish)
    {
      Debug.Log("PressureReliefValveEquipmentIcon.CreateInitialElement");
      PressureReliefValveBlockPatternImporter.Import(onFinish);
    }
  }
}
