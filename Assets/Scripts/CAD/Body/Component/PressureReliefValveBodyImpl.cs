using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chiyoda.CAD.Model;

namespace Chiyoda.CAD.Body
{

    public class PressureReliefValveBodyImpl : MonoBehaviour
    {
        [SerializeField]
        GameObject inlet;

        [SerializeField]
        GameObject outlet;

        [SerializeField] 
        GameObject bonnet;
        
        [SerializeField] 
        GameObject cap;

        public GameObject Inlet
        {
            get
            {
                return inlet;
            }

            set
            {
                inlet = value;
            }
        }

        public GameObject Outlet
        {
            get
            {
                return outlet;
            }

            set
            {
                outlet = value;
            }
        }
        
        public GameObject Bonnet
        {
            get
            {
                return bonnet;
            }

            set
            {
                bonnet = value;
            }
        }
        
        public GameObject Cap
        {
            get
            {
                return cap;
            }

            set
            {
                cap = value;
            }
        }
    }
}
