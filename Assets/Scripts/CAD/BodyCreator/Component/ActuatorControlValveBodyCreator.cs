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
                var impl = body.MainObject.GetComponent<ActuatorControlValveBodyImpl>();
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

            var impl = body.MainObject.GetComponent<ActuatorControlValveBodyImpl>();

            impl.MainValve.transform.localScale = impl.MainValve.transform.localScale * ModelScale;

            float oper_Dim_A = (float)actuatorControlValve.Oper_Dim_A;

            var partAScale = impl.PartA.transform.localScale;
            impl.PartA.transform.localScale = new Vector3((float)oper_Dim_A, (float)partAScale.y, (float)partAScale.z);

            var refOperationPos = impl.ReferenceOperation.transform.localPosition;
            impl.ReferenceOperation.transform.localPosition = new Vector3((float)refOperationPos.x, -(float)oper_Dim_A / (2 * 10), (float)refOperationPos.z);

            var mainOperationPos = impl.MainOperation.transform.localPosition;
            impl.MainOperation.transform.localPosition = new Vector3((float)mainOperationPos.x, (float)oper_Dim_A / (2 * 10), (float)mainOperationPos.z);


            //impl.MainValve.transform.localScale = new Vector3((float)actuatorControlValve.Length / 2, (float)actuatorControlValve.Diameter, (float)actuatorControlValve.Diameter) * ModelScale;
            //impl.ReferenceOperation.transform.localScale = new Vector3((float)actuatorControlValve.DiaphramLength, (float)actuatorControlValve.DiaphramDiameter, (float)actuatorControlValve.DiaphramDiameter) * ModelScale;
        }

        protected void ChangePosition(Transform targetTransform)
        {

        }
    }

}