using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneradorVampiros : MonoBehaviour
{
    public int tiempoGeneracion = 2;
    public int limiteVampiros = 20;
    private Vampiro pfVampiro;
    private List<Material> listaMateriales;

    private void Awake()
    {
        pfVampiro = Resources.Load<Vampiro>("Vampiro");
        InicializarVida();
        InicializarMateriales(); // <-- inicializamos los materiales
    }

    private void OnEnable()
    {
        //if (!GameManager.listaGeneradoresVampiros.Contains(this))
            //GameManager.listaGeneradoresVampiros.Add(this);

        StartCoroutine(Corrutina());
    }

    private void OnDisable()
    {
        GameManager.listaGeneradoresVampiros.Remove(this);

        if (GameManager.listaGeneradoresVampiros.Count == 0)
        {
            GameManager.self.Victoria();
        }
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

    #region VIDA
    [Header("Vida")]
    [SerializeField] private Transform canvas;
    [SerializeField] private Image barraVida;
    [SerializeField] private int vidaBase = 40;      // configurable
    [SerializeField] private bool duplicarVida = false; // duplica la vida si está activo
    private int _vida;
    private int vidaMaxima;
    public bool Muerto => _vida <= 0;

    private void InicializarVida()
    {
        vidaMaxima = duplicarVida ? vidaBase * 2 : vidaBase;
        _vida = vidaMaxima;
        if (barraVida != null)
            barraVida.fillAmount = (float)_vida / vidaMaxima;
    }

    private void InicializarMateriales()
    {
        listaMateriales = new List<Material>();
        // obtenemos todos los materiales de este objeto y sus hijos
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            listaMateriales.AddRange(r.materials);
        }
    }

    private void LateUpdate()
    {
        if (canvas != null && Camera.main != null)
            canvas.LookAt(Camera.main.transform);
    }

    public int Vida
    {
        get => _vida;
        set
        {
            if (Muerto) return;
            if (value < _vida) RecibirDaño();
            if (value <= 0)
            {
                _vida = 0;
                Morir();
            }
            else if (value > vidaMaxima)
            {
                _vida = vidaMaxima;
            }
            else
            {
                _vida = value;
            }
            if (barraVida != null)
                barraVida.fillAmount = (float)_vida / vidaMaxima;
        }
    }

    private void Morir()
    {
        Invoke("Destruir", 2);
    }

    private void Destruir()
    {
        Destroy(gameObject);
    }

    private void RecibirDaño()
    {
        Emision = Color.red * 3;
        Invoke("QuitarEmisivo", 0.1f);
    }

    private void QuitarEmisivo()
    {
        Emision = Color.black;
    }

    public Color Emision
    {
        set
        {
            if (listaMateriales == null) return;
            foreach (Material m in listaMateriales)
                m.SetColor("_EmissionColor", value);
        }
    }
    #endregion
}
