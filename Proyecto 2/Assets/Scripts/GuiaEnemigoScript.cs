using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuiaEnemigoScript : MonoBehaviour
{
    public GameObject helicoptero;
    private NavMeshAgent agente;
    public enum Estado { BAJOHELICOPTERO, IRMETA }
    public Estado estado;
    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.enabled = false;
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
        agente.destination = GenerarPosicionAleatoria();
        estado = Estado.IRMETA;
    }
    private Vector3 GenerarPosicionAleatoria()
    {
        Vector3 posicionAleatoria;
        float posicionAleatoriaX = Random.Range(transform.position.x - 30, transform.position.x + 30);
        if (posicionAleatoriaX > 50) posicionAleatoriaX = 50f;
        if (posicionAleatoriaX < -50) posicionAleatoriaX = -50f;
        float posicionAleatoriaZ = Random.Range(transform.position.z - 30, transform.position.z + 30);
        if (posicionAleatoriaZ > 50) posicionAleatoriaZ = 50f;
        if (posicionAleatoriaZ < -50) posicionAleatoriaZ = -50f;
        posicionAleatoria = new Vector3(posicionAleatoriaX, transform.position.y, posicionAleatoriaZ);
        return posicionAleatoria;
    }
    public void CambiarDestino()
    {
        if (agente.remainingDistance <= agente.stoppingDistance)
        {
            agente.SetDestination(GenerarPosicionAleatoria());
        }
    }
}
