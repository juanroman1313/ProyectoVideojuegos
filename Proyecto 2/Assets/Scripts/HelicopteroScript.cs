using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopteroScript : MonoBehaviour
{
    private enum Estado {DESPEGAR, SEGUIRGUIA}
    private Estado estado;
    private const float VELVERT = 0.2f;
    private const float VELHOR = 10f;
    private float alturaDeseada;
    private Rigidbody rb;
    private float masa;
    private float fuerzaLevitacion;
    private RaycastHit[] detectSens;
    public GameObject guia;
    void Start()
    {
        estado = Estado.DESPEGAR;
        alturaDeseada = 0;
        rb = GetComponent<Rigidbody>();
        masa = rb.mass; // Masa del helicoptero (1000 Kg)
        fuerzaLevitacion = -(Physics.gravity.y * masa); // Fuerza de levitación del helicoptero (Fuerza necesaria para anular las fuerzas)
        detectSens = new RaycastHit[5];
    }
    private void FixedUpdate()
    {
        alturaDeseada = 10;
        AlcanzarAltura(alturaDeseada, VELVERT);
        Sensores();
        print("Estado helicoptero: " + estado);
        switch (estado)
        {
            case Estado.DESPEGAR:
                Despegar();
                break;
            case Estado.SEGUIRGUIA:
                AlcanzarPosicion(guia, VELHOR);
                break;
        }
    }
    private void Despegar()
    {
        if(transform.position.y >= alturaDeseada - 1)
        {
            estado = Estado.SEGUIRGUIA;
            guia.GetComponent<GuiaScript>().CambiarAIrMeta();
        }
    }
    // Método para hacer que el helicóptero alzance una altura determinada, manejando la variable alturaDeseada.
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
    private void AlcanzarPosicion(GameObject objeto, float velocidadHorizontal)
    {
        // Posición objeto teniendo en cuenta la altura del helicóptero.
        Vector3 posObj = new Vector3(objeto.transform.position.x, transform.position.y, objeto.transform.position.z);
        Vector3 vectorDireccionObjetivo = posObj - transform.position;
        float velRel = objeto.GetComponent<Rigidbody>().velocity.magnitude - rb.velocity.magnitude;
        float anguloVDirYVVel = Vector3.Angle(vectorDireccionObjetivo, rb.velocity);
        if(velRel > 0 || anguloVDirYVVel < 70)
        {
            float factor = vectorDireccionObjetivo.magnitude * velocidadHorizontal;
            rb.AddForce(vectorDireccionObjetivo * factor);
            transform.LookAt(posObj);
        }
        else
        {
            rb.AddForce(vectorDireccionObjetivo * masa * 10);
        }
    }
}
