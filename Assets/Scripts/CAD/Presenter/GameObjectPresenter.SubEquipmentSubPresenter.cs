using System;
using Chiyoda.CAD.Body;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Presenter
{
	partial class GameObjectPresenter
	{
		private class SubEquipmentSubPresenter : SubPresenter<SubEquipment>
		{
			public SubEquipmentSubPresenter(GameObjectPresenter basePresenter) : base(basePresenter) { }

			protected override bool IsRaised(SubEquipment sub)
			{
				return BodyMap.ContainsBody(sub);
			}

			protected override void Raise(SubEquipment sub)
			{ }

			protected override void Update( SubEquipment sub )
			{
				Body.Body subObject ;
				if ( false == BodyMap.TryGetBody( sub, out var body ) ) {
					subObject = BodyFactory.CreateBody( sub ) ;
					BodyMap.Add( sub, subObject ) ;
				}
				else {
					var oldSubEquipmentObject = body as Body.Body ;
					subObject = BodyFactory.UpdateBody( sub, oldSubEquipmentObject ) ;
					if ( subObject != oldSubEquipmentObject ) {
						Destroy( sub ) ;
						BodyMap.Add( sub, subObject ) ;
					}
				}

				var equipment = sub.Parent as Equipment ;
				BodyMap.TryGetBody( equipment, out var iBody ) ;
				var equipmentBody = iBody as Body.Body ;
				var parentBody = GetParentBody( subObject ) ;
				if ( parentBody != equipmentBody ) {
					if ( null != subObject ) {
						if ( null != equipmentBody ) {
							subObject.gameObject.transform.SetParent( equipmentBody.transform, false ) ;
						}
						else {
							subObject.gameObject.transform.SetParent( null, false ) ;
						}
					}
				}

        // Shellの変更があった場合にはFrontEnd/RearEndも強制的に更新する
        var shell = sub as Shell;
        if (shell != null) 
        {
          var frontEnd = shell.HeatExchanger.FrontEnd;
          if (frontEnd != null)
          {
            frontEnd.UpdateForcibly();
          }
          var rearEnd = shell.HeatExchanger.RearEnd;
          if (rearEnd != null)
          {
            rearEnd.UpdateForcibly();
          }
        }
			}

			protected override void TransformUpdate(SubEquipment sub)
			{
			}

			protected override void Destroy(SubEquipment sub)
			{
				IBody body;
				if (BodyMap.TryGetBody(sub, out body))
				{
					BodyMap.Remove(sub);
				}
				body.RemoveFromView();
			}

			private static Body.Body GetParentBody(Body.Body body)
			{
				if (null == body) return null;

				Transform parent = body.transform.parent;
				if (null == parent) return null;

				return parent.GetComponent<Body.Body>();
			}
		}
	}
}