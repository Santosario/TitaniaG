using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    #region CORE
    private static UI self;
    private void Awake()
    {
        self = this;
    }
    #endregion CORE

    private void Start()
    {
        int highscoreObtenido = PlayerPrefs.GetInt("HighscoreActual", 0);
        int mejorHighscore = PlayerPrefs.GetInt("Highscore", 0);

        txtHighscore.text = $"Highscore obtenido: {highscoreObtenido}";
        txtMejorHighscore.text = $"Mejor Highscore: {mejorHighscore}";
    }

    #region HUD
    [Header("HUD")]
    [SerializeField] private TMP_Text txtMunicion;
    [SerializeField] private Image barraCorrer;
    [SerializeField] private Image rellenoCorrer;

    [Header("Highscore")]
    [SerializeField] private TMP_Text txtHighscore; // Highscore de esta partida
    [SerializeField] private TMP_Text txtMejorHighscore; // Mejor highscore

    public static Image BarraCorrer => self.barraCorrer;
    public static Image RellenoCorrer => self.rellenoCorrer;

    public static string Municion
    {
        set => self.txtMunicion.text = value;
    }

    [Header("Vida")]
    [SerializeField] private TMP_Text txtVidaHUD;

    public static void ActualizarVida(int vidaActual, int vidaMaxima)
    {
        self.txtVidaHUD.text = $"{vidaActual} / {vidaMaxima}";
    }

    [Header("Generadores")]
    [SerializeField] private TMP_Text txtGeneradores;

    private void Update()
    {
        int generadoresVivos = GameManager.listaGeneradoresVampiros.Count;
        txtGeneradores.text = $"Tumbas restantes: {generadoresVivos}";
    }

    [Header("Tiempo")]
    [SerializeField] private TMP_Text txtTiempo;

    public static void ActualizarTiempo(float tiempoRestante)
    {
        int minutos = Mathf.FloorToInt(tiempoRestante / 60f);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60f);
        self.txtTiempo.text = $"{minutos:00}:{segundos:00}";
    }
    #endregion HUD

    #region HIGHSCORE
    public static void MostrarHighscore(int highscoreActual)
    {
        // Mejor highscore guardado
        int mejor = PlayerPrefs.GetInt("Highscore", 0);

        // Si esta partida supera el anterior, guardamos
        if (highscoreActual > mejor)
        {
            mejor = highscoreActual;
            PlayerPrefs.SetInt("Highscore", mejor);
            PlayerPrefs.Save();
        }

        // Actualizar textos
        self.txtHighscore.text = "Highscore obtenido: " + highscoreActual;
        self.txtMejorHighscore.text = "Mejor Highscore: " + mejor;
    }
    #endregion HIGHSCORE
}
