using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelicopteroEnemigoScript : MonoBehaviour
{
    private enum Estado {DESPEGAR,VAGAR,ESQUIVAROBSTACULO }
    private Estado estado;
    private const float VELVERT = 0.2f;
    private float alturaDeseada;
    private Rigidbody rb;
    private float masa;
    private float fuerzaLevitacion;
    private RaycastHit[] detectSens;
    private Vector3 posicionDeseada;
    public float RapidezHorizontal = 1f;
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
        switch (estado)
        {
            case Estado.DESPEGAR:
                Despegar();
                break;
            case Estado.VAGAR:
                Vagar();
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
        AlcanzarPosicion(posicionDeseada, RapidezHorizontal, 0.2f);
    }
    IEnumerator VagarRutina()
    {
        while (true)
        {
            float alturaAleatoria = Random.Range(10, 20);
            alturaDeseada = alturaAleatoria;
            float posicionAleatoriaX = Random.Range(transform.position.x-20, transform.position.x+20);
            if (posicionAleatoriaX > 100) posicionAleatoriaX = 100f;
            if (posicionAleatoriaX < -100) posicionAleatoriaX = -100f;
            float posicionAleatoriaZ = Random.Range(transform.position.z-20, transform.position.z+20);
            if (posicionAleatoriaZ > 100) posicionAleatoriaZ = 100f;
            if (posicionAleatoriaZ < -100) posicionAleatoriaZ = -100f;
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
}
