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
using weka.core.converters; 

public class AprendizLento_2_incognitas : MonoBehaviour
{
    weka.classifiers.trees.M5P saberPredecirDistancia, saberPredecirFuerzaX;
    weka.core.Instances casosEntrenamiento;
    Text texto;
    private string ESTADO = "Sin conocimiento";
    public GameObject pelota;
    GameObject InstanciaPelota, PuntoObjetivo;
    public float valorMaximoFx, valorMaximoFy, paso, Velocidad_Simulacion=1;
    float mejorFuerzaX, mejorFuerzaY, distanciaObjetivo;
    Rigidbody r;
 
    void Start()
    {
        Time.timeScale = Velocidad_Simulacion;                                          //...opcional: hace que se vea más rápido (recomendable hasta 5)
        texto = Canvas.FindObjectOfType<Text>();
        if (ESTADO == "Sin conocimiento") StartCoroutine("Entrenamiento");              //Lanza el proceso de entrenamiento                                                                                    
    }

    IEnumerator Entrenamiento()
    {

        //Uso de una tabla vacía:
        casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Iniciales_Experiencias.arff"));  //Lee fichero con variables. Sin instancias
        
        //Uso de una tabla con los datos del último entrenamiento:
        //casosEntrenamiento = new weka.core.Instances(new java.io.FileReader("Assets/Finales_Experiencias.arff"));    //... u otro con muchas experiencias

        if (casosEntrenamiento.numInstances() < 10)
        {
            texto.text = "ENTRENAMIENTO: crea una tabla con las fuerzas Fx y Fy utilizadas y las distancias alcanzadas.";
            print("Datos de entrada: valorMaximoFx=" + valorMaximoFx + " valorMaximoFy="+ valorMaximoFy + "  " + ((valorMaximoFx == 0 || valorMaximoFy == 0) ? " ERROR: alguna fuerza es siempre 0" : ""));
            for (float Fx = 1; Fx <= valorMaximoFx; Fx = Fx +  paso)                      //Bucle de planificación de la fuerza FX durante el entrenamiento
            {
                for (float Fy = 1; Fy <= valorMaximoFy; Fy = Fy + paso)                    //Bucle de planificación de la fuerza FY durante el entrenamiento
                {
                    InstanciaPelota = Instantiate(pelota) as GameObject;
                    Rigidbody rb = InstanciaPelota.GetComponent<Rigidbody>();              //Crea una pelota física
                    rb.AddForce(new Vector3(Fx, Fy, 0), ForceMode.Impulse);                //y la lanza con las fuerza Fx y Fy
                    yield return new WaitUntil(() => (rb.transform.position.y < 0));       //... y espera a que la pelota llegue al suelo

                    Instance casoAaprender = new Instance(casosEntrenamiento.numAttributes());
                    print("ENTRENAMIENTO: con fuerza Fx " + Fx + " y Fy=" + Fy + " se alcanzó una distancia de " + rb.transform.position.x + " m");
                    casoAaprender.setDataset(casosEntrenamiento);                          //crea un registro de experiencia
                    casoAaprender.setValue(0, Fx);                                         //guarda los datos de las fuerzas Fx y Fy utilizadas
                    casoAaprender.setValue(1, Fy);
                    casoAaprender.setValue(2, rb.transform.position.x);                    //anota la distancia alcanzada
                    casosEntrenamiento.add(casoAaprender);                                 //guarda el registro en la lista casosEntrenamiento
                    rb.isKinematic = true; rb.GetComponent<Collider>().isTrigger = true;   //...opcional: paraliza la pelota
                    Destroy(InstanciaPelota,1);                                              //...opcional: destruye la pelota
                }                                                                          //FIN bucle de lanzamientos con diferentes de fuerzas
            }


            File salida = new File("Assets/Finales_Experiencias.arff");
            if (!salida.exists())
                System.IO.File.Create(salida.getAbsoluteFile().toString()).Dispose();
            ArffSaver saver = new ArffSaver();
            saver.setInstances(casosEntrenamiento);
            saver.setFile(salida);
            saver.writeBatch();
        }

        //APRENDIZAJE CONOCIMIENTO:  
        saberPredecirFuerzaX = new M5P();                                                //crea un algoritmo de aprendizaje M5P (árboles de regresión)
        casosEntrenamiento.setClassIndex(0);                                             //y hace que aprenda Fx dada la distancia y Fy
        saberPredecirFuerzaX.buildClassifier(casosEntrenamiento);                        //REALIZA EL APRENDIZAJE DE FX A PARTIR DE LA DISTANCIA Y FY

        saberPredecirDistancia = new M5P();                                              //crea otro algoritmo de aprendizaje M5P (árboles de regresión)  
        casosEntrenamiento.setClassIndex(2);                                             //La variable a aprender a calcular la distancia dada Fx e FY                                                                                         
        saberPredecirDistancia.buildClassifier(casosEntrenamiento);                      //este algoritmo aprende un "modelo fisico aproximado"

        ESTADO = "Con conocimiento";

        print(casosEntrenamiento.numInstances() +" espers "+ saberPredecirDistancia.toString());

        //EVALUACION DEL CONOCIMIENTO APRENDIDO: 
        if (casosEntrenamiento.numInstances() >= 10){
            casosEntrenamiento.setClassIndex(0);
            Evaluation evaluador = new Evaluation(casosEntrenamiento);                   //...Opcional: si tien mas de 10 ejemplo, estima la posible precisión
            evaluador.crossValidateModel(saberPredecirFuerzaX, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Fx durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + " N");
            casosEntrenamiento.setClassIndex(2);
            evaluador.crossValidateModel(saberPredecirDistancia, casosEntrenamiento, 10, new java.util.Random(1));
            print("El Error Absoluto Promedio con Distancias durante el entrenamiento fue de " + evaluador.meanAbsoluteError().ToString("0.000000") + " m");
        }

        //PRUEBA: Estimación de la distancia a la Canasta
        //distanciaObjetivo = leer_Distancia_de_la_canasta...  //...habría que implementar un metodo para leer la distancia objetivo;    

        //... o generacion aleatoria de una distancia dependiendo de sus límites:        
        AttributeStats estadisticasDistancia = casosEntrenamiento.attributeStats(2);        //Opcional: Inicializa las estadisticas de las distancias
        float maximaDistanciaAlcanzada = (float) estadisticasDistancia.numericStats.max;    //Opcional: Obtiene el valor máximo de las distancias alcanzadas
        distanciaObjetivo = UnityEngine.Random.Range(maximaDistanciaAlcanzada * 0.2f, maximaDistanciaAlcanzada * 0.8f);  //Opcional: calculo aleatorio de la distancia 

        /////////////////    SITUA LA CANASTA EN LA "distanciaObjetivo"  ESTIMADA   ///////////////////
        PuntoObjetivo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        PuntoObjetivo.transform.position = new Vector3(distanciaObjetivo, -1, 0);
        PuntoObjetivo.transform.localScale = new Vector3(1.1f, 1, 1.1f);
        PuntoObjetivo.GetComponent<Collider>().isTrigger = true;

        /////////////////////////////////////////////////////////////////////////////////////////////

    }
    void FixedUpdate()                                                                                 //DURANTEL EL JUEGO: Aplica lo aprendido para lanzar a la canasta
    {
        if ((ESTADO == "Con conocimiento") && (distanciaObjetivo > 0))
        {
            Time.timeScale = 1;                                                                               //Durante el juego, el NPC razona así... (no juega aún)   
            float menorDistancia = 1e9f;
            print("-- OBJETIVO: LANZAR LA PELOTA A UNA DISTANCIA DE " + distanciaObjetivo + " m.");
       
            //Si usa dos bucles Fx y Fy con "modelo fisico aproximado", complejidad n^2
            //Reduce la complejidad con un solo bucle FOR, así

            for (float Fy = 1; Fy < valorMaximoFy; Fy = Fy + paso)                                            //Bucle FOR con fuerza Fy, deduce Fx = f (Fy, distancia) y escoge mejor combinacion         
            {
                Instance casoPrueba = new Instance(casosEntrenamiento.numAttributes());
                casoPrueba.setDataset(casosEntrenamiento);
                casoPrueba.setValue(1, Fy);                                                                   //crea un registro con una Fy
                casoPrueba.setValue(2, distanciaObjetivo);                                                    //y la distancia
                float Fx = (float)saberPredecirFuerzaX.classifyInstance(casoPrueba);                          //Predice Fx a partir de la distancia y una Fy 
                if ((Fx >= 1) && (Fx <= valorMaximoFx))
                {
                    Instance casoPrueba2 = new Instance(casosEntrenamiento.numAttributes());
                    casoPrueba2.setDataset(casosEntrenamiento);                                                  //Utiliza el "modelo fisico aproximado" con Fx y Fy                 
                    casoPrueba2.setValue(0, Fx);                                                                 //Crea una registro con una Fx
                    casoPrueba2.setValue(1, Fy);                                                                 //Crea una registro con una Fy
                    float prediccionDistancia = (float)saberPredecirDistancia.classifyInstance(casoPrueba2);     //Predice la distancia dada Fx y Fy
                    if (Mathf.Abs(prediccionDistancia - distanciaObjetivo) < menorDistancia)                     //Busca la Fy con una distancia más cercana al objetivo
                    {
                        menorDistancia = Mathf.Abs(prediccionDistancia - distanciaObjetivo);                     //si encuentra una buena toma nota de esta distancia
                        mejorFuerzaX = Fx;                                                                       //de la fuerzas que uso, Fx
                        mejorFuerzaY = Fy;                                                                       //tambien Fy
                        print("RAZONAMIENTO: Una posible acción es ejercer una fuerza Fx=" + mejorFuerzaX + " y Fy= " + mejorFuerzaY + " se alcanzaría una distancia de " + prediccionDistancia);
                    }
                }
            }                                                                                                     //FIN DEL RAZONAMIENTO PREVIO
            if ((mejorFuerzaX == 0) && (mejorFuerzaY == 0)) { 
                texto.text = "NO SE LANZÓ LA PELOTA: La distancia de "+distanciaObjetivo+" m no se ha alcanzado muchas veces.";
                print(texto.text);
            }
            else
            {
                InstanciaPelota = Instantiate(pelota) as GameObject;
                r = InstanciaPelota.GetComponent<Rigidbody>();                                                        //EN EL JUEGO: utiliza la pelota física del juego (si no existe la crea)
                r.AddForce(new Vector3(mejorFuerzaX, mejorFuerzaY, 0), ForceMode.Impulse);                            //la lanza en el videojuego con la fuerza encontrada
                print("DECISION REALIZADA: Se lanzó pelota con fuerza Fx =" + mejorFuerzaX + " y Fy= " + mejorFuerzaY);
                ESTADO = "Acción realizada";
            }
         }
        if (ESTADO == "Acción realizada")
        {
            texto.text = "Para una canasta a " + distanciaObjetivo.ToString("0.000") + " m, las fuerzas Fx y Fy a utilizar será: " + mejorFuerzaX.ToString("0.000") + "N y " + mejorFuerzaY.ToString("0.000") + "N, respectivamente";
            if (r.transform.position.y < 0)                                            //cuando la pelota cae por debajo de 0 m
            {                                                                          //escribe la distancia en x alcanzada
                print("La canasta está a una distancia de " + distanciaObjetivo + " m");
                print("La pelota lanzada llegó a " + r.transform.position.x + ". El error fue de " + (r.transform.position.x - distanciaObjetivo).ToString("0.000000") + " m");
                r.isKinematic = true;
                ESTADO = "FIN";
            }
        }
    }
}
