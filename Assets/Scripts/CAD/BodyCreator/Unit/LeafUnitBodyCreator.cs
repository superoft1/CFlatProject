using Chiyoda.CAD.Model;
using UnityEngine;
using Chiyoda.CAD.Plotplan;

namespace Chiyoda.CAD.Body
{
  public class LeafUnitBodyCreator : BodyCreator<LeafUnit, LeafUnitBody>
  {
    public LeafUnitBodyCreator(Entity entity) : base(entity)
    {
    }

    protected override void SetupGeometry(LeafUnitBody body, LeafUnit lu)
    {
      var go = body.gameObject;
      go.transform.rotation = Quaternion.identity;
      go.transform.localPosition = Vector3.zero;
      go.transform.localRotation = Quaternion.identity;
    }

    protected override void SetupMaterials(LeafUnitBody body, LeafUnit lu)
    {
    }
  }
}