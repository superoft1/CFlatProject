using System.Linq ;
using Chiyoda.CAD.Model ;
using Chiyoda.CAD.Model.Electricals ;
using UnityEngine ;

namespace Chiyoda.CAD.Body
{

  public class CablePathBodyCreator : BodyCreator<CablePath, CablePathBody>
  {
    public CablePathBodyCreator(Entity _entity) : base(_entity)
    {
    }
    protected override void SetupMaterials( CablePathBody body, CablePath entity )
    {
    }
    protected override void SetupGeometry( CablePathBody body, CablePath entity )
    {
      var transform = body.transform ;
      transform.position = (Vector3) entity.Origin ;
      transform.rotation = Quaternion.identity;

      if (body.PlaneBody == null) {
        body.PlaneBody = CreateBody( body.gameObject, entity ) ;
      }
      body.PlaneBody.transform.localScale = (Vector3)entity.Size + Vector3.forward* 0.001f;
    }
    
    GameObject CreateBody(GameObject parent, CablePath entity)
    {
      var theBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
      theBody.transform.parent = parent.transform;
      theBody.transform.localPosition = Vector3.zero ;
      theBody.transform.localRotation = Quaternion.identity;
      return theBody;
    }

  }
}