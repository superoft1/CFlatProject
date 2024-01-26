using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;
using Chiyoda.CAD.Core;
using System.Linq;
using System;

namespace Chiyoda.CAD.Plotplan
{
    [System.Serializable]
    public class ProcessUnit : CompositeUnit
    {
        public ProcessUnit(Document document) : base(document)
        {

        }

        //未実装
        public override IEnumerable<IElement> Children => throw new NotImplementedException();
    }

}
