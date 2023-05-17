using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CajaScript : MonoBehaviour
{
    public GameObject helicoptero;
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("montana"))
        {
            GetComponent<FixedJoint>().connectedBody = null;
            helicoptero.GetComponent<HelicopteroScript>().choqueCaja = true;
        }
    }
}
