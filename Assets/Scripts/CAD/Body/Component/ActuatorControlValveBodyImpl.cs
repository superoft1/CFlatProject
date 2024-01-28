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
        GameObject operationConnector;

        [SerializeField]
        GameObject cylinder1;

        [SerializeField]
        GameObject cylinder2;

        [SerializeField]
        GameObject cube1;

        [SerializeField]
        GameObject cube2;

        [SerializeField]
        GameObject head;

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

        public GameObject OperationConnector
        {
            get
            {
                return operationConnector;
            }

            set
            {
                operationConnector = value;
            }
        }

        public GameObject Cylinder1
        {
            get
            {
                return cylinder1;
            }

            set
            {
                cylinder1 = value;
            }
        }

        public GameObject Cylinder2
        {
            get
            {
                return cylinder2;
            }

            set
            {
                cylinder2 = value;
            }
        }

        public GameObject Cube1
        {
            get
            {
                return cube1;
            }

            set
            {
                cube1 = value;
            }
        }

        public GameObject Cube2
        {
            get
            {
                return cube2;
            }

            set
            {
                cube2 = value;
            }
        }

        public GameObject Head
        {
            get
            {
                return head;
            }

            set
            {
                head = value;
            }
        }
    }
}