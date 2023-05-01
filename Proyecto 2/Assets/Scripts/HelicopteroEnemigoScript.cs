using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopteroEnemigoScript : MonoBehaviour
{
    private enum Estado {DESPEGAR,VAGAR,ESQUIVAROBSTACULOFRENTE,ESQUIVAROBSTACULOABAJO }
    private Estado estado;
    private const float VELVERT = 0.2f;
    private float alturaDeseada;
    private Rigidbody rb;
    private float masa;
    private float fuerzaLevitacion;
    private RaycastHit[] detectSens;
    private Vector3 posicionDeseada;
    public float RapidezHorizontal = 10f;
    void Start()
    {
        estado = Estado.DESPEGAR;
        alturaDeseada = 10;
        rb = GetComponent<Rigidbody>();
        masa = rb.mass; // Masa del helicoptero (1000 Kg)
        fuerzaLevitacion = -(Physics.gravity.y * masa); // Fuerza de levitación del helicoptero (Fuerza necesaria para anular las fuerzas)
        detectSens = new RaycastHit[5];
        posicionDeseada = transform.position;
    }
    private void FixedUpdate()
    {
        Sensores();
        switch (estado)
        {
            case Estado.DESPEGAR:
                Despegar();
                break;
            case Estado.VAGAR:
                Vagar();
                break;
            case Estado.ESQUIVAROBSTACULOABAJO:
                EsquivarObstaculoAbajo();
                break;
        }
    }
    private void Despegar()
    {
        AlcanzarAltura(alturaDeseada, VELVERT);
        if (transform.position.y >= alturaDeseada - 1)
        {
            estado = Estado.VAGAR;
            StartCoroutine("VagarRutina");
        }
    }
    private void Vagar()
    {
        AlcanzarAltura(alturaDeseada, VELVERT);
        AlcanzarPosicion(posicionDeseada, RapidezHorizontal, 2f);
        if (detectSens[0].collider != null)
        {
            StopCoroutine("VagarRutina");
            estado = Estado.ESQUIVAROBSTACULOABAJO;
        }
    }
    private void EsquivarObstaculoAbajo()
    {
        AlcanzarAltura(alturaDeseada, VELVERT);
        AlcanzarPosicion(posicionDeseada, RapidezHorizontal, 2f);
        alturaDeseada += detectSens[0].point.y;
        if (transform.position.y >= alturaDeseada - 1)
        {
            estado = Estado.VAGAR;
            StartCoroutine("VagarRutina");
        }
    }
    IEnumerator VagarRutina()
    {
        while (true)
        {
            float alturaAleatoria = Random.Range(10, 20);
            alturaDeseada = alturaAleatoria;
            float posicionAleatoriaX = Random.Range(transform.position.x-10, transform.position.x+10);
            if (posicionAleatoriaX > 50) posicionAleatoriaX = 50f;
            if (posicionAleatoriaX < -50) posicionAleatoriaX = -50f;
            float posicionAleatoriaZ = Random.Range(transform.position.z-10, transform.position.z+10);
            if (posicionAleatoriaZ > 50) posicionAleatoriaZ = 50f;
            if (posicionAleatoriaZ < -50) posicionAleatoriaZ = -50f;
            posicionDeseada = new Vector3(posicionAleatoriaX, alturaDeseada, posicionAleatoriaZ);
            yield return new WaitForSeconds(3f);
        }
    }
    private void AlcanzarAltura(float altura, float velocidadVertical)
    {
        float distanciaAObjetivo = (altura - transform.position.y);
        if (rb.velocity.y >= 0)
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
    private void AlcanzarPosicion(Vector3 posObjetivo, float rapidezHorizontal, float propulsionFrontal)
    {
        Vector3 vectorHaciaObjetivo = posObjetivo - transform.position;
        float velocidadRelativa = 10f;
        float angulo = Vector3.Angle(vectorHaciaObjetivo, GetComponent<Rigidbody>().velocity);
        if ((velocidadRelativa > 0) || (angulo < 70))
        {
            float factor = vectorHaciaObjetivo.magnitude * rapidezHorizontal;
            rb.AddForce(vectorHaciaObjetivo * propulsionFrontal * factor);
            rb.transform.LookAt(new Vector3(posObjetivo.x, rb.transform.position.y, posObjetivo.z));
        }
        else
        { //Ir frenando... Tarea: cambiar la siguiente instrucción por rb.addForce... con el mismo efecto.
            rb.velocity = rb.velocity * 0.95f;
            rb.transform.LookAt(new Vector3(posObjetivo.x, rb.transform.position.y, posObjetivo.z));
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
