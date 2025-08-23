using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // si usarás texto UI

public class GameManager : MonoBehaviour
{
    #region CORE
    public static GameManager self;

    private void Awake()
    {
        self = this;
        vampirosEliminados = 0;
        highscoreActual = 0;

        Awake_Zombies();
        Awake_Vampiros();
        InicializarGeneradores();
    }

    [SerializeField] private float tiempoRestante = 300; // 5 minutos

    private void Update()
    {
        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            if (tiempoRestante < 0) tiempoRestante = 0;

            // Actualizar el UI
            UI.ActualizarTiempo(tiempoRestante);

            if (tiempoRestante <= 0)
            {
                Jugador.Morir();
            }
        }
    }
    #endregion CORE

    #region GENERADORES ALEATORIOS
    [Header("Generadores Aleatorios")]
    [SerializeField] private GeneradorVampiros pfGenerador; // Prefab del generador
    [SerializeField] private Transform[] puntosGeneracion; // Puntos posibles en el escenario
    [SerializeField] private int minGeneradores = 5;
    [SerializeField] private int maxGeneradores = 10;

    public void InicializarGeneradores()
    {
        // Limpiar lista previa
        listaGeneradoresVampiros.Clear();

        // Número aleatorio de generadores, limitado a los puntos disponibles
        int cantidad = Mathf.Min(UnityEngine.Random.Range(minGeneradores, maxGeneradores + 1), puntosGeneracion.Length);

        List<int> indicesDisponibles = new List<int>();
        for (int i = 0; i < puntosGeneracion.Length; i++)
            indicesDisponibles.Add(i);

        for (int i = 0; i < cantidad; i++)
        {
            if (indicesDisponibles.Count == 0) break;

            int randIndex = UnityEngine.Random.Range(0, indicesDisponibles.Count);
            int puntoElegido = indicesDisponibles[randIndex];
            indicesDisponibles.RemoveAt(randIndex);

            GeneradorVampiros gen = Instantiate(pfGenerador, puntosGeneracion[puntoElegido].position, Quaternion.identity);
            listaGeneradoresVampiros.Add(gen);

            Debug.Log($"Generador {i + 1}/{cantidad} instanciado en punto {puntoElegido}. Total lista: {listaGeneradoresVampiros.Count}");
        }
    }
    #endregion GENERADORES ALEATORIOS


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
            zombie.gameObject.SetActive(distancia < distanciaOptimizacion);
        }

        yield return new WaitForSeconds(1);
        goto Repetir;
    }
    #endregion ZOMBIES

    #region VAMPIROS
    public static float distanciaOptimizacionv = 60;
    public static List<Vampiro> listaVampiros = new List<Vampiro>();
    public static int vampirosActivos = 0;

    public void Awake_Vampiros()
    {
        StartCoroutine(CrRevisarVampiros());
    }

    public IEnumerator CrRevisarVampiros()
    {
    Repetir:
        foreach (Vampiro vampiro in listaVampiros)
        {
            float distanciav = Vector3.Distance(Jugador.transform.position, vampiro.transform.position);
            vampiro.gameObject.SetActive(distanciav < distanciaOptimizacionv);
        }

        yield return new WaitForSeconds(1);
        goto Repetir;
    }
    #endregion VAMPIROS

    #region GENERADORES
    public static List<GeneradorVampiros> listaGeneradoresVampiros = new List<GeneradorVampiros>();
    public static int totalGeneradoresVampiros => listaGeneradoresVampiros.Count;
    #endregion GENERADORES

    #region VICTORIA
    public void Victoria()
    {
        if (Jugador.Muerto) return;

        // Agregar puntos por tiempo restante
        highscoreActual += Mathf.FloorToInt(tiempoRestante);
        // Agregar puntos por vida restante
        highscoreActual += Jugador.Vida;

        // Guardar highscore si es mayor al anterior
        int highscoreGuardado = PlayerPrefs.GetInt("Highscore", 0);
        if (highscoreActual > highscoreGuardado)
        {
            PlayerPrefs.SetInt("Highscore", highscoreActual); // Mejor highscore
            PlayerPrefs.Save();
            Debug.Log("Nuevo Highscore: " + highscoreActual);
        }
        else
        {
            Debug.Log("Puntaje de la partida: " + highscoreActual + " | Highscore: " + highscoreGuardado);
        }

        // Guardar el highscore de esta partida para mostrar en Victory
        PlayerPrefs.SetInt("HighscoreActual", highscoreActual);
        PlayerPrefs.Save();

        StopAllCoroutines();
        SceneManager.LoadScene("Victory");
    }
    #endregion VICTORIA

    #region HIGHSCORE
    [Header("Highscore")]
    public int puntosPorVampiro = 100; // puntos por cada vampiro destruido
    private int highscoreActual = 0; // puntaje de la partida
    private int vampirosEliminados = 0; // contador de vampiros destruidos
    private float tiempoRestantePartida => tiempoRestante; // para usar en Victoria
    #endregion HIGHSCORE

    public static void AgregarVampiroEliminado()
    {
        self.vampirosEliminados++;
        self.highscoreActual += self.puntosPorVampiro;
    }



    [SerializeField] private Jugador _jugador;
    public static Jugador Jugador => self._jugador;
}

