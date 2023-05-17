using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalaScript : MonoBehaviour
{
    public bool cocheAlcanzado;
    // Start is called before the first frame update
    void Start()
    {
        cocheAlcanzado = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("coche")) cocheAlcanzado = true;
    }
}
