using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class HelicopteroEnemigoScript : MonoBehaviour
{
    public enum Estado {DESPEGAR,VAGAR,ESQUIVAR }
    public Estado estado;
    private const float VELVERT = 0.2f;
    private const float VELGIR = 1000f;
    public const float VELHOR = 100f;
    private float alturaDeseada;
    private Rigidbody rb;
    private float masa;
    private float fuerzaLevitacion;
    private RaycastHit[] detectSens;
    private Vector3 posicionDeseada;
    public GameObject guia;

    private float distanciaObstaculo;
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
        if (DestinoAlcanzado())
        {
            guia.GetComponent<GuiaEnemigoScript>().CambiarDestino();
        }
        if (ObstaculoLateralDetectado())
        {
            estado = Estado.ESQUIVAR;
            StartCoroutine(AumentarAltura(distanciaObstaculo,rb.velocity.magnitude));
        }
        AlcanzarAltura(alturaDeseada, VELVERT);
        AlcanzarPosicion(guia, VELHOR);
    }
    private bool DestinoAlcanzado()
    {
        //print("Distancia: "+Vector3.Distance(transform.position, guia.transform.position));
        return Vector3.Distance(new Vector3(transform.position.x,0,transform.position.z),new Vector3(guia.transform.position.x,0,guia.transform.position.z)) <= 20;
    }
    private void Esquivar()
    {
        if (!ObstaculoLateralDetectado())
        {
            estado = Estado.VAGAR;
            return;
        }
        AlcanzarAltura(alturaDeseada, VELVERT);
    }
    IEnumerator AumentarAltura(float distancia,float velocidad)
    {
        while (ObstaculoLateralDetectado())
        {
            alturaDeseada += 1f;
            yield return new WaitForSeconds((distancia/velocidad) * 0.1f);
        }
    }
    private bool ObstaculoLateralDetectado()
    {
        bool obstaculo = false;
        int pos = 1;
        if (Sensores())
        {
            while (!obstaculo && pos < detectSens.Length)
            {
                if (detectSens[pos].collider != null
                    && detectSens[pos].collider.gameObject.CompareTag("edificio")
                    && detectSens[pos].distance < rb.velocity.magnitude)
                {
                    Vector3 vDirEdificio = detectSens[pos].point - transform.position;
                    if (Vector3.Angle(vDirEdificio, rb.velocity) < 90)
                    {
                        distanciaObstaculo = detectSens[pos].distance;
                        obstaculo = true;
                    }
                }
                pos++;
            }
        }
        return obstaculo;
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
        //float velRel = objeto.GetComponent<Rigidbody>().velocity.magnitude - rb.velocity.magnitude;
        float anguloVDirYVVel = Vector3.Angle(vectorDireccionObjetivo, rb.velocity);
        float velocidadGuia = objeto.GetComponent<NavMeshAgent>().velocity.magnitude;

        if (anguloVDirYVVel > 70) // Si el �ngulo entre el vector direcci�n al objetivo y el vector velocidad del helicoptero
                                  // es mayor de 70, es decir, nos hemos pasado del objetivo.
        {
            if (Vector3.Distance(transform.position, posObj) >= 3) // Si la distancia es mayor a 3, debemos girarnos para volver
                                                                   // atr�s, ya que nos hemos pasado demasiado.
            {
                //transform.LookAt(posObj);
                giroFisico(vectorDireccionObjetivo, VELGIR);
                rb.AddForce(transform.forward * masa * 10);
                //print("Me paso con distancia mayor a 3.");
            }
            else    // Si la distancia es menor a 3, no hace falta girar. Corregimos para llegar al objetivo.
            {
                rb.AddForce(vectorDireccionObjetivo * masa * 10);
                //print("Me paso con distancia menor a 3.");
            }
            rb.AddForce(-rb.velocity * 5 * masa); // Debemos frenar para corregir.
        }
        else if (Vector3.Distance(transform.position, posObj) < 3) // Si la distancia entre el helicoptero y el objetivo es
                                                                   // menor a 3, mantenemos posici�n.
        {
            float factor = vectorDireccionObjetivo.magnitude * velocidadHorizontal;
            rb.AddForce(vectorDireccionObjetivo * factor);
            noGirar(VELGIR); // frenamos el giro.
            //print("Mantener posici�n.");
        }
        else if (Vector3.Distance(transform.position, posObj) < rb.velocity.magnitude
                || (velocidadGuia > 0 && rb.velocity.magnitude > velocidadGuia)) // Frenamos en caso de que vayamos m�s rapido
                                                                                 // que el objeto guia agente que seguimos,
                                                                                 // o si la distancia entre el objeto guia
                                                                                 // agente y el helicoptero es menor que la
                                                                                 // velocidad del helicoptero.
        {
            //rb.AddForce(-transform.forward * masa);
            rb.AddForce(-rb.velocity * masa);
            //print("Frenar.");
        }
        else            // En cualquier otro caso aceleramos hacia el objeto guia.
        {
            //transform.LookAt(posObj);
            giroFisico(vectorDireccionObjetivo, VELGIR);
            float factor = vectorDireccionObjetivo.magnitude * velocidadHorizontal;
            rb.AddForce(transform.forward * factor);
            //print("Acelerar.");
        }
    }
    private void giroFisico(Vector3 vectorDir, float velocidadGiro)
    {
        // Calculamos el producto escalar del vector3.up y el vector direcci�n hacia el objetivo.
        // Obtenemos el positivo y el negativo.
        Vector3 perpYObjPos = Vector3.Cross(Vector3.up, vectorDir);
        Vector3 perpYObjNeg = -Vector3.Cross(Vector3.up, vectorDir);
        // Calculamos para cada vector, el �ngulo con el transform.forward del helicoptero.
        float anguloPos = Vector3.Angle(perpYObjPos, transform.forward);
        float anguloNeg = Vector3.Angle(perpYObjNeg, transform.forward);
        Vector3 ejeGiro = Vector3.up; // Eje en el que vamos a girar.
        if (anguloPos < anguloNeg)
        { // Si el �ngulo con el vector positivo es menor, giramos hacia la izquierda, si no, hacia la derecha.
            ejeGiro *= -1;
            if (rb.angularVelocity.y > 0) // Si nos pasamos, corregimos haciendo un frenado m�s fuerte.
            {
                rb.AddRelativeTorque(ejeGiro * velocidadGiro * 2);
            }
        }
        else
        {
            if (rb.angularVelocity.y < 0)
            {
                rb.AddRelativeTorque(ejeGiro * velocidadGiro * 2);
            }
        }
        rb.AddRelativeTorque(ejeGiro * velocidadGiro);
    }
    private void noGirar(float velocidadGiro)
    {
        Vector3 ejeGiro = Vector3.up; // Eje en el que vamos a girar.
        if (rb.angularVelocity.y > 0) // si estamos girando hacia la derecha, hacemos fuerza hacia la izquierda.
        {
            ejeGiro *= -1;
            rb.AddRelativeTorque(ejeGiro * velocidadGiro);
        }
        else if (rb.angularVelocity.y < 0) // si estamos girando hacia la izquierda, hacemos fuerza hacia la derecha.
        {
            rb.AddRelativeTorque(ejeGiro * velocidadGiro);
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
