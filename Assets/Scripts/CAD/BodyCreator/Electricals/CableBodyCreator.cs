using System.Linq ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Electricals ;
using UnityEngine ;

namespace Chiyoda.CAD.Body
{

  public class CableBodyCreator : BodyCreator<Cable, CableBody>
  {
    public CableBodyCreator(Entity _entity) : base(_entity)
    {
    }
    protected override void SetupMaterials( CableBody body, Cable entity )
    {
    }
    protected override void SetupGeometry( CableBody body, Cable entity )
    {
      if(!entity.Points.Any()) return;

      // 原点は始点とする
      var transform = body.transform ;

      transform.position = (Vector3) entity.StartPoint ;
      transform.rotation = Quaternion.identity ;
      
      if (!body.Cables.Any()) {
        foreach ( var startEndPoint in entity.CableStartEnd ) {
          body.Cables.Add( CreateCable( body.gameObject, entity.Diameter, startEndPoint ));
        }
      }
    }
    
    GameObject CreateCable(GameObject parent, double diameter, (Vector3d, Vector3d) startEndPoint)
    {
      var direction = (Vector3)( startEndPoint.Item2 - startEndPoint.Item1 ) ;
      var theBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      theBody.transform.parent = parent.transform;
      theBody.transform.localPosition = (Vector3)((startEndPoint.Item1 + startEndPoint.Item2)/2.0);
      theBody.transform.localScale = new Vector3((float)diameter, (float)diameter, direction.magnitude);
      theBody.transform.localRotation = Quaternion.LookRotation( direction );
      return theBody;
    }
  }
}