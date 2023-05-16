using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImanScript : MonoBehaviour
{
    public GameObject helicoptero;
    private bool comprobacion;
    private void Start()
    {
        comprobacion = true;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (comprobacion && collision.collider.gameObject.CompareTag("caja"))
        {
            collision.collider.gameObject.GetComponent<FixedJoint>().connectedBody = GetComponent<Rigidbody>();
            helicoptero.GetComponent<HelicopteroScript>().choqueCaja = true;
            comprobacion = false;
        }
    }
}
