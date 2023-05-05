using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class HelicopteroEnemigoScript : MonoBehaviour
{
    public enum Estado {DESPEGAR,VAGAR,ESQUIVAR }
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

    private bool edificioObstaculo;
    private float tiempoSubida;
    void Start()
    {
        estado = Estado.DESPEGAR;
        alturaDeseada = 10;
        rb = GetComponent<Rigidbody>();
        masa = rb.mass; // Masa del helicoptero (1000 Kg)
        fuerzaLevitacion = -(Physics.gravity.y * masa); // Fuerza de levitación del helicoptero (Fuerza necesaria para anular las fuerzas)
        detectSens = new RaycastHit[5];
        posicionDeseada = transform.position;
        edificioObstaculo = false;
        tiempoSubida = 0f;
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
            case Estado.ESQUIVAR:
                Esquivar();
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
        }
    }
    private void Vagar()
    {
        AlcanzarAltura(alturaDeseada, VELVERT);
        AlcanzarPosicion(guia, RapidezHorizontal);
        if (Vector3.Distance(detectSens[0].point, guia.transform.position)<2)
        {
            guia.GetComponent<GuiaEnemigoScript>().CambiarDestino();
            alturaDeseada = 10f;
        }
        if (ObstaculoDetectado())
        {
            estado = Estado.ESQUIVAR;
            edificioObstaculo = true;
            StartCoroutine("AumentarAltura");
        }
    }
    
    private void Esquivar()
    {
        print(tiempoSubida);
        AlcanzarAltura(alturaDeseada, VELVERT);
        if (edificioObstaculo && !ObstaculoDetectado())
        {
            tiempoSubida = 0f;
            edificioObstaculo = false;
        }else if (!ObstaculoDetectado())
        {
            tiempoSubida += Time.deltaTime;
        }
        if (tiempoSubida >= 5)
        {
            estado = Estado.VAGAR;
            StopCoroutine("AumentarAltura");
        }
    }
    IEnumerator AumentarAltura()
    {
        while (true)
        {
            alturaDeseada += 0.5f;
            yield return new WaitForSeconds(0.1f);
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
        // Posición objeto teniendo en cuenta la altura del helicóptero.
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
            rb.transform.LookAt(posObj);
            rb.AddForce(transform.forward * masa * 10);
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
    private bool ObstaculoDetectado()
    {
        bool obstaculo = false;
        print("pepe");
        for(int i=1;i<detectSens.Length;i++)
        {
            RaycastHit hit = detectSens[i];
            if (hit.distance <= 5 && (hit.collider!=null&&(hit.collider.CompareTag("edificio")|| hit.collider.CompareTag("montana"))))
            {
                print(hit.collider.tag);
                obstaculo = true;
                break;
            }
        }
        return obstaculo;
    }
}
