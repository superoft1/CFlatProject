using Chiyoda.CAD.Model;
using UnityEngine;
using Chiyoda.CAD.Plotplan;

namespace Chiyoda.CAD.Body
{
    public class RectangleFacilityBodyCreator : BodyCreator<RectangleFacility, RectangleFacilityBody>
    {
        public RectangleFacilityBodyCreator(Entity _entity) : base(_entity)
        {
        }

        protected override void SetupMaterials(RectangleFacilityBody body, RectangleFacility rf)
        {
            SetupMaterial(body.SampleBody, body, rf);

        }

        private void SetupMaterial(GameObject go, RectangleFacilityBody body, RectangleFacility rf)
        {
            go.GetComponent<MeshRenderer>().material = GetMaterial(body, rf);
        }

        protected override void SetupGeometry(RectangleFacilityBody body, RectangleFacility rf)
        {
            var go = body.gameObject;
            go.transform.rotation = Quaternion.identity;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            CreateSample(go, rf, body);
        }



        void CreateSample(GameObject top, RectangleFacility rf, RectangleFacilityBody body)
        {
            var samplebody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            samplebody.transform.parent = top.transform;
            samplebody.transform.localPosition = rf.Origin;
            samplebody.transform.localRotation = Quaternion.identity;
            samplebody.transform.localScale =  Vector3.one;
            body.SampleBody = samplebody;

    
        }
    }
}