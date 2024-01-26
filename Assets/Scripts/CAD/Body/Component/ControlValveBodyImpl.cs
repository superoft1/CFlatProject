using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{

    public class ControlValveBodyImpl : MonoBehaviour
    {
        [SerializeField]
        GameObject mainValve;

        [SerializeField]
        GameObject referenceOperation;

        public GameObject ReferenceOperation
        {
            get
            {
                return referenceOperation;
            }

            set
            {
                referenceOperation = value;
            }
        }

        public GameObject MainValve
        {
            get
            {
                return mainValve;
            }

            set
            {
                mainValve = value;
            }
        }
    }
}
