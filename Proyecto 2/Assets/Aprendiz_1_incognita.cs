//Programación de Videojuegos, Universidad de Málaga (Prof. M. Nuñez, mnunez@uma.es)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using weka.classifiers.trees;
using weka.classifiers.evaluation;
using weka.core;
using java.io;
using java.lang;
using java.util;
using weka.classifiers.functions;
using weka.classifiers;

public class Aprendiz_1_incognita : MonoBehaviour
{
    weka.classifiers.trees.M5P saberPredecirFuerzaX;
    weka.core.Instances casosEntrenamiento;    
    string ESTADO = "Sin conocimiento";
    string acciones;
    public GameObject pelota;
    GameObject InstanciaPelota, PuntoObjetivo;
    float distanciaObjetivo, mejorFuerzaX;
    public float valorMaximoFx, pasoFx, Fy_fijo;
    Rigidbody r;

    void OnGUI()
    {
        GUI.Label(new Rect(10, 5, 600, 20), "Estado: "+ESTADO);
        GUI.Label(new Rect(10, 20, 600, 20), acciones);
    }

    void Start()
    {
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");              //Lanza el proceso de entrenamiento                                              
    }
    IEnumerator Entrenamiento()
    {   casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Experiencias.arff"));  //Lee fichero con las variables y experiencias

        if (casosEntrenamiento.numInstances() < 10)
            for (float Fx = 1; Fx <= valorMaximoFx; Fx = Fx + pasoFx)                   //BUCLE de planificación de la fuerza FX durante el entrenamiento
            {
                InstanciaPelota = Instantiate(pelota) as GameObject;
                Rigidbody rb = InstanciaPelota.GetComponent<Rigidbody>();               //Crea una pelota física
                rb.AddForce(new Vector3(Fx, Fy_fijo, 0), ForceMode.Impulse);                 //y la lanza con esa fuerza Fx  (Fy es siempre 10N)
                yield return new WaitUntil(() => (rb.transform.position.y < 0));        //... y espera a que la pelota llegue al suelo

                Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                acciones="Generando experiencia con fuerzas Fx= " + Fx + " N   Fy= "+ Fy_fijo+" N  se alcanzó distancia= " + rb.transform.position.x+" m";
                casoAaprender.setDataset(casosEntrenamiento);                           //crea un registro de experiencia
                casoAaprender.setValue(0, Fx);                                          //guarda el dato de la fuerza utilizada
                casoAaprender.setValue(1, rb.transform.position.x);                     //anota la distancia alcanzada
                casosEntrenamiento.add(casoAaprender);                                  //guarda el registro de experiencia 
                                                                                        //----------------------------------------------------- 
                rb.isKinematic = true; rb.GetComponent<Collider>().isTrigger = true;    //...opcional: paraliza la pelota
                Destroy(InstanciaPelota, 1f);                                           //...opcional: destruye la pelota en 1 seg para que ver donde cayó.            
            }                                                                           //FIN bucle de lanzamientos con diferentes de fuerzas
        //APRENDIZADE CONOCIMIENTO:  
        saberPredecirFuerzaX = new M5P();                                               //crea un algoritmo de aprendizaje M5P (árboles de regresión)
        casosEntrenamiento.setClassIndex(0);                                            //la variable a aprender será la fuerza Fx (id=0) dada la distancia
        saberPredecirFuerzaX.buildClassifier(casosEntrenamiento);                       //REALIZA EL APRENDIZAJE DE FX A PARTIR DE LAS EXPERIENCIAS
        ESTADO = "Con conocimiento";        
    }

    void FixedUpdate()                                                                  //DURANTEL EL JUEGO: Aplica lo aprendido para lanzar a la canasta
    {
        if (ESTADO == "Con conocimiento")
        {
            distanciaObjetivo = UnityEngine.Random.Range(1.0f, 15.0f);                            //Distancia de la Canasta (... Opcional: generada aleatoriamente)

            PuntoObjetivo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);             // ... opcional: muestra la canasta a la distancia propuesta
            PuntoObjetivo.transform.position = new Vector3(distanciaObjetivo, -1, 0);
            PuntoObjetivo.transform.localScale = new Vector3(1.1f, 1, 1.1f);
            PuntoObjetivo.GetComponent<Collider>().isTrigger = true;                        //...  opcional: hace que la canasta no sea física 

            acciones = "Se situo canasta a " + distanciaObjetivo.ToString("0.000") + " m. ";

            Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());  //Crea un registro de experiencia durante el juego
            casoPrueba.setDataset(casosEntrenamiento);
            casoPrueba.setValue(1, distanciaObjetivo);                               //le pone el dato de la distancia a alcanzar

            mejorFuerzaX = (float)saberPredecirFuerzaX.classifyInstance(casoPrueba);  //predice la fuerza dada la distancia utilizando el algoritmo M5P

            InstanciaPelota = Instantiate(pelota) as GameObject;                      //Utiliza la pelota física del juego (si no existe la crea)
            r = InstanciaPelota.GetComponent<Rigidbody>();
            r.AddForce(new Vector3(mejorFuerzaX, Fy_fijo, 0), ForceMode.Impulse);          //y por fin la la lanza en el videojuego con la fuerza encontrada
            //print("Se lanzó una pelota con fuerza Fx=" + mejorFuerzaX + " y Fy= 10 N");
            ESTADO = "Jugando";
            acciones = acciones + " Se aplicó Fx= " + mejorFuerzaX.ToString("0.000") + " N  Fy= " + Fy_fijo+" N";
        }
        if (ESTADO == "Jugando")
        {
            if (r.transform.position.y < 0)                                            //cuando la pelota cae por debajo de 0 m
            {                                                                          //escribe la distancia en x alcanzada
              // print(" Fuerzas aplicadas Fx=" + mejorFuerzaX.ToString("0.000") + " Fy = 10. ";
              // print("La pelota lanzada llegó a " + r.transform.position.x + ". El error fue de " + (r.transform.position.x - distanciaObjetivo).ToString("0.000000") + " m");
              acciones = acciones +"  El error: " + (r.transform.position.x - distanciaObjetivo).ToString("0.000000") + " m";

              r.isKinematic = true;
              ESTADO = "Accion Finalizada";
            }          
        }
    }
}
