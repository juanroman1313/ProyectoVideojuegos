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
                agente.speed = meta.GetComponent<Rigidbody>().velocity.magnitude;
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
}
