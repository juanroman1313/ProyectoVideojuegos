using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopteroScript : MonoBehaviour
{
    private enum Estado {DESPEGAR}
    private Estado estado;
    private const float VELVERT = 0.2f;
    private float alturaDeseada;
    private Rigidbody rb;
    private float masa;
    private float fuerzaLevitacion;
    private RaycastHit[] detectSens; 
    void Start()
    {
        estado = Estado.DESPEGAR;
        alturaDeseada = 0;
        rb = GetComponent<Rigidbody>();
        masa = rb.mass; // Masa del helicoptero (1000 Kg)
        fuerzaLevitacion = -(Physics.gravity.y * masa); // Fuerza de levitaci�n del helicoptero (Fuerza necesaria para anular las fuerzas)
        detectSens = new RaycastHit[5];
    }
    private void FixedUpdate()
    {
        alturaDeseada = 10;
        AlcanzarAltura(alturaDeseada, VELVERT);
        Sensores();
        switch (estado)
        {
            case Estado.DESPEGAR:
                Despegar();
            break;
        }
    }
    private void Despegar()
    {

    }
    // M�todo para hacer que el helic�ptero alzance una altura determinada, manejando la variable alturaDeseada.
    private void AlcanzarAltura(float altura, float velocidadVertical)
    {
        float distanciaAObjetivo = (altura - transform.position.y);
        if(rb.velocity.y >= 0)
        {
            float factor = distanciaAObjetivo * velocidadVertical;
            rb.AddForce(Vector3.up * fuerzaLevitacion * factor);
        }
        else
        {
            float factor = Mathf.Max(0, distanciaAObjetivo * velocidadVertical);
            rb.AddForce(Vector3.up * fuerzaLevitacion * factor * 10);
        }
    }

    // Sensores para detectar objetos del entorno.
    private bool Sensores()
    {
        bool d1 = Physics.Raycast(transform.position - transform.up * 0.5f, -transform.up, out detectSens[0]);
        Debug.DrawRay(transform.position - transform.up * 0.5f, -transform.up * 10, Color.red);
        bool d2 = Physics.Raycast(transform.position + transform.forward * 2.5f, transform.forward, out detectSens[1]);
        Debug.DrawRay(transform.position + transform.forward * 2.5f, transform.forward * 10, Color.red);
        bool d3 = Physics.Raycast(transform.position + transform.right * 2f, transform.right, out detectSens[2]);
        Debug.DrawRay(transform.position + transform.right * 2f, transform.right * 10, Color.red);
        bool d4 = Physics.Raycast(transform.position - transform.forward * 4.5f, -transform.forward, out detectSens[3]);
        Debug.DrawRay(transform.position - transform.forward * 4.5f, -transform.forward * 10, Color.red);
        bool d5 = Physics.Raycast(transform.position - transform.right * 2f, -transform.right, out detectSens[4]);
        Debug.DrawRay(transform.position - transform.right * 2f, -transform.right * 10, Color.red);
        return d1 || d2 || d3 || d4 || d5;
    }
}
