using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EntrenamientoGuia : MonoBehaviour
{
    public GameObject helicoptero;
    private NavMeshAgent agente;
    public GameObject meta;
    public enum Estado { BAJOHELICOPTERO, IRMETA }
    public Estado estado;
    private float velocidadAgente;
    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.enabled = false;
        velocidadAgente = meta.GetComponent<Rigidbody>().velocity.magnitude;
        estado = Estado.BAJOHELICOPTERO;
    }

    void Update()
    {
        print("Estado guia: " + estado);
        switch (estado)
        {
            case Estado.BAJOHELICOPTERO:
                BajoHelicoptero();
                break;
            case Estado.IRMETA:
                if (Vector3.Distance(transform.position, meta.transform.position) < 10)
                {
                    velocidadAgente = 4;
                }
                agente.speed = velocidadAgente;
                agente.destination = new Vector3(meta.transform.position.x, 0, meta.transform.position.z);
                break;
        }
    }
    private void BajoHelicoptero()
    {
        transform.position = new Vector3(helicoptero.transform.position.x, 0, helicoptero.transform.position.z);
    }
    public void CambiarAIrMeta()
    {
        agente.enabled = true;
        estado = Estado.IRMETA;
    }
    public void CambiarVelocidad(float velocidad)
    {
        velocidadAgente = velocidad;
    }
}
