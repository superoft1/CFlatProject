using System;
using Chiyoda.CAD.Core;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Model.Structure;
using UnityEngine;

namespace Chiyoda.CAD.Plotplan
{
  [System.Serializable]
    public abstract class Unit : Entity, IFreeDraggablePlacement
    {
        public event EventHandler NewlyLocalCodChanged;
        public event EventHandler HistoricallyLocalCodChanged;
        public event EventHandler LocalCodChanged
        {
            add
            {
                NewlyLocalCodChanged += value;
                HistoricallyLocalCodChanged += value;
            }
            remove
            {
                NewlyLocalCodChanged -= value;
                HistoricallyLocalCodChanged -= value;
            }
        }

        private readonly Memento<LocalCodSys3d> _localCod;
        private readonly Memento<double> _exRotation;
 
        protected Unit(Document document) : base(document)
        {
            _localCod = CreateMemento(LocalCodSys3d.Identity);
            _localCod.AfterNewlyValueChanged += OnNewlyLocalCodChanged;
            _localCod.AfterHistoricallyValueChanged += OnHistoricallyLocalCodChanged;

            _exRotation = CreateMemento(0.0);
            _exRotation.AfterNewlyValueChanged += OnNewlyLocalCodChanged;
            _exRotation.AfterHistoricallyValueChanged += OnHistoricallyLocalCodChanged;
        }

        public override void CopyFrom(ICopyable another, CopyObjectStorage storage)
        {
            base.CopyFrom(another, storage);
            var entity = another as Unit;
            _localCod.CopyFrom(entity._localCod.Value);
            _exRotation.CopyFrom(entity._exRotation.Value);
        }

        protected virtual void OnNewlyLocalCodChanged(object sender, EventArgs e)
        {
            NewlyLocalCodChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnHistoricallyLocalCodChanged(object sender, EventArgs e)
        {
            HistoricallyLocalCodChanged?.Invoke(this, EventArgs.Empty);
        }

       // public abstract override IEnumerable<IElement> Children
       // {
       //     get;
       // }

        //未実装
        public LocalCodSys3d ParentCod
        {
            get
            {
                    return LocalCodSys3d.Identity;
            }
        }

        public LocalCodSys3d GlobalCod
        {
            get
            {
                return ParentCod.GlobalizeCodSys(LocalCod);
            }
        }

        public LocalCodSys3d LocalCod
        {
            get
            {
                if (0 == _exRotation.Value)
                {
                    return _localCod.Value;
                }
                else
                {
                    var cod = _localCod.Value;
                    var q = cod.Rotation;
                    var r1 = Quaternion.AngleAxis((float)_exRotation.Value, new Vector3(0, 0, 1));
                    return new LocalCodSys3d(cod.Origin, r1 * q,cod.IsMirrorType);
                }
            }

            set
            {
                if (0 == _exRotation.Value)
                {
                    _localCod.Value = value;
                }
                else
                {
                    var r1 = Quaternion.AngleAxis(-(float)_exRotation.Value, new Vector3(0, 0, 1));
                    _localCod.Value = new LocalCodSys3d(value.Origin, r1 * value.Rotation,value.IsMirrorType);
                }
            }
        }
    }

}
