using System;
using System.Collections;
using System.Collections.Generic;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Topology;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
	[Entity(EntityType.Type.SideSideTypePump)]
	public class SideSideTypePump : HorizontalPump
	{
		public SideSideTypePump( Document document ) : base( document )
		{
			EquipmentName = "SideSideTypePump";
		}
	}
}
