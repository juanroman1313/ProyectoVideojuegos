using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CocheScript : MonoBehaviour
{
    public Transform modeloRuedaIzquierda, modeloRuedaFrontalDerecha;
    float fuerzaFrontal;
    public GameObject guia;
    public float intencionAvazar = 1, intencionGirar = 0; //0 es sin intención, 1 máx velocidad
    bool colisionandoSuelo = false;
    float maxAnguloGiroVolante = 40; Rigidbody rb;
    float anguloObjeto;
    float distancia;
    public enum Estado {AVANZA,GIRAIZQ,GIRADER};
    public Estado estado;
    Vector3 vectorHaciaObjeto;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fuerzaFrontal = 100 * rb.mass * (-Physics.gravity.y) * 0.8f; //debe ser >= 2 * masa * gravedad * coeficienteFricciónEq;
        guia.GetComponent<GuiaCocheScript>().CambiarAIrMeta();
        estado = Estado.AVANZA;
    }
    void FixedUpdate()
    {
        vectorHaciaObjeto = guia.transform.position - transform.position;
        anguloObjeto = Vector3.SignedAngle(transform.forward, new Vector3(vectorHaciaObjeto.x, 0, vectorHaciaObjeto.z) , Vector3.up);
        distancia = Vector3.Distance(transform.position, guia.transform.position);
        switch (estado)
        {
            case Estado.AVANZA:
                Avanza();
                break;
            case Estado.GIRAIZQ:
                GirarIzq();
                break;
            case Estado.GIRADER:
                GirarDer();
                break;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "suelo" || other.gameObject.tag == "montana")
            colisionandoSuelo = true;
    }
    private void Avanza()
    {
        print("Angulo: " + anguloObjeto);
        print("Distancia Coche: " + distancia);
        if (distancia > 10)
        {
            modeloRuedaIzquierda.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaIzquierda.right * distancia * fuerzaFrontal);
            modeloRuedaFrontalDerecha.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaFrontalDerecha.right * distancia * fuerzaFrontal);
        }
        else
        {
            modeloRuedaIzquierda.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaIzquierda.right * distancia * fuerzaFrontal*-1);
            modeloRuedaFrontalDerecha.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaFrontalDerecha.right * distancia * fuerzaFrontal*-1);
        }
       
        if(anguloObjeto > 10)
        {
            estado = Estado.GIRADER;
            return;
        }
        if(anguloObjeto < -10)
        {
            estado = Estado.GIRAIZQ;
            return;
        }
        if (Vector3.Distance(transform.position, guia.transform.position) < 8)
        {
            guia.GetComponent<GuiaCocheScript>().CambiarDestino();
        }
    }
    private void GirarIzq()
    {
        print("Angulo: " + anguloObjeto);
        print("Distancia Coche: " + distancia);
        modeloRuedaIzquierda.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaIzquierda.right * distancia * fuerzaFrontal*anguloObjeto);
        modeloRuedaFrontalDerecha.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaFrontalDerecha.right * distancia * fuerzaFrontal*100000);
        if(anguloObjeto >= -5)
        {
            estado = Estado.AVANZA;
            return;
        }
    }
    private void GirarDer()
    {
        print("Angulo: " + anguloObjeto);
        print("Distancia Coche: " + distancia);
        modeloRuedaIzquierda.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaIzquierda.right * distancia * fuerzaFrontal*100000);
        modeloRuedaFrontalDerecha.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaFrontalDerecha.right * distancia * fuerzaFrontal * -1 *anguloObjeto);
        if (anguloObjeto <= 5)
        {
            estado = Estado.AVANZA;
            return;
        }
    }
    /*
    private void AlcanzarPosicion(GameObject objeto,float velocidadHorizontal)
    {
        vectorHaciaObjeto = objeto.transform.position - transform.position;
        anguloObjeto = Vector3.SignedAngle(transform.forward, vectorHaciaObjeto, Vector3.up);
        distancia = Vector3.Distance(transform.position, objeto.transform.position);
        print("Angulo: " + anguloObjeto);
        print("Distancia Coche: " + distancia);
        if(anguloObjeto>=-10 && anguloObjeto <= 10)
        {
            modeloRuedaIzquierda.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaIzquierda.right * distancia * velocidadHorizontal);
            modeloRuedaFrontalDerecha.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaFrontalDerecha.right * distancia * velocidadHorizontal);
        }else if (anguloObjeto > 3)
        {
            modeloRuedaIzquierda.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaIzquierda.right * distancia * velocidadHorizontal*anguloObjeto);
            modeloRuedaFrontalDerecha.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaFrontalDerecha.right * distancia * velocidadHorizontal*anguloObjeto *-1);
        }else if (anguloObjeto < -3)
        {
            modeloRuedaIzquierda.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaIzquierda.right * distancia * velocidadHorizontal * anguloObjeto);
            modeloRuedaFrontalDerecha.gameObject.GetComponent<Rigidbody>().AddRelativeTorque(modeloRuedaFrontalDerecha.right * distancia * velocidadHorizontal * anguloObjeto *-1);
        }
    }
    */
}
