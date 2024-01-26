using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using UnityEngine;

namespace Chiyoda.CAD.Body
{
	public class FixHeatExchangerBodyCreator : BodyCreator<FixHeatExchanger, FixHeatExchangerBody>
	{
		public FixHeatExchangerBodyCreator(Entity _entity) : base(_entity)
		{
		}

		protected override void SetupMaterials(FixHeatExchangerBody body, FixHeatExchanger fixHe)
		{
			SetupMaterial(body.TubeBody, body, fixHe);
			SetupMaterial(body.FlangeBody1, body, fixHe);
			SetupMaterial(body.FlangeBody2, body, fixHe);
			SetupMaterial(body.SaddleBody1, body, fixHe);
			SetupMaterial(body.SaddleBody2, body, fixHe);
			SetupMaterial(body.ShellCoverBody1, body, fixHe);
			SetupMaterial(body.ShellCoverBody2, body, fixHe);
		}

		private void SetupMaterial(GameObject go, FixHeatExchangerBody body, FixHeatExchanger fixHe)
		{
			go.GetComponent<MeshRenderer>().material = GetMaterial(body, fixHe);
		}

		protected override void SetupGeometry(FixHeatExchangerBody body, FixHeatExchanger fixHe)
		{
			var go = body.gameObject;
			go.transform.rotation = Quaternion.identity;

			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;

			CreateTube(go, fixHe, body);
			CreateFlanges(go, fixHe, body);
			CreateShellCovers(go, fixHe, body);
			CreateSuddles(go, fixHe, body);
		}

		void CreateTube(GameObject top, FixHeatExchanger fixHe, FixHeatExchangerBody body)
		{
			var tubeBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			tubeBody.transform.parent = top.transform;
			tubeBody.transform.localPosition = new Vector3(0.0f, (float)(0.5 * fixHe.LengthOfTube - fixHe.DistanceOf1stSaddle), (float)(fixHe.HeightOfSaddle + 0.5 * fixHe.DiameterOfTube));
			tubeBody.transform.localRotation = Quaternion.identity;
			tubeBody.transform.localScale = new Vector3((float)fixHe.DiameterOfTube, (float)(0.5 * fixHe.LengthOfTube), (float)fixHe.DiameterOfTube);
			body.TubeBody = tubeBody;
		}

		void CreateFlanges(GameObject top, FixHeatExchanger fixHe, FixHeatExchangerBody body)
		{
			var flangeBody1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			flangeBody1.transform.parent = top.transform;
			flangeBody1.transform.localPosition = new Vector3(0.0f, (float)(-fixHe.DistanceOf1stSaddle - 0.5 * fixHe.LengthOfFlange), (float)(fixHe.HeightOfSaddle + 0.5 * fixHe.DiameterOfTube));
			flangeBody1.transform.localRotation = Quaternion.identity;
			flangeBody1.transform.localScale = new Vector3((float)fixHe.DiameterOfFlange, (float)(0.5 * fixHe.LengthOfFlange), (float)fixHe.DiameterOfFlange);
			body.FlangeBody1 = flangeBody1;

			var flangeBody2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			flangeBody2.transform.parent = top.transform;
			flangeBody2.transform.localPosition = new Vector3(0.0f, (float)(fixHe.LengthOfTube - fixHe.DistanceOf1stSaddle + 0.5 * fixHe.LengthOfFlange), (float)(fixHe.HeightOfSaddle + 0.5 * fixHe.DiameterOfTube));
			flangeBody2.transform.localRotation = Quaternion.identity;
			flangeBody2.transform.localScale = new Vector3((float)fixHe.DiameterOfFlange, (float)(0.5 * fixHe.LengthOfFlange), (float)fixHe.DiameterOfFlange);
			body.FlangeBody2 = flangeBody2;
		}

		void CreateShellCovers(GameObject top, FixHeatExchanger fixHe, FixHeatExchangerBody body)
		{
      // ローカル座標原点から遠い側
			var shellCoverBody1 = GameObjectUtil.CreateTaperBody((float)(0.5 * fixHe.DiameterOfTube), (float)(0.5 * fixHe.DiameterOfShellCover), (float)fixHe.LengthOfShellCover, false, true);
			shellCoverBody1.transform.parent = top.transform;
			shellCoverBody1.transform.localPosition = new Vector3(0.0f, (float)(fixHe.LengthOfTube - fixHe.DistanceOf1stSaddle + fixHe.LengthOfFlange), (float)(fixHe.HeightOfSaddle + 0.5 * fixHe.DiameterOfTube));
			shellCoverBody1.transform.localRotation = Quaternion.identity;
			shellCoverBody1.transform.localScale = Vector3.one;
			body.ShellCoverBody1 = shellCoverBody1;

      // ローカル座標原点から近い側
			var shellCoverBody2 = GameObjectUtil.CreateTaperBody((float)(0.5 * fixHe.DiameterOfTube), (float)(0.5 * fixHe.DiameterOfShellCover), (float)fixHe.LengthOfShellCover, false, true);
			shellCoverBody2.transform.parent = top.transform;
			shellCoverBody2.transform.localPosition = new Vector3(0.0f, -(float)(fixHe.DistanceOf1stSaddle + fixHe.LengthOfFlange), (float)(fixHe.HeightOfSaddle + 0.5 * fixHe.DiameterOfTube));
			shellCoverBody2.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.down);
			shellCoverBody2.transform.localScale = Vector3.one;
			body.ShellCoverBody2 = shellCoverBody2;
		}

		void CreateSuddles(GameObject top, FixHeatExchanger fixHe, FixHeatExchangerBody body)
		{
			// Saddle部分だけでモデリングすると隙間が空くためTube半径分を加味してモデリング
      // ローカル座標原点から遠い側
			var saddleBody1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			saddleBody1.transform.parent = top.transform;
			saddleBody1.transform.localPosition = new Vector3(0.0f, (float)(fixHe.DistanceOf2ndSaddle - fixHe.DistanceOf1stSaddle), (float)(0.5 * (fixHe.HeightOfSaddle + 0.5 * fixHe.DiameterOfTube)));
			saddleBody1.transform.localRotation = Quaternion.identity;
			saddleBody1.transform.localScale = new Vector3((float)fixHe.LengthOfSaddle, (float)fixHe.WidthOfSaddle, (float)(fixHe.HeightOfSaddle + 0.5 * fixHe.DiameterOfTube));
			body.SaddleBody1 = saddleBody1;

      // ローカル座標原点から近い側
      var saddleBody2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			saddleBody2.transform.parent = top.transform;
			saddleBody2.transform.localPosition = new Vector3(0.0f, 0.0f, (float)(0.5 * (fixHe.HeightOfSaddle + 0.5 * fixHe.DiameterOfTube)));
			saddleBody2.transform.localRotation = Quaternion.identity;
			saddleBody2.transform.localScale = new Vector3((float)fixHe.LengthOfSaddle, (float)fixHe.WidthOfSaddle, (float)(fixHe.HeightOfSaddle + 0.5 * fixHe.DiameterOfTube));
			body.SaddleBody2 = saddleBody2;
		}
	}
}
