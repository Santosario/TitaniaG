using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Vampiro : MonoBehaviour
{
    #region COMPONENTES
    private NavMeshAgent agent;
    private CapsuleCollider capsula;
    private Animator animator;
    private List<Material> listaMateriales;
    private string currentAnimationState;

    private static class AnimStates
    {
        public const string IDLE = "idle";
        public const string CAMINAR = "caminar";
        public const string CORRER = "correr";
        public const string ATACAR = "Atacar";
        public const string ATACAR_DIENTES = "Atacar dientes";
        public const string MORIR = "Muerte";
        public const string RECIBIR_DAÑO = "Recibir daño";
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        capsula = GetComponent<CapsuleCollider>();
        animator = transform.GetChild(0).GetComponent<Animator>();

        listaMateriales = new List<Material>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            listaMateriales.Add(r.material);

        InicializarVida();
    }
    #endregion

    #region VIDA
    [Header("Vida")]
    [SerializeField] private Transform canvas;
    [SerializeField] private Image barraVida;
    [SerializeField] private int vidaBase = 40;

    private int _vida;
    private int vidaMaxima;
    public bool Muerto => _vida <= 0;

    private void InicializarVida()
    {
        vidaMaxima = vidaBase;
        _vida = vidaMaxima;
        if (barraVida != null)
            barraVida.fillAmount = (float)_vida / vidaMaxima;
    }

    public int Vida
    {
        get => _vida;
        set
        {
            if (Muerto) return;
            if (value < _vida) RecibirDaño();

            _vida = Mathf.Clamp(value, 0, vidaMaxima);

            if (barraVida != null)
                barraVida.fillAmount = (float)_vida / vidaMaxima;

            if (_vida <= 0) Morir();
        }
    }

    private void RecibirDaño()
    {
        Emision = Color.red * 3;
        Invoke(nameof(QuitarEmisivo), 0.1f);

        ChangeAnimationState(AnimStates.RECIBIR_DAÑO, true);
        float dur = GetClipLength(AnimStates.RECIBIR_DAÑO);
        StartCoroutine(FinRecibirDaño(dur));
    }

    private IEnumerator FinRecibirDaño(float dur)
    {
        yield return new WaitForSeconds(dur);
        if (!Muerto)
            currentAnimationState = null; // deja que Update() elija la animación correcta
    }

    private void QuitarEmisivo()
    {
        Emision = Color.black;
    }

    public Color Emision
    {
        set
        {
            foreach (Material m in listaMateriales)
                m.SetColor("_EmissionColor", value);
        }
    }

    private void Morir()
    {
        StopAllCoroutines();
        agent.isStopped = true;
        capsula.enabled = false;
        if (canvas != null) canvas.gameObject.SetActive(false);
        ChangeAnimationState(AnimStates.MORIR, true);
        GameManager.AgregarVampiroEliminado();
        Invoke(nameof(Destruir), 5f);
    }

    private void Destruir()
    {
        Destroy(gameObject);
    }
    #endregion

    #region ATAQUE Y MOVIMIENTO
    [Header("Ataque")]
    [SerializeField] private float radioAtaque = 1.5f;
    [SerializeField] private float velocidad = 3f;
    [SerializeField] private bool permitirAtaqueDientes = true;
    [SerializeField] private float tiempoRecargaAtaque = 1.2f;
    private bool puedeAtacar = true;
    private bool atacando = false;

    private void Update()
    {
        if (Muerto || GameManager.Jugador.Muerto) return;

        float distancia = Vector3.Distance(transform.position, GameManager.Jugador.transform.position);

        if (distancia > radioAtaque)
        {
            // Persigue al jugador
            agent.isStopped = false;
            agent.speed = velocidad;
            agent.stoppingDistance = 0f;
            agent.SetDestination(GameManager.Jugador.transform.position);
            ChangeAnimationState(AnimStates.CORRER);
            atacando = false;
        }
        else
        {
            // Solo atacar si no está atacando y puede atacar
            agent.isStopped = true;
            if (!atacando && puedeAtacar)
            {
                StartCoroutine(RealizarAtaque());
            }
        }
    }

    private IEnumerator RealizarAtaque()
    {
        atacando = true;
        puedeAtacar = false;

        string anim = permitirAtaqueDientes && Random.value < 0.5f ? AnimStates.ATACAR_DIENTES : AnimStates.ATACAR;
        ChangeAnimationState(anim);

        float dur = GetClipLength(anim);

        // Esperar a la mitad de la animación para aplicar daño
        yield return new WaitForSeconds(dur / 2f);

        // Aplicar daño solo si el jugador sigue dentro del radio
        float distancia = Vector3.Distance(transform.position, GameManager.Jugador.transform.position);
        if (distancia <= radioAtaque)
        {
            GameManager.Jugador.Trigger_AtaqueVampiro();
        }

        // Esperar a que termine la animación
        yield return new WaitForSeconds(dur / 2f);

        atacando = false;
        ChangeAnimationState(AnimStates.IDLE);

        yield return new WaitForSeconds(tiempoRecargaAtaque);
        puedeAtacar = true;
    }
    #endregion

    #region ANIMACIÓN
    private void ChangeAnimationState(string newState, bool force = false)
    {
        if (currentAnimationState == newState) return;
        animator.Play(newState);
        currentAnimationState = newState;
    }

    private float GetClipLength(string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return 1f;
        foreach (var c in animator.runtimeAnimatorController.animationClips)
            if (c.name == clipName) return c.length;
        return 1f;
    }
    #endregion
}
