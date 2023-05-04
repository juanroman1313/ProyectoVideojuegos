using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuiaScript : MonoBehaviour
{
    public GameObject helicoptero;
    private NavMeshAgent agente;
    public GameObject meta;
    public enum Estado {BAJOHELICOPTERO, IRMETA}
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
                agente.destination = meta.transform.position;
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
        agente.destination = meta.transform.position;
        estado = Estado.IRMETA;
    }
}
