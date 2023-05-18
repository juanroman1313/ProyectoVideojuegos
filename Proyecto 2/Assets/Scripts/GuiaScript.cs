using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class GuiaScript : MonoBehaviour
{
    public GameObject helicoptero;
    private NavMeshAgent agente;
    public GameObject[] destinos;
    private int posDestino;
    private GameObject destinoAct;
    public enum Estado {BAJOHELICOPTERO, IRDESTINOS}
    public Estado estado;
    public bool accionCaja;
    public bool ataque;
    public GameObject coche;
    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.enabled = false;
        estado = Estado.BAJOHELICOPTERO;
        posDestino = 0;
        destinoAct = destinos[posDestino];
        accionCaja = false;
        ataque = false;
    }

    void Update()
    {
        //print("Estado guia: " + estado);
        switch (estado)
        {
            case Estado.BAJOHELICOPTERO:
                BajoHelicoptero();
                break;
            case Estado.IRDESTINOS:
                IrDestinos();
                // agente.destination = new Vector3(destinoAct.transform.position.x, 0, destinoAct.transform.position.z); // para seguir al coche.
                break;
        }
    }
    private void IrDestinos()
    {
        // agente.destination = destinoAct.transform.position;
        //print(accionCaja);
        if(agente.remainingDistance <= agente.stoppingDistance && (posDestino == 0 || posDestino == 1))
        {
            accionCaja = true;
        }
        if(agente.remainingDistance <= agente.stoppingDistance && posDestino == 2)
        {
            ataque = true;
        }
        if(posDestino == 3)
        {
            GetComponent<NavMeshAgent>().speed = destinoAct.GetComponent<Rigidbody>().velocity.magnitude;
            agente.destination = new Vector3(destinoAct.transform.position.x, 5, destinoAct.transform.position.z); // para seguir al coche.
            if (Vector3.Distance(transform.position, coche.transform.position) <= 5) helicoptero.GetComponent<HelicopteroScript>().cercaCoche = true;
        }
    }
    private void BajoHelicoptero()
    {
        transform.position = new Vector3(helicoptero.transform.position.x, 0, helicoptero.transform.position.z);
    }
    public void CambiarAIrDestinos()
    {
        agente.enabled = true;
        agente.destination = destinoAct.transform.position; // Lo pongo aquí porque me entra en el if de arriba.
        estado = Estado.IRDESTINOS;
    }
    public void SiguienteDestino()
    {
        accionCaja = false;
        posDestino++;
        if (posDestino == 4) posDestino = 0;
        destinoAct = destinos[posDestino];
        agente.destination = destinoAct.transform.position; // Lo pongo aquí porque me entra en el if de arriba.
    }
}
