using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuiaCocheScript : MonoBehaviour
{
    public GameObject coche;
    private NavMeshAgent agente;
    public GameObject[] puntos;
    private int puntoActual;
    public enum Estado { BAJOCOCHE, IRMETA }
    public Estado estado;
    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.enabled = false;
        estado = Estado.BAJOCOCHE;
        puntoActual = 0;
    }

    void Update()
    {
        //print("Estado guia: " + estado);
        switch (estado)
        {
            case Estado.BAJOCOCHE:
                BajoHelicoptero();
                break;
            case Estado.IRMETA:
                break;
        }
    }
    private void BajoHelicoptero()
    {
        transform.position = new Vector3(puntos[0].transform.position.x, 0, puntos[0].transform.position.z);
    }
    public void CambiarAIrMeta()
    {
        agente.enabled = true;
        agente.destination = puntos[puntoActual].transform.position;
        estado = Estado.IRMETA;
    }
    public void CambiarDestino()
    {
        if (agente.remainingDistance <= agente.stoppingDistance)
        {
            puntoActual++;
            if (puntoActual == 4) puntoActual = 0;
            agente.SetDestination(puntos[puntoActual].transform.position);
        }
    }
}
