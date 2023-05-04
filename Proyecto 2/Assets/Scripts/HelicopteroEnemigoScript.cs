using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class HelicopteroEnemigoScript : MonoBehaviour
{
    public enum Estado {DESPEGAR,VAGAR,ESQUIVAROBSTACULOFRENTE,ESQUIVAROBSTACULOABAJO }
    public Estado estado;
    private const float VELVERT = 0.2f;
    private float alturaDeseada;
    private Rigidbody rb;
    private float masa;
    private float fuerzaLevitacion;
    private RaycastHit[] detectSens;
    private Vector3 posicionDeseada;
    public float RapidezHorizontal = 10f;
    public GameObject guia;
    void Start()
    {
        estado = Estado.DESPEGAR;
        alturaDeseada = 10;
        rb = GetComponent<Rigidbody>();
        masa = rb.mass; // Masa del helicoptero (1000 Kg)
        fuerzaLevitacion = -(Physics.gravity.y * masa); // Fuerza de levitaci�n del helicoptero (Fuerza necesaria para anular las fuerzas)
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
                //EsquivarObstaculoAbajo();
                break;
        }
    }
    private void Despegar()
    {
        AlcanzarAltura(alturaDeseada, VELVERT);
        if (transform.position.y >= alturaDeseada - 1)
        {
            estado = Estado.VAGAR;
            guia.GetComponent<GuiaEnemigoScript>().CambiarAIrMeta();
            //StartCoroutine("VagarRutina");
        }
    }
    private void Vagar()
    {
        AlcanzarAltura(alturaDeseada, VELVERT);
        AlcanzarPosicion(guia, RapidezHorizontal);
        if (Vector3.Distance(detectSens[0].point, guia.transform.position)<2)
        {
            guia.GetComponent<GuiaEnemigoScript>().CambiarDestino();
        }
        /*
        if (detectSens[0].collider.tag=="Edificio")
        {
            StopCoroutine("VagarRutina");
            StartCoroutine("EsquivarAbajoRutina");
            estado = Estado.ESQUIVAROBSTACULOABAJO;
        }
        */
    }
    /*
    private void EsquivarObstaculoAbajo()
    {
        AlcanzarAltura(alturaDeseada, VELVERT);
        posicionDeseada = transform.forward + new Vector3(0, 0, 10);
        AlcanzarPosicion(posicionDeseada, RapidezHorizontal, 2f);
        if (detectSens[0].distance>=10)
        {
            StopCoroutine("EsquivarAbajoRutina");
        }
        if (detectSens[0].distance >= 20)
        {
            estado = Estado.VAGAR;
            StartCoroutine("VagarRutina");
        }
    }
    */
    IEnumerator EsquivarAbajoRutina()
    {
        while (true)
        {
            alturaDeseada++;
            yield return new WaitForSeconds(0.2f);
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
    private void AlcanzarPosicion(GameObject objeto, float velocidadHorizontal)
    {
        // Posici�n objeto teniendo en cuenta la altura del helic�ptero.
        Vector3 posObj = new Vector3(objeto.transform.position.x, transform.position.y, objeto.transform.position.z);
        Vector3 vectorDireccionObjetivo = posObj - transform.position;
        float velRel = objeto.GetComponent<Rigidbody>().velocity.magnitude - rb.velocity.magnitude;
        float anguloVDirYVVel = Vector3.Angle(vectorDireccionObjetivo, rb.velocity);
        if (velRel > 0 || anguloVDirYVVel < 70)
        {
            float factor = vectorDireccionObjetivo.magnitude * velocidadHorizontal;
            rb.AddForce(vectorDireccionObjetivo * factor);
            rb.transform.LookAt(posObj);
        }
        else
        {
            rb.AddForce(vectorDireccionObjetivo * masa * 10);
            rb.AddForce(-rb.velocity* 5 * masa);
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