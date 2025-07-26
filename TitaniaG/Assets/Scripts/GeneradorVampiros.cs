using System;
using System.Collections;
using UnityEngine;

public class GeneradorVampiros : MonoBehaviour
{
    public int tiempoGeneracion = 2;
    public int limiteVampiros = 20;
    private Vampiro pfVampiro;

    private void Awake()
    {
        pfVampiro = Resources.Load<Vampiro>("Vampiro");
    }

    private void OnEnable()
    {
        StartCoroutine(Corrutina());
    }

    private IEnumerator Corrutina()
    {
    Reptir:
        yield return new WaitForSeconds(tiempoGeneracion);

        float distancia = Vector3.Distance(GameManager.Jugador.transform.position, transform.position);

        if (distancia > GameManager.distanciaOptimizacion) goto Reptir;

        if (GameManager.vampirosActivos > limiteVampiros) goto Reptir;

        Instantiate(pfVampiro, transform.position, transform.rotation);

        goto Reptir;
    }
}
