using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Vampiro : MonoBehaviour
{
    #region CORE
    public bool instanciado = false;
    private void Awake()
    {
        ObtenerComponentes();
        InicializarVida();
        Awake_MainVampiro();
        GameManager.listaVampiros.Add(this);
    }
    private void OnEnable()
    {
        GameManager.vampirosActivos++;
        CambiarDeConducta();
    }
    private void OnDisable()
    {
        GameManager.vampirosActivos--;
    }
    private void OnDestroy()
    {
        GameManager.listaVampiros.Remove(this);
    }
    #endregion

    #region COMPONENTES
    private NavMeshAgent agent;
    private CapsuleCollider capsula;
    private Animator animator;
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
    private List<Material> listaMateriales;
    private void ObtenerComponentes()
    {
        agent = GetComponent<NavMeshAgent>();
        capsula = GetComponent<CapsuleCollider>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        listaMateriales = new List<Material>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            listaMateriales.Add(r.material);
        }
    }
    #endregion

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
        StopAllCoroutines();
        UnlockState();
        if (agent != null) agent.isStopped = true;
        if (capsula != null) capsula.enabled = false;
        if (canvas != null) canvas.gameObject.SetActive(false);
        ChangeAnimationState(AnimStates.MORIR, true);
        Invoke("Destruir", 5);
    }
    private void Destruir()
    {
        Destroy(gameObject);
    }

    private void RecibirDaño()
    {
        Emision = Color.red * 3;
        float dur = GetClipLength(AnimStates.RECIBIR_DAÑO);
        LockState(AnimStates.RECIBIR_DAÑO, dur);
        ChangeAnimationState(AnimStates.RECIBIR_DAÑO);
        StartCoroutine(FinRecibirDaño(dur));
        Invoke("QuitarEmisivo", 0.1f);
    }

    private IEnumerator FinRecibirDaño(float t)
    {
        yield return new WaitForSeconds(t);
        if (!Muerto)
            ChangeAnimationState(AnimStates.IDLE);
        UnlockState();
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
    #endregion

    #region NAV MESH AGENT
    private Vector3 PuntoRandom()
    {
        float rangoMinimo = 4;
        float rangoMaximo = 8;
        float x = Random.Range(rangoMinimo, rangoMaximo);
        float z = Random.Range(rangoMinimo, rangoMaximo);
        x *= Random.Range(0, 2) == 0 ? 1 : -1;
        z *= Random.Range(0, 2) == 0 ? 1 : -1;
        Vector3 posicionRandom = transform.position + new Vector3(x, 0, z);
        if (NavMesh.SamplePosition(posicionRandom, out NavMeshHit hit, 5, NavMesh.AllAreas))
            return hit.position;
        return transform.position;
    }
    private void Caminar(Vector3 posicion)
    {
        ChangeAnimationState(AnimStates.CAMINAR);
        agent.speed = 0.4f;
        agent.stoppingDistance = 0;
        agent.SetDestination(posicion);
    }
    private void PerseguirAlJugador()
    {
        ChangeAnimationState(AnimStates.CORRER);
        agent.speed = 3f;
        agent.stoppingDistance = 0f;
        agent.SetDestination(GameManager.Jugador.transform.position);
    }
    private void Parar()
    {
        ChangeAnimationState(AnimStates.IDLE);
        Atacando = false;
        agent.stoppingDistance = 0;
        agent.SetDestination(transform.position);
    }
    #endregion

    #region MAIN VAMPIRO
    private IEnumerator crConducta;
    private IEnumerator crVision;
    private float distanciaJugador = 0;
    private bool pasivo = false;
    private void Awake_MainVampiro()
    {
        CambiarDeConducta();
    }
    private void CambiarDeConducta()
    {
        StopAllCoroutines();
        crConducta = pasivo ? CrConductaActiva() : CrConductaPasiva();
        StartCoroutine(crConducta);
        pasivo = !pasivo;
    }
    #endregion

    #region CONDUCTA PASIVA
    private float radioVisionPasiva = 5;
    private IEnumerator CrConductaPasiva()
    {
        crVision = CrVisionPasiva();
        StartCoroutine(crVision);
        float tiempoMinimo = 2f;
        float tiempoMaximo = 5f;
    Inicio:
        Parar();
        Caminar(PuntoRandom());
        yield return new WaitForSeconds(1f);
    RevisarDestino:
        if (agent.remainingDistance > 0.1f)
        {
            yield return new WaitForFixedUpdate();
            goto RevisarDestino;
        }
        Parar();
        float tiempoEspera = Random.Range(tiempoMinimo, tiempoMaximo);
        yield return new WaitForSeconds(tiempoEspera);
        goto Inicio;
    }
    private IEnumerator CrVisionPasiva()
    {
    Inicio:
        yield return new WaitForFixedUpdate();
        distanciaJugador = Vector3.Distance(transform.position, GameManager.Jugador.transform.position);
        if (distanciaJugador < radioVisionPasiva)
        {
            CambiarDeConducta();
            yield break;
        }
        goto Inicio;
    }
    #endregion

    #region CONDUCTA ACTIVA
    private float radioVisionActiva = 10f;
    [SerializeField] private float radioAtaque = 1.2f;
    [Header("Ataque")]
    [SerializeField] private bool permitirAtaqueDientes = true;
    [SerializeField] private float tiempoDeRecargaAtaque = 1.2f;
    private bool puedeAtacar = true;
    private IEnumerator CrConductaActiva()
    {
        crVision = CrVisionActiva();
        StartCoroutine(crVision);
    Inicio:
        yield return new WaitForSeconds(0.04f);
        if (GameManager.Jugador.Muerto)
        {
            Atacando = false;
            Parar();
        }
        else if (distanciaJugador > radioAtaque)
        {
            Atacando = false;
            PerseguirAlJugador();
        }
        else
        {
            Atacando = true;
        }
        goto Inicio;
    }
    private IEnumerator CrVisionActiva()
    {
    Inicio:
        yield return new WaitForFixedUpdate();
        distanciaJugador = Vector3.Distance(transform.position, GameManager.Jugador.transform.position);
        if (distanciaJugador > radioVisionActiva)
        {
            CambiarDeConducta();
            yield break;
        }
        goto Inicio;
    }

    private bool _atacando;
    public bool Atacando
    {
        get => _atacando;
        set
        {
            if (Muerto || GameManager.Jugador.Muerto)
            {
                value = false;
            }
            if (value && puedeAtacar && !_atacando)
            {
                _atacando = true;
                puedeAtacar = false;
                agent.stoppingDistance = 0;
                agent.SetDestination(transform.position);
                if (permitirAtaqueDientes && Random.value < 0.5f)
                {
                    ChangeAnimationState(AnimStates.ATACAR_DIENTES);
                }
                else
                {
                    ChangeAnimationState(AnimStates.ATACAR);
                }
                StartCoroutine(RecuperarAtaqueTrasAnimacion());
            }
            else if (!value)
            {
                _atacando = false;
            }
        }
    }

    private IEnumerator RecuperarAtaqueTrasAnimacion()
    {
        string ataqState = (currentAnimationState == AnimStates.ATACAR_DIENTES) ? AnimStates.ATACAR_DIENTES : AnimStates.ATACAR;
        float dur = GetClipLength(ataqState);
        LockState(ataqState, dur);
        yield return new WaitForSeconds(dur);
        _atacando = false;
        ChangeAnimationState(AnimStates.IDLE);
        yield return new WaitForSeconds(tiempoDeRecargaAtaque);
        puedeAtacar = true;
        UnlockState();
    }
    #endregion

    #region ANIMACIÓN con Estabilización/Bloqueo
    //---- BLOQUEO PARA ANIMACIONES ONE-SHOT (ataque/daño/muerte)
    private string lockedState = null;
    private float lockedUntil = 0f;
    private bool IsLocked => lockedState != null && Time.time < lockedUntil;
    private void LockState(string state, float duration)
    {
        lockedState = state;
        lockedUntil = Time.time + Mathf.Max(0.01f, duration);
    }
    private void UnlockState()
    {
        lockedState = null;
    }
    private void ChangeAnimationState(string newState, bool force = false)
    {
        if (currentAnimationState == newState) return;
        // SOLO dejar interrumpir ataques o daño si NO está bloqueado, salvo por muerte o "force"
        if (!force && IsLocked && newState != lockedState && newState != AnimStates.MORIR)
            return;

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
    #endregion ANIMACIÓN

    #region GIZMOS
    private void OnDrawGizmos()
    {
        Gizmos.color = pasivo ? Color.white : Color.red;
        Gizmos.DrawWireSphere(transform.position, pasivo ? radioVisionPasiva : radioVisionActiva);
        if (!pasivo)
        {
            Gizmos.color = new Color(1f, 0.5f, 0);
            Gizmos.DrawWireSphere(transform.position + Vector3.up, radioAtaque);
        }
    }
    #endregion
}