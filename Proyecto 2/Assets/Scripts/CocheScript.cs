using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CocheScript : MonoBehaviour
{
    public Transform modeloRuedaIzquierda, modeloRuedaFrontalDerecha;
    float fuerzaFrontal;
    public GameObject guia;
    Rigidbody rb;
    float anguloObjeto;
    float distancia;
    public enum Estado {AVANZA,GIRAIZQ,GIRADER};
    public Estado estado;
    Vector3 vectorHaciaObjeto;
    bool destinoAlcanzado;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        fuerzaFrontal = 1000000000 * rb.mass * (-Physics.gravity.y) * 0.8f; //debe ser >= 2 * masa * gravedad * coeficienteFricciónEq;
        guia.GetComponent<GuiaCocheScript>().CambiarAIrMeta();
        estado = Estado.AVANZA;
        destinoAlcanzado = false;
    }
    void FixedUpdate()
    {
        vectorHaciaObjeto = guia.transform.position - transform.position;
        anguloObjeto = Vector3.SignedAngle(new Vector3(transform.forward.x,0,transform.forward.z), new Vector3(vectorHaciaObjeto.x,0, vectorHaciaObjeto.z), Vector3.up);
        distancia = Vector3.Distance(transform.position, guia.transform.position);
        if (Vector3.Distance(transform.position, guia.transform.position) < 8 && !destinoAlcanzado)
        {
            guia.GetComponent<GuiaCocheScript>().CambiarDestino();
            destinoAlcanzado = true;
        }
        if (distancia > 8) destinoAlcanzado = false;
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
    private void Avanza()
    {
        if (distancia > rb.velocity.magnitude) 
        {
            modeloRuedaIzquierda.gameObject.GetComponent<Rigidbody>().AddTorque(modeloRuedaIzquierda.right * distancia * fuerzaFrontal);
            modeloRuedaFrontalDerecha.gameObject.GetComponent<Rigidbody>().AddTorque(modeloRuedaFrontalDerecha.right * distancia * fuerzaFrontal);
        }
        else
        {
            modeloRuedaIzquierda.gameObject.GetComponent<Rigidbody>().AddTorque(modeloRuedaIzquierda.right * distancia * fuerzaFrontal*-1);
            modeloRuedaFrontalDerecha.gameObject.GetComponent<Rigidbody>().AddTorque(modeloRuedaFrontalDerecha.right * distancia * fuerzaFrontal*-1);
        }
       
        if(anguloObjeto > 5)
        {
            estado = Estado.GIRADER;
            return;
        }
        if(anguloObjeto < -5)
        {
            estado = Estado.GIRAIZQ;
            return;
        }

    }
    private void GirarIzq()
    {
        //print("Angulo: " + anguloObjeto);
        //print("Distancia Coche: " + distancia);
        modeloRuedaIzquierda.gameObject.GetComponent<Rigidbody>().AddTorque(modeloRuedaIzquierda.right * distancia * fuerzaFrontal*anguloObjeto);
        modeloRuedaFrontalDerecha.gameObject.GetComponent<Rigidbody>().AddTorque(modeloRuedaFrontalDerecha.right * distancia * fuerzaFrontal);
        if(anguloObjeto >= -2)
        {
            estado = Estado.AVANZA;
            return;
        }
    }
    private void GirarDer()
    {
        //print("Angulo: " + anguloObjeto);
        //print("Distancia Coche: " + distancia);
        modeloRuedaIzquierda.gameObject.GetComponent<Rigidbody>().AddTorque(modeloRuedaIzquierda.right * distancia * fuerzaFrontal);
        modeloRuedaFrontalDerecha.gameObject.GetComponent<Rigidbody>().AddTorque(modeloRuedaFrontalDerecha.right * distancia * fuerzaFrontal * -1 *anguloObjeto);
        if (anguloObjeto <= 2)
        {
            estado = Estado.AVANZA;
            return;
        }
    }
}
