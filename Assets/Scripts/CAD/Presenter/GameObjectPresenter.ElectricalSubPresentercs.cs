using Chiyoda.CAD.Body;
using Chiyoda.CAD.Model.Electricals ;

namespace Chiyoda.CAD.Presenter
{
  partial class GameObjectPresenter
  {
    private class ElectricalSubPresenter : SubPresenter<Electricals>
    {
      public ElectricalSubPresenter(GameObjectPresenter basePresenter) : base(basePresenter) { }

      protected override bool IsRaised(Electricals element)
      {
        return BodyMap.ContainsBody(element);
      }

      protected override void Raise(Electricals element)
      { }

      protected override void Update(Electricals element)
      {
        IBody body;
        Body.Body oldObj, obj;
        if (false == BodyMap.TryGetBody(element, out body))
        {
          obj = BodyFactory.CreateBody(element);
          BodyMap.Add(element, obj);
        }
        else
        {
          oldObj = body as Body.Body;
          obj = BodyFactory.UpdateBody( element, oldObj );
          if (obj != oldObj)
          {
            Destroy(element);
            BodyMap.Add(element, obj);
          }
        }
      }
      protected override void TransformUpdate( Electricals element )
      {
      }

      protected override void Destroy(Electricals element)
      {
        IBody body;
        if (BodyMap.TryGetBody(element, out body))
        {
          BodyMap.Remove(element);
        }
        body.RemoveFromView();
      }
    }
  }
}
