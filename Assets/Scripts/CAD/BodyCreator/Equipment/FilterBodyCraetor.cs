using System.Collections.Generic;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
	public class FilterBodyCreator : BodyCreator<Filter, FilterBody>
	{
		public FilterBodyCreator(Entity _entity) : base(_entity)
		{
		}

		protected override void SetupMaterials(FilterBody body, Filter filter)
		{
			SetupMaterial(body.CylinderBody, body, filter);
			SetupMaterial(body.NorthLegBody, body, filter);
			SetupMaterial(body.SouthLegBody, body, filter);
			SetupMaterial(body.EastLegBody, body, filter);
			SetupMaterial(body.WestLegBody, body, filter);
			SetupMaterial(body.FlangeBody, body, filter);
			SetupMaterial(body.CapBody, body, filter);
		}

		private void SetupMaterial(GameObject go, FilterBody body, Filter filter)
		{
			go.GetComponent<MeshRenderer>().material = GetMaterial(body, filter);
		}

		protected override void SetupGeometry(FilterBody body, Filter filter)
		{
			var go = body.gameObject;
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one ;

			CreateFilter(go, filter, body);
			CreateLegs(go, filter, body);
			CreateFlange(go, filter, body);
			CreateCap(go, filter, body);
		}

		void CreateFilter(GameObject top, Filter filter, FilterBody body)
		{
			var filterBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			filterBody.transform.parent = top.transform;
			filterBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(filter.HeightOfLeg + 0.5 * filter.HeightOfCylinder));
			filterBody.transform.localRotation = Quaternion.LookRotation(Vector3.up);
			filterBody.transform.localScale = new Vector3((float)filter.DiameterOfFilter, (float)(0.5 * filter.HeightOfCylinder), (float)filter.DiameterOfFilter);
			body.CylinderBody = filterBody;
		}

		void CreateLegs(GameObject top, Filter filter, FilterBody body)
		{
			var northLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
			northLegBody.transform.parent = top.transform;
			northLegBody.transform.localPosition = new Vector3(0.0f, (float)(0.5 * (filter.DiameterOfFilter + filter.LegThickness)), (float)(0.5 * filter.HeightOfLeg));
			northLegBody.transform.localRotation = Quaternion.identity;
			northLegBody.transform.localScale = new Vector3((float)filter.WidthOfLegs, (float)filter.LegThickness, (float)filter.HeightOfLeg);
			body.NorthLegBody = northLegBody;

			var southLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
			southLegBody.transform.parent = top.transform;
			southLegBody.transform.localPosition = new Vector3(0.0f, -(float)(0.5 * (filter.DiameterOfFilter + filter.LegThickness)), (float)(0.5 * filter.HeightOfLeg));
			southLegBody.transform.localRotation = Quaternion.identity;
			southLegBody.transform.localScale = new Vector3((float)filter.WidthOfLegs, (float)filter.LegThickness, (float)filter.HeightOfLeg);
			body.SouthLegBody = southLegBody;

			var eastLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
			eastLegBody.transform.parent = top.transform;
			eastLegBody.transform.localPosition = new Vector3((float)(0.5 * (filter.DiameterOfFilter + filter.LegThickness)), 0.0f, (float)(0.5 * filter.HeightOfLeg));
			eastLegBody.transform.localRotation = Quaternion.identity;
			eastLegBody.transform.localScale = new Vector3((float)filter.LegThickness, (float)filter.WidthOfLegs, (float)filter.HeightOfLeg);
			body.EastLegBody = eastLegBody;

			var westLegBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
			westLegBody.transform.parent = top.transform;
			westLegBody.transform.localPosition = new Vector3(-(float)(0.5 * (filter.DiameterOfFilter + filter.LegThickness)), 0.0f, (float)(0.5 * filter.HeightOfLeg));
			westLegBody.transform.localRotation = Quaternion.identity;
			westLegBody.transform.localScale = new Vector3((float)filter.LegThickness, (float)filter.WidthOfLegs, (float)filter.HeightOfLeg);
			body.WestLegBody = westLegBody;
		}

		void CreateFlange(GameObject top, Filter filter, FilterBody body)
		{
			var flangeBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			flangeBody.transform.parent = top.transform;
			flangeBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(filter.HeightOfLeg + filter.HeightOfCylinder + 0.5 * filter.HeightOfFlange));
			flangeBody.transform.localRotation = Quaternion.LookRotation(Vector3.up);
      flangeBody.transform.localScale = new Vector3((float)filter.DiameterOfFlange, (float)(0.5 * filter.HeightOfFlange), (float)filter.DiameterOfFlange);
			body.FlangeBody = flangeBody;
		}

		void CreateCap(GameObject top, Filter filter, FilterBody body)
		{
			var capBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			capBody.transform.parent = top.transform;
			capBody.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(filter.HeightOfLeg));
			capBody.transform.localRotation = Quaternion.identity;
			capBody.transform.localScale = new Vector3((float)filter.DiameterOfFilter, (float)filter.DiameterOfFilter, (float)(2.0 * filter.LengthOfCap));
			body.CapBody = capBody;
		}
	}
}
