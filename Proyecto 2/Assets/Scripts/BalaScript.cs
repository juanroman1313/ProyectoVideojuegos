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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("coche")) cocheAlcanzado = true;
    }
}
