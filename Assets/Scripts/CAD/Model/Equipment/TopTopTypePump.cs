using System;
using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
	[Entity(EntityType.Type.TopTopTypePump)]
	public class TopTopTypePump : HorizontalPump
	{
		public TopTopTypePump( Document document ) : base( document )
		{
			EquipmentName = "TopTopTypePump";
		}
	}
}
