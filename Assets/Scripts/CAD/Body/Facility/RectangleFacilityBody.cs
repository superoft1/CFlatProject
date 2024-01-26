using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Plotplan;

namespace Chiyoda.CAD.Body
{
    public class RectangleFacilityBody : Body
    {
        [SerializeField]
        GameObject sampleBody;
        public GameObject SampleBody
        {
            get
            {
                return SampleBody;
            }

            set
            {
                sampleBody = value;
            }
        }
    }
}
