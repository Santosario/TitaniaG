using System;
using System.Collections;
using UnityEngine;

public class GeneradordeZombies : MonoBehaviour
{
    public int tiempoGeneracion = 2;
    public int limiteZombies = 20;
    private Zombie pfZombie;

    private void Awake()
    {
        pfZombie = Resources.Load<Zombie>("Zombie");
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

        if (GameManager.zombiesActivos > limiteZombies) goto Reptir;

        Instantiate(pfZombie, transform.position, transform.rotation);

        goto Reptir;
    }
}
