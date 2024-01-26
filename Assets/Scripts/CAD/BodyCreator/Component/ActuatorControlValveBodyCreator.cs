using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
    public class ActuatorControlValveBodyCreator : BodyCreator<ActuatorControlValve, Body>
    {
        public ActuatorControlValveBodyCreator(Entity _entity) : base(_entity)
        { }

        protected override void SetupMaterials(Body body, ActuatorControlValve actuatorControlValve)
        {
            if (Topology.Route.HasColor(entity, out Color newColor))
            {
                ChangeMaterialColor(body, newColor);
            }
            else
            {
                var impl = body.MainObject.GetComponent<ControlValveBodyImpl>();
                var renderers = impl.MainValve.GetComponentsInChildren<MeshRenderer>();
                var material = GetMaterial(body, actuatorControlValve);
                foreach (var render in renderers)
                {
                    render.material = material;
                }
                impl.ReferenceOperation.GetComponent<MeshRenderer>().material = GetMaterial(body, actuatorControlValve);
            }
        }

        protected override void SetupGeometry(Body body, ActuatorControlValve actuatorControlValve)
        {
            var go = body.gameObject;

            go.transform.localPosition = (Vector3)actuatorControlValve.Origin;
            go.transform.localRotation = Quaternion.identity;

            body.MainObject.transform.localPosition = Vector3.zero;
            body.MainObject.transform.localRotation = Quaternion.identity;

            var impl = body.MainObject.GetComponent<ControlValveBodyImpl>();
            impl.MainValve.transform.localScale = new Vector3((float)actuatorControlValve.Length / 2, (float)actuatorControlValve.Diameter, (float)actuatorControlValve.Diameter) * ModelScale;
            impl.ReferenceOperation.transform.localScale = new Vector3((float)actuatorControlValve.DiaphramLength, (float)actuatorControlValve.DiaphramDiameter, (float)actuatorControlValve.DiaphramDiameter) * ModelScale;
        }
    }

}