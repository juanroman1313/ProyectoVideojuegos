using java.security;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class HelicopteroScript : MonoBehaviour
{
    private enum Estado {DESPEGAR, TOMARCAJA, SEGUIRGUIA, DEJARCAJA, ESQUIVAR}
    private Estado estado;
    private const float VELVERT = 0.2f;
    private const float VELHOR = 100f;
    private const float VELGIR = 1000f;
    private const float ALTURABASE = 10;
    private float alturaDeseada;
    private Rigidbody rb;
    private float masa;
    private float fuerzaLevitacion;
    private RaycastHit[] detectSens;
    public GameObject guia;
    public GameObject engancheCadena;

    private float distanciaObstaculo;
    private float t;
    private bool alturaObt;
    private bool distMontObtenida;
    private float distMont;

    private bool enganche;
    private bool cajaTomada;
    private bool cajaSoltada;
    public bool choqueCaja;
    private float alturaObjetivoSubida;
    public GameObject[] enganches;
    public GameObject caja;
    void Start()
    {
        estado = Estado.DESPEGAR;
        rb = GetComponent<Rigidbody>();
        masa = rb.mass + 5 * 9 + 10; // Masa del helicoptero (1000 Kg) + masa imán + masa cadenas.
        fuerzaLevitacion = -(Physics.gravity.y * masa); // Fuerza de levitación del helicoptero (Fuerza necesaria para anular las fuerzas)
        detectSens = new RaycastHit[5];
        //engancheCadena.GetComponent<FixedJoint>().connectedBody = null;
        alturaDeseada = ALTURABASE;
        distanciaObstaculo = 0;
        t = 1;
        alturaObt = false;
        distMontObtenida = false;
        distMont = 0;
        enganche = true;
        cajaTomada = false;
        choqueCaja = false;
        cajaSoltada = false;
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
            case Estado.TOMARCAJA:
                TomarCaja();
                break;
            case Estado.SEGUIRGUIA:             // ESQUIVAR
                SeguirGuia();
                break;
            case Estado.DEJARCAJA:
                DejarCaja();
                break;
            case Estado.ESQUIVAR:               // SEGUIRGUIA
                Esquivar();
                break;
        }
    }
    private void Despegar()
    {
        if(transform.position.y >= alturaDeseada - 1)
        {
            estado = Estado.SEGUIRGUIA;
            guia.GetComponent<GuiaScript>().CambiarAIrDestinos();
        }
    }
    // Método para tomar caja. Si el imán choca con la caja, empezamos la subida. Si alcanzamos la altura base, seguimos a la guia.
    private void TomarCaja()
    {
        AlcanzarAltura(alturaDeseada, VELVERT);
        AlcanzarPosicion(guia, VELHOR);
        if (choqueCaja && !cajaTomada)
        {
            StartCoroutine(Subir(ALTURABASE));
            masa += 1;
            cajaTomada = true;
        }
        if(cajaTomada && alturaDeseada >= ALTURABASE)
        {
            guia.GetComponent<GuiaScript>().SiguienteDestino();
            estado = Estado.SEGUIRGUIA;
        }
    }
    private void DejarCaja()
    {
        AlcanzarAltura(alturaDeseada, VELVERT);
        AlcanzarPosicion(guia, VELHOR);
        if (choqueCaja && !cajaSoltada)
        {
            StartCoroutine(Subir(alturaObjetivoSubida));
            masa -= 1;
            cajaSoltada = true;
            Destroy(caja.GetComponent<FixedJoint>()); // Destruimos el fixed joint de la caja.
        }
        if (cajaSoltada && alturaDeseada >= alturaObjetivoSubida)
        {
            foreach (GameObject g in enganches) // Destruimos el enganche para que no se bugee;
            {
                Destroy(g);
            }
            masa -= (5 * 9 + 10);
            enganche = false;
            guia.GetComponent<GuiaScript>().SiguienteDestino();
            estado = Estado.SEGUIRGUIA;
        }
    }
    // Método para hacer que el helicóptero alcance una altura determinada, manejando la variable alturaDeseada.
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
        bool d1;
        if (enganche)
        {
            d1 = Physics.Raycast(transform.position - transform.up * 5f, -Vector3.up, out detectSens[0]);
            Debug.DrawRay(transform.position - transform.up * 3f, -Vector3.up * 10, Color.red);
        }
        else
        {
            d1 = Physics.Raycast(transform.position - transform.up * 0.5f, -Vector3.up, out detectSens[0]);
            Debug.DrawRay(transform.position - transform.up * 0.5f, -Vector3.up * 10, Color.red);
        }
        bool d2 = Physics.Raycast(transform.position + transform.forward * 2.5f, new Vector3(transform.forward.x, 0, transform.forward.z), out detectSens[1]);
        Debug.DrawRay(transform.position + transform.forward * 2.5f, new Vector3(transform.forward.x, 0, transform.forward.z) * 10, Color.red);
        bool d3 = Physics.Raycast(transform.position + transform.right * 2f, new Vector3(transform.right.x, 0, transform.right.z), out detectSens[2]);
        Debug.DrawRay(transform.position + transform.right * 2f, new Vector3(transform.right.x, 0, transform.right.z) * 10, Color.red);
        bool d4 = Physics.Raycast(transform.position - transform.forward * 4.5f, -new Vector3(transform.forward.x, 0, transform.forward.z), out detectSens[3]);
        Debug.DrawRay(transform.position - transform.forward * 4.5f, -new Vector3(transform.forward.x, 0, transform.forward.z) * 10, Color.red);
        bool d5 = Physics.Raycast(transform.position - transform.right * 2f, -new Vector3(transform.right.x, 0, transform.right.z), out detectSens[4]);
        Debug.DrawRay(transform.position - transform.right * 2f, -new Vector3(transform.right.x, 0, transform.right.z) * 10, Color.red);
        return d1 || d2 || d3 || d4 || d5;
    }
    // Corutina para bajar hasta tomar la caja.
    private IEnumerator Bajar()
    {
        print(choqueCaja);
        while (!choqueCaja)
        {
            print("Bajando...");
            alturaDeseada -= 0.1f;
            yield return new WaitForSeconds(0.3f);
        }
        choqueCaja = false;
    }
    // Corutina para subir lentamente.
    private IEnumerator Subir(float alturaObjetivo)
    {
        while (alturaDeseada < alturaObjetivo)
        {
            print("Subiendo...");
            alturaDeseada += 0.1f;
            yield return new WaitForSeconds(0.3f);
        }
    }
    private void SeguirGuia()
    {
        // Condición para empezar a bajar y tomar la caja.
        print("Distancia a guia: " + Vector3.Distance(new Vector3(transform.position.x, guia.transform.position.y, transform.position.z), guia.transform.position));
        if (!cajaTomada
            && Vector3.Distance(new Vector3(transform.position.x, guia.transform.position.y, transform.position.z), guia.transform.position) <= 0.005f
            && guia.GetComponent<GuiaScript>().accionCaja)
        {
            StartCoroutine("Bajar");
            estado = Estado.TOMARCAJA;
            return;
        }
        if(cajaTomada && !cajaSoltada
            && Vector3.Distance(new Vector3(transform.position.x, guia.transform.position.y, transform.position.z), guia.transform.position) <= 0.005f
            && guia.GetComponent<GuiaScript>().accionCaja)
        {
            alturaObjetivoSubida = alturaDeseada;
            StartCoroutine("Bajar");
            estado = Estado.DEJARCAJA;
            return;
        }
        // Si detectamos algún obstáculo lateral, entramos en el if y cambiamos de estado.
        if (ObstaculoLateralDetectado())
        {
            //print("EDIFICIO DETECTADO.");
            estado = Estado.ESQUIVAR;
            float velocidad = rb.velocity.magnitude;
            StartCoroutine(AumentarAltura(distanciaObstaculo, velocidad)); 
            return;
        }
        // Si pasa algún tiempo sin obstáculos abajo, bajamos a la altura base.
        if(alturaDeseada != ALTURABASE && Time.time > t + 3 && !EdificioAbajo() && !MontanaAbajo())
        {
            alturaDeseada = ALTURABASE;
        }
        // Si hay un edificio abajo, nos quedamos a la misma altura.
        if (EdificioAbajo())
        {
            //print("edificio abajo.");
            if (!alturaObt)
            {
                alturaDeseada = transform.position.y;
                alturaObt = true;
            }
            t = Time.time;
        }
        else
        {
            alturaObt = false;
        }
        // Si hay una montaña abajo, obtenemos la primera distancia a la montaña y la mantenemos en todo momento.
        if (MontanaAbajo())
        {
            if (!distMontObtenida)
            {
                if (enganche)
                {
                    distMont = detectSens[0].distance + 10f;
                }
                else
                {
                    distMont = detectSens[0].distance;
                }
                distMontObtenida = true;
            }
            alturaDeseada = detectSens[0].point.y + distMont;
        }
        else
        {
            distMontObtenida = false;
        }
        AlcanzarPosicion(guia, VELHOR);

    }
    private void Esquivar()
    {
        // Si dejamos de detectar el obstáculo lateral, significa que estamos a más altura. Por lo tanto, cambiamos a SEGUIRGUIA.
        if (!ObstaculoLateralDetectado())
        {
            estado = Estado.SEGUIRGUIA;
            t = Time.time;
            return;
        }
        AlcanzarAltura(alturaDeseada, VELVERT);
    }
    // Corutina para aumentar altura al detectar obstáculo lateral.
    IEnumerator AumentarAltura(float distancia, float velocidad)
    {
        while (ObstaculoLateralDetectado())
        {
            //print("SUBIENDO...");
            alturaDeseada += 1f;
            yield return new WaitForSeconds((distancia/velocidad) * 0.1f); // He intentado dependiendo de la velocidad contra
                                                                           // el edificio y distancia, subir la altura con más
                                                                           // o menos velocidad.
        }
    }
    // Función para detectar obstáculos laterales.
    private bool ObstaculoLateralDetectado()
    {
        bool obstaculo = false;
        int pos = 1;
        if (Sensores())
        {
            while (!obstaculo && pos < detectSens.Length)
            {
                if (detectSens[pos].collider != null 
                    && detectSens[pos].collider.gameObject.CompareTag("edificio") // Si el obstáculo es un edificio.
                    && detectSens[pos].distance < rb.velocity.magnitude) // Si la distancia es menor que la velocidad a la que vamos.
                {
                    // Obtenemos el vector dirección de el helicoptero con el punto de choque del sensor.
                    Vector3 vDirEdificio = detectSens[pos].point - transform.position;
                    // Suponemos que si el ángulo entre el vector anterior y el vector velocidad es menor de 90 grados, estamos yendo
                    // en dirección al edificio.
                    if(Vector3.Angle(vDirEdificio, rb.velocity) < 90)
                    {
                        distanciaObstaculo = detectSens[pos].distance;
                        obstaculo = true; // Por lo tanto, es un obstáculo.
                    }
                }
                pos ++;
            }
        }
        return obstaculo;
    }
    // Función para detectar si hay un edificio abajo.
    private bool EdificioAbajo()
    {
        bool obstaculo = false;
        if(Sensores())
        {
            if (detectSens[0].collider.gameObject.CompareTag("edificio"))
            {
                obstaculo = true;
            }
        }
        return obstaculo;
    }
    // Función para detectar si hay una montaña abajo.
    private bool MontanaAbajo()
    {
        bool obstaculo = false;
        if (Sensores())
        {
            if (detectSens[0].collider.gameObject.CompareTag("montana"))
            {
                obstaculo = true;
            }
        }
        return obstaculo;
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
                                                                   // menor a 3, mantenemos posición.
        {
            float factor = vectorDireccionObjetivo.magnitude * velocidadHorizontal;
            rb.AddForce(vectorDireccionObjetivo * factor);
            NoGirar(VELGIR); // frenamos el giro.
            //print("Mantener posición.");
        }
        else if(Vector3.Distance(transform.position, posObj) < rb.velocity.magnitude
                || (velocidadGuia > 0 && rb.velocity.magnitude > velocidadGuia)) // Frenamos en caso de que vayamos más rapido
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
        // Calculamos el producto escalar del vector3.up y el vector dirección hacia el objetivo.
        // Obtenemos el positivo y el negativo.
        Vector3 perpYObjPos = Vector3.Cross(Vector3.up, vectorDir);
        Vector3 perpYObjNeg = - Vector3.Cross(Vector3.up, vectorDir);
        // Calculamos para cada vector, el ángulo con el transform.forward del helicoptero.
        float anguloPos = Vector3.Angle(perpYObjPos, transform.forward);
        float anguloNeg = Vector3.Angle(perpYObjNeg, transform.forward);
        Vector3 ejeGiro = Vector3.up; // Eje en el que vamos a girar.
        if(anguloPos < anguloNeg){ // Si el ángulo con el vector positivo es menor, giramos hacia la izquierda, si no, hacia la derecha.
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
}
