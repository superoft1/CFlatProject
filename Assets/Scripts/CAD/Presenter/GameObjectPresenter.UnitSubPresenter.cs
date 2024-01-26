using Chiyoda.CAD.Body ;
using Chiyoda.CAD.Plotplan;
using Chiyoda.UI ;
using UnityEngine ;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter
  {
    private class UnitSubPresenter : SubPresenter<Unit>
    {
      public UnitSubPresenter(GameObjectPresenter basePresenter) : base(basePresenter)
      {
      }

      protected override bool IsRaised(Unit unit)
      {
        return ! ( unit is LeafUnit ) || BodyMap.ContainsBody( unit ) ;
      }

      protected override void Raise(Unit unit)
      {
        var body = BodyFactory.CreateBody(unit);
        body.IsHighlighted = true;
        body.transform.SetParent( RootGameObject.transform ) ;
        body.gameObject.name = unit.Name;
        body.gameObject.SetActive(false);

        var unitBody = body.GetComponentInChildren<UnitBody>();
        if ( unit is LeafUnit leafUnit ) {
          unitBody.Init( leafUnit ) ;
        }

        BodyMap.Add( unit, body ) ;
      }

      protected override void Update(Unit unit)
      {
        BodyMap.TryGetBody(unit, out var body);
        var unitBody = body as Body.Body;
        if (unitBody == null) return;

        if ( unit is LeafUnit leafUnit ) {
          unitBody.MainObject.transform.localScale = (Vector3) leafUnit.AreaSize ;
        }
      }

      protected override void TransformUpdate(Unit unit)
      {
        BodyMap.TryGetBody(unit, out var body);
        var unitBody = body as Body.Body;
        if (unitBody == null) return;

        unitBody.transform.SetLocalCodSys(unit.LocalCod);
      }

      protected override void Destroy(Unit unit)
      {
        BodyMap.Remove( unit ) ;
      }
    }
  }
}