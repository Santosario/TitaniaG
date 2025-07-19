using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region CORE

    public static GameManager self;

    private void Awake()
    {
        self = this;
        Awake_Zombies();
    }

    #endregion CORE

    #region ZOMBIES
    public static float distanciaOptimizacion = 60;
    public static List<Zombie> listaZombies = new List<Zombie>();
    public static int zombiesActivos = 0;

    public void Awake_Zombies()
    {
        StartCoroutine(CrRevisarZombies());
    }

    public IEnumerator CrRevisarZombies()
    {
        Repetir:
        foreach (Zombie zombie in listaZombies)
        {
            float distancia = Vector3.Distance(Jugador.transform.position, zombie.transform.position);
            print(distancia);
            zombie.gameObject.SetActive(distancia < distanciaOptimizacion);
        }
        
        yield return new WaitForSeconds(1);
        goto Repetir;
    }

    #endregion ZOMBIES

    [SerializeField] private Jugador _jugador;

    public static Jugador Jugador => self._jugador;

}
