using System;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
	[Entity(EntityType.Type.NozzleOnPlane)]
	public class NozzleOnPlane : Nozzle
	{
		public NozzleOnPlane( Document document ) : base( document )
		{
			this.x = CreateMementoAndSetupValueEvents(0.0);
			this.y = CreateMementoAndSetupValueEvents(0.0);

      this.xAxis = CreateMementoAndSetupValueEvents(Vector3d.left);
      this.yAxis = CreateMementoAndSetupValueEvents(Vector3d.up);
		}

		private readonly Memento<double> x;
		private readonly Memento<double> y;
    private readonly Memento<Vector3d> xAxis;
    private readonly Memento<Vector3d> yAxis;
		
		public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
		{
			base.CopyFrom( another, storage );

			var entity = another as NozzleOnPlane;
			this.x.CopyFrom( entity.x.Value ) ;
			this.y.CopyFrom( entity.y.Value ) ;
      this.xAxis.CopyFrom(entity.xAxis.Value);
      this.yAxis.CopyFrom(entity.yAxis.Value);
		}

		[UI.Property(UI.PropertyCategory.BaseData, "X", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 1)]
		public double X
		{
			get => x.Value;
			set => x.Value = value;
		}

		[UI.Property(UI.PropertyCategory.BaseData, "Y", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable, Order = 2)]
		public double Y
		{
			get => y.Value;
			set => y.Value = value;
		}

    public Vector3d XAxis { get => xAxis.Value; set => xAxis.Value = value; }
    public Vector3d YAxis { get => yAxis.Value; set => yAxis.Value = value; }

    public Vector3d Offset => X * XAxis + Y * YAxis;
	}
}