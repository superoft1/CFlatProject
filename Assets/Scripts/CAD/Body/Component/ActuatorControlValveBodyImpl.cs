using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;


namespace Chiyoda.CAD.Body
{
    public class ActuatorControlValveBodyImpl : MonoBehaviour
    {
        [SerializeField]
        GameObject mainValve;

        [SerializeField]
        GameObject referenceOperation;

        [SerializeField]
        GameObject mainOperation;

        [SerializeField]
        GameObject partA;

        [SerializeField]
        GameObject partB;

        [SerializeField]
        GameObject partC;

        [SerializeField]
        GameObject partD;

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

        public GameObject MainOperation
        {
            get
            {
                return mainOperation;
            }

            set
            {
                mainOperation = value;
            }
        }

        public GameObject PartA
        {
            get
            {
                return partA;
            }

            set
            {
                partA = value;
            }
        }

        public GameObject PartB
        {
            get
            {
                return partB;
            }

            set
            {
                partB = value;
            }
        }

        public GameObject PartC
        {
            get
            {
                return partC;
            }

            set
            {
                partC = value;
            }
        }

        public GameObject PartD
        {
            get
            {
                return partD;
            }

            set
            {
                partD = value;
            }
        }
    }
}