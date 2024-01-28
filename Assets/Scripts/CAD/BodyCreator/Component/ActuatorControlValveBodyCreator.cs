using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{
    public class ActuatorControlValveBodyCreator : BodyCreator<ActuatorControlValve, Body>
    {
        private readonly float defaultVectorValue = 1.0f;

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
            float oper_Dim_B = (float)actuatorControlValve.Oper_Dim_B;
            float oper_Dim_C = (float)actuatorControlValve.Oper_Dim_C;
            float oper_Dim_D = (float)actuatorControlValve.Oper_Dim_D;

            // Part A
            var partAScale = impl.OperationConnector.transform.localScale;
            impl.OperationConnector.transform.localScale = new Vector3((float)oper_Dim_A, (float)partAScale.y, (float)partAScale.z);

            // Reference Operation
            var refOperationPos = impl.ReferenceOperation.transform.localPosition;
            impl.ReferenceOperation.transform.localPosition = new Vector3((float)refOperationPos.x, -(float)oper_Dim_A / (2 * 10), (float)refOperationPos.z);

            // Reference Operation
            var mainOperationPos = impl.MainOperation.transform.localPosition;
            impl.MainOperation.transform.localPosition = new Vector3((float)mainOperationPos.x, (float)oper_Dim_A / (2 * 10), (float)mainOperationPos.z);

            // (Cylinder 1 - Part B)
            // Scale
            impl.Cylinder1.transform.localScale = new Vector3((float)oper_Dim_B, (float)oper_Dim_D, (float)oper_Dim_D);

            // Position
            float partBMoveDistance = (defaultVectorValue - (float)oper_Dim_B) / (float)(2 * 10);

            var partBPos = impl.Cylinder1.transform.localPosition;
            impl.Cylinder1.transform.localPosition = new Vector3((float)partBPos.x - partBMoveDistance, (float)partBPos.y, (float)partBPos.z);

            var partBCubePos = impl.Cube1.transform.localPosition;
            impl.Cube1.transform.localPosition = new Vector3((float)partBCubePos.x - partBMoveDistance * 2, (float)partBCubePos.y, (float)partBCubePos.z);

            // (Cylinder 2 - Part C)
            // Scale
            impl.Cylinder2.transform.localScale = new Vector3((float)oper_Dim_C, (float)oper_Dim_D, (float)oper_Dim_D);

            // Position
            float partCMoveDistance = (defaultVectorValue - (float)oper_Dim_C) / (float)(2 * 10);

            var partCPos = impl.Cylinder2.transform.localPosition;
            impl.Cylinder2.transform.localPosition = new Vector3((float)partCPos.x + partCMoveDistance, (float)partCPos.y, (float)partCPos.z);

            var partCCubePos = impl.Cube2.transform.localPosition;
            impl.Cube2.transform.localPosition = new Vector3((float)partCCubePos.x + partCMoveDistance * 2, (float)partCCubePos.y, (float)partCCubePos.z);

            // Head Part
            // float partDMoveDistance = (defaultVectorValue - (float)oper_Dim_D) / (float)(10);
            var headPartPos = impl.Head.transform.localPosition;
            impl.Head.transform.localPosition = new Vector3((float)headPartPos.x, ((float)oper_Dim_D / 2 + impl.Head.transform.localScale.x) / (2 * 10), (float)headPartPos.z);


        }
    }

}