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
using weka.core.converters;

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
    weka.classifiers.trees.M5P saberPredecirVelocidadBala;
    weka.core.Instances casosEntrenamiento;

    public GameObject bala;
    public GameObject canon;

    private float velocidadInicial;
    public GameObject coche;

    string ESTADO;
    public GameObject cochePrefab;
    public GameObject puntoFinal;
    private bool prueba;
    void Start()
    {
        estado = Estado.DESPEGAR;
        alturaDeseada = ALTURABASE;
        rb = GetComponent<Rigidbody>();
        masa = rb.mass; // Masa del helicoptero (1000 Kg) + masa imán + masa cadenas.
        fuerzaLevitacion = -(Physics.gravity.y * masa); // Fuerza de levitación del helicoptero (Fuerza necesaria para anular las fuerzas)
        velocidadInicial = 5f;
        ESTADO = "Sin conocimiento";
        prueba = false;
    }

    private void FixedUpdate()
    {
        print("ESTADO CONOCIMIENTO: "+ESTADO);
        AlcanzarAltura(alturaDeseada, VELVERT);
        if (ESTADO=="Sin conocimiento")
        {
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
        else
        {
            if (!prueba)
            {
                prueba = true;
                StartCoroutine(Prueba());
            }
            SeguirGuia();
        }
       
    }
    private void Despegar()
    {
        if (transform.position.y >= alturaDeseada - 1)
        {
            estado = Estado.SEGUIRGUIA;
            guia.GetComponent<EntrenamientoGuia>().CambiarAIrMeta();
            StartCoroutine("RutinaEntrenamiento");
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
    private void Disparar(float vInitBala)
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
        float velocidadGuia;
        for(int i = 0; i < 10; i++)
        {
            velocidadGuia = UnityEngine.Random.Range(2f, 10f);
            guia.GetComponent<EntrenamientoGuia>().CambiarVelocidad(velocidadGuia);
            //BUCLE de planificación de la velocidad inicial durante el entrenamiento
            for (float vInit = velocidadInicial; vInit <= 50; vInit += 5)                   
            {
                GameObject balaLanzada = Instantiate(bala, canon.transform.position, canon.transform.rotation) as GameObject;
                Rigidbody rbBala = balaLanzada.GetComponent<Rigidbody>();
                float masaBala = rbBala.mass;
                float fuerza = masaBala * vInit;
                balaLanzada.GetComponent<SphereCollider>().isTrigger = true;
                rbBala.AddForce(canon.transform.forward * fuerza, ForceMode.Impulse);
                yield return new WaitUntil(() => (rbBala.transform.position.y <= 2.5));        //... y espera a que la pelota llegue a una altura
                
                    Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                    casoAaprender.setDataset(casosEntrenamiento);                           //crea un registro de experiencia
                    casoAaprender.setValue(0, Vector3.Distance(transform.position, balaLanzada.transform.position));
                    casoAaprender.setValue(1, coche.GetComponent<Rigidbody>().velocity.magnitude);
                    casoAaprender.setValue(2, vInit);
                    casosEntrenamiento.add(casoAaprender);
                                                //guarda el registro de experiencia 
                                                                                   
                rbBala.isKinematic = true;    
                Destroy(balaLanzada, 1f);                                                     
            }
        }            //FIN bucle de lanzamientos con diferentes de velocidades
        ESTADO = "Con conocimiento";
        //APRENDIZADE CONOCIMIENTO:  
        saberPredecirVelocidadBala = new weka.classifiers.trees.M5P();  //crea un algoritmo de aprendizaje M5P
        casosEntrenamiento.setClassIndex(2);              //la variable a aprender será velocidad inicial de la bala
        saberPredecirVelocidadBala.buildClassifier(casosEntrenamiento);
        //Guardamos los datos
        File salida = new File("Assets/Scripts/Finales_Experiencias.arff");
        if (!salida.exists())
            System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
        ArffSaver saver = new ArffSaver();
        saver.setInstances(casosEntrenamiento);
        saver.setFile(salida);
        saver.writeBatch();
    }
    private IEnumerator PruebaDisparo()
    {
        while (true)
        {
            velocidadInicial++;
            Disparar(velocidadInicial);
            yield return new WaitForSeconds(3f);
        }
    }
    private IEnumerator Prueba()
    {
        Destroy(coche);
        GameObject cocheInstancia= Instantiate(cochePrefab, new Vector3(0, 3.05f, 19.17f), Quaternion.identity);
        cocheInstancia.GetComponent<CocheScript>().enabled = true;
        cocheInstancia.GetComponent<CocheScript>().guia = puntoFinal;
        transform.position = new Vector3(0,0.5f, 2.98f);
        guia.transform.position = new Vector3(0, 0, 2.98f);
        guia.GetComponent<EntrenamientoGuia>().meta = cocheInstancia;
        coche = cocheInstancia;
        yield return new WaitForSeconds(UnityEngine.Random.Range(10f,20f));
        Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
        casoPrueba.setDataset(casosEntrenamiento);
        casoPrueba.setValue(0, Vector3.Distance(transform.position, coche.transform.position));
        casoPrueba.setValue(1, coche.GetComponent<Rigidbody>().velocity.magnitude);
        float mejorVelocidad = (float)saberPredecirVelocidadBala.classifyInstance(casoPrueba);
        Disparar(mejorVelocidad);
    }
}
