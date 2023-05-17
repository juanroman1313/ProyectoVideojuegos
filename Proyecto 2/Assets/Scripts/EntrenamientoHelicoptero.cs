using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using weka.classifiers.trees;
using weka.classifiers.evaluation;
using weka.core;
using java.io;
using java.lang;
using java.util;
using weka.classifiers.functions;
using weka.classifiers;

public class EntrenamientoHelicoptero : MonoBehaviour
{
    private enum Estado { DESPEGAR, SEGUIRGUIA}
    private Estado estado;
    private const float VELVERT = 0.2f;
    private const float VELHOR = 100f;
    private const float VELGIR = 1000f;
    private const float ALTURABASE = 10;
    private float alturaDeseada;
    private Rigidbody rb;
    private float masa;
    private float fuerzaLevitacion;
    public GameObject guia;
    weka.classifiers.trees.J48 saberPredecirFuerzaZ;
    weka.core.Instances casosEntrenamiento;

    public GameObject bala;
    public GameObject canon;

    private float velocidadInicial;
    void Start()
    {
        estado = Estado.DESPEGAR;
        alturaDeseada = ALTURABASE;
        rb = GetComponent<Rigidbody>();
        masa = rb.mass; // Masa del helicoptero (1000 Kg) + masa imán + masa cadenas.
        fuerzaLevitacion = -(Physics.gravity.y * masa); // Fuerza de levitación del helicoptero (Fuerza necesaria para anular las fuerzas)
        velocidadInicial = 5f;
    }

    private void FixedUpdate()
    {
        AlcanzarAltura(alturaDeseada, VELVERT);
        print("Estado helicoptero: " + estado);
        switch (estado)                         // ESTADOS SIGUIENTES:
        {
            case Estado.DESPEGAR:               // SEGUIRGUIA
                Despegar();
                break;
            case Estado.SEGUIRGUIA:             // ESQUIVAR
                SeguirGuia();
                break;
        }
    }
    private void Despegar()
    {
        if (transform.position.y >= alturaDeseada - 1)
        {
            estado = Estado.SEGUIRGUIA;
<<<<<<< HEAD
            guia.GetComponent<EntrenamientoGuia>().CambiarAIrMeta();
            StartCoroutine("PruebaDisparo");
=======
            guia.GetComponent<GuiaScript>().CambiarAIrDestinos();
            StartCoroutine("RutinaEntrenamiento");
>>>>>>> dcff2ecf480fcc6eb4dcad60c1de567a3f332159
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
    private void SeguirGuia()
    {
        AlcanzarPosicion(guia, VELHOR);

    }
    private void AlcanzarPosicion(GameObject objeto, float velocidadHorizontal)
    {
        // Posición objeto teniendo en cuenta la altura del helicóptero.
        Vector3 posObj = new Vector3(objeto.transform.position.x, transform.position.y, objeto.transform.position.z);
        Vector3 vectorDireccionObjetivo = posObj - transform.position;
        //float velRel = objeto.GetComponent<Rigidbody>().velocity.magnitude - rb.velocity.magnitude;
        float anguloVDirYVVel = Vector3.Angle(vectorDireccionObjetivo, rb.velocity);
        float velocidadGuia = objeto.GetComponent<NavMeshAgent>().velocity.magnitude;

        if (anguloVDirYVVel > 70) // Si el ángulo entre el vector dirección al objetivo y el vector velocidad del helicoptero
                                  // es mayor de 70, es decir, nos hemos pasado del objetivo.
        {
            if (Vector3.Distance(transform.position, posObj) >= 3) // Si la distancia es mayor a 3, debemos girarnos para volver
                                                                   // atrás, ya que nos hemos pasado demasiado.
            {
                //transform.LookAt(posObj);
                giroFisico(vectorDireccionObjetivo, VELGIR);
                rb.AddForce(transform.forward * masa * 10);
                print("Me paso con distancia mayor a 3.");
            }
            else    // Si la distancia es menor a 3, no hace falta girar. Corregimos para llegar al objetivo.
            {
                rb.AddForce(vectorDireccionObjetivo * masa * 10);
                print("Me paso con distancia menor a 3.");
            }
            rb.AddForce(-rb.velocity * 5 * masa); // Debemos frenar para corregir.
        }
        else if (Vector3.Distance(transform.position, posObj) < 3) // Si la distancia entre el helicoptero y el objetivo es
                                                                   // menor a 3, mantenemos posición.
        {
            float factor = vectorDireccionObjetivo.magnitude * velocidadHorizontal;
            rb.AddForce(vectorDireccionObjetivo * factor);
            NoGirar(VELGIR); // frenamos el giro.
            print("Mantener posición.");
        }
        else if (Vector3.Distance(transform.position, posObj) < rb.velocity.magnitude
                || (velocidadGuia > 0 && rb.velocity.magnitude > velocidadGuia)) // Frenamos en caso de que vayamos más rapido
                                                                                 // que el objeto guia agente que seguimos,
                                                                                 // o si la distancia entre el objeto guia
                                                                                 // agente y el helicoptero es menor que la
                                                                                 // velocidad del helicoptero.
        {
            //rb.AddForce(-transform.forward * masa);
            rb.AddForce(-rb.velocity * masa);
            print("Frenar.");
        }
        else            // En cualquier otro caso aceleramos hacia el objeto guia.
        {
            //transform.LookAt(posObj);
            giroFisico(vectorDireccionObjetivo, VELGIR);
            float factor = vectorDireccionObjetivo.magnitude * velocidadHorizontal;
            rb.AddForce(transform.forward * factor);
            print("Acelerar.");
        }
    }
    private void giroFisico(Vector3 vectorDir, float velocidadGiro)
    {
        // Calculamos el producto escalar del vector3.up y el vector dirección hacia el objetivo.
        // Obtenemos el positivo y el negativo.
        Vector3 perpYObjPos = Vector3.Cross(Vector3.up, vectorDir);
        Vector3 perpYObjNeg = -Vector3.Cross(Vector3.up, vectorDir);
        // Calculamos para cada vector, el ángulo con el transform.forward del helicoptero.
        float anguloPos = Vector3.Angle(perpYObjPos, transform.forward);
        float anguloNeg = Vector3.Angle(perpYObjNeg, transform.forward);
        Vector3 ejeGiro = Vector3.up; // Eje en el que vamos a girar.
        if (anguloPos < anguloNeg)
        { // Si el ángulo con el vector positivo es menor, giramos hacia la izquierda, si no, hacia la derecha.
            ejeGiro *= -1;
            if (rb.angularVelocity.y > 0) // Si nos pasamos, corregimos haciendo un frenado más fuerte.
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
    private void NoGirar(float velocidadGiro)
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
    private void disparar(float vInitBala)
    {
        GameObject balaLanzada = Instantiate(bala, canon.transform.position, canon.transform.rotation) as GameObject;
        Rigidbody rbBala = balaLanzada.GetComponent<Rigidbody>();
        float masaBala = rbBala.mass;
        float fuerza = masaBala * vInitBala;
        rbBala.AddForce(canon.transform.forward * fuerza, ForceMode.Impulse);
    }
    private IEnumerator RutinaEntrenamiento()
    {
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Scripts/Experiencias.arff"));
        yield return null;

    }
    private IEnumerator PruebaDisparo()
    {
        while (true)
        {
            velocidadInicial++;
            disparar(velocidadInicial);
            yield return new WaitForSeconds(3f);
        }
    }
}
