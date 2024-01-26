using System;
using Chiyoda.CAD.Core;
using UnityEngine;

namespace Chiyoda.CAD.Model
{
	[Entity(EntityType.Type.NozzleOnCylinder)]
	public class NozzleOnCylinder : Nozzle
	{
		private readonly Memento<double> _height;
		private readonly Memento<double> _angle;

		public NozzleOnCylinder( Document document ) : base( document )
		{
			this._height = CreateMementoAndSetupValueEvents( 0.0 ) ;
			this._angle = CreateMementoAndSetupValueEvents( 0.0 ) ;
		}
		
		public override void CopyFrom( ICopyable another, CopyObjectStorage storage )
		{
			base.CopyFrom( another, storage );

			var entity = another as NozzleOnCylinder;
			_height.CopyFrom( entity._height.Value );
			_angle.CopyFrom( entity._angle.Value );
		}

		[UI.Property(UI.PropertyCategory.BaseData, "Height", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
		public double Height
		{
			get
			{
				return _height.Value;
			}
			set
			{
				_height.Value = value;
			}
		}

		[UI.Property(UI.PropertyCategory.BaseData, "Angle", ValueType = UI.ValueType.GeneralNumeric, Visibility = UI.PropertyVisibility.Editable)]
		public double Angle
		{
			get
			{
				return _angle.Value;
			}
			set
			{
				_angle.Value = value;
			}
		}

	}
}