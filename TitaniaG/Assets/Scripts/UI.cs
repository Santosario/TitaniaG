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

    #region HUD
    [Header("HUD")]
    [SerializeField] private TMP_Text txtMunicion;
    [SerializeField] private Image barraCorrer;
    [SerializeField] private Image rellenoCorrer;

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
        float porcentaje = (float)vidaActual / vidaMaxima;
        self.txtVidaHUD.text = $"{vidaActual} / {vidaMaxima}";
    }

    #endregion HUD
}
