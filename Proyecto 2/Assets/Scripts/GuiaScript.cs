using System.Collections;
using System.Collections.Generic;
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
    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.enabled = false;
        estado = Estado.BAJOHELICOPTERO;
        posDestino = 0;
        destinoAct = destinos[posDestino];
        accionCaja = false;
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
        print(accionCaja);
        if(agente.remainingDistance <= agente.stoppingDistance && (posDestino == 0 || posDestino == 1))
        {
            accionCaja = true;
        }
    }
    private void BajoHelicoptero()
    {
        transform.position = new Vector3(helicoptero.transform.position.x, 0, helicoptero.transform.position.z);
    }
    public void CambiarAIrDestinos()
    {
        agente.enabled = true;
        agente.destination = destinoAct.transform.position; // Lo pongo aqu� porque me entra en el if de arriba.
        estado = Estado.IRDESTINOS;
    }
    public void SiguienteDestino()
    {
        accionCaja = false;
        posDestino++;
        destinoAct = destinos[posDestino];
        agente.destination = destinoAct.transform.position; // Lo pongo aqu� porque me entra en el if de arriba.
    }
}
