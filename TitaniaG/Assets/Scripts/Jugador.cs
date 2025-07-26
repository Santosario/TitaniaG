using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;



public class Jugador : MonoBehaviour
{

    #region CORE

    private void Awake()
    {
        Awake_ObtenerComponentes();
        UI.ActualizarVida(_vida, vidaMaxima);
    }

    // Update is called once per frame
    private void Update()
    {
        Update_Disparo();
        Update_Movimiento();
        Update_Test();
        Update_Espada();
    }

    private void Update_Test()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            Vida -= 25;
        }
    }
    #endregion CORE

    #region COMPONENTES

    private CharacterController cc;
    private Animator animator;


    private void Awake_ObtenerComponentes()
    {
        cc = GetComponent<CharacterController>();
        animator = transform.GetChild(0).GetComponent<Animator>();
    }
    #endregion COMPONENTES

    #region MOVIMIENTO
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 5;
    [SerializeField] private float velocidadCorrer = 10;

    [Header("Rotaci�n con Mouse")]
    [SerializeField] private Transform objetivoCamara; // El GameObject que la Cinemachine sigue (CinemachineCameraTarget)
    [SerializeField] private float sensibilidadMouse = 2f;

    [Header("Dash")]
    [SerializeField] private float fuerzaDash = 20f;
    [SerializeField] private float duracionDash = 0.2f;
    [SerializeField] private float cooldownDash = 1.0f;

    private bool enDash = false;
    private float tiempoDashRestante = 0f;
    private float cooldownDashRestante = 0f;
    private Vector3 direccionDash;

    private float rotacionVertical = 0f;

    private Vector3 axis;
    private Vector3 mov = Vector3.zero;

    private bool bloquearMovimiento = false;
    public bool bloquearRotacion = false;
    private bool corriendo = false;
    private float tCorrerMax = 3;
    private float tCorrer = 3;


    public const float Gravedad = -9.81f;

    private void Update_Movimiento()

    {
        if (corriendo)
        {
            if (tCorrer > 0) tCorrer -= Time.deltaTime;
            else corriendo = false;
        }

        else
        {
            if (tCorrer < tCorrerMax) tCorrer += Time.deltaTime / 2;
        }

        if (tCorrer < tCorrerMax)
        {
            UI.BarraCorrer.gameObject.SetActive(true);
            UI.RellenoCorrer.fillAmount = tCorrer / tCorrerMax;
        }

        else
        {
            UI.BarraCorrer.gameObject.SetActive(false);
        }

        if (!enDash && Input.GetKeyDown(KeyCode.Space) && cooldownDashRestante <= 0f && axis != Vector3.zero)
        {
            enDash = true;
            tiempoDashRestante = duracionDash;
            cooldownDashRestante = cooldownDash;
            direccionDash = transform.TransformDirection(axis);
        }


        Rotar();

        if (bloquearMovimiento) return;

        axis = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        axis.Normalize();

        if (Input.GetKeyDown(KeyCode.LeftShift) && tCorrer > 0) corriendo = true;
        if (Input.GetKeyUp(KeyCode.LeftShift)) corriendo = false;

        float vel = corriendo ? velocidadCorrer : velocidad;


        animator.SetFloat("x", axis.x);
        animator.SetFloat("y", axis.z);
        animator.SetFloat("velocidad", vel);


        Vector3 movXZ = transform.TransformDirection(axis) * vel;
        mov.x = movXZ.x;
        mov.z = movXZ.z;

        if (cc.isGrounded)
        {
            mov.y = 0;
        }
        else
        {
            mov.y += Gravedad * Time.deltaTime;
        }

        cc.Move(mov * Time.deltaTime);

        if (enDash)
        {
            cc.Move(direccionDash * fuerzaDash * Time.deltaTime);
            tiempoDashRestante -= Time.deltaTime;
            if (tiempoDashRestante <= 0f)
            {
                enDash = false;
            }
        }
        else
        {
            if (cooldownDashRestante > 0f)
                cooldownDashRestante -= Time.deltaTime;
        }
    }

    private void Rotar()
    {
        if (bloquearRotacion) return;

        float mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadMouse;

        // Rotar el jugador (horizontal)
        transform.Rotate(Vector3.up * mouseX);

        // Ya no rotamos verticalmente la c�mara
        rotacionVertical = 0f;
        objetivoCamara.localRotation = Quaternion.identity;

        objetivoCamara.localRotation = Quaternion.Euler(rotacionVertical, 0f, 0f);
    }

    #endregion MOVIMIENTO

    #region DISPARO

    [SerializeField] private AnimationClip aDisparoI;
    [SerializeField] private AnimationClip aDisparoR;
    [SerializeField] private AnimationClip aDisparoA;
    [SerializeField] private AnimationClip aRecarga;

    public ModoDisparo ModoDisparo = ModoDisparo.Individual;


    private bool disparando = false;

    private void Update_Disparo()
    {
        if (Input.GetKeyDown(KeyCode.R)) Recargar();

        if (Input.GetMouseButtonDown(1)) Disparar();

        if (Input.GetMouseButtonUp(1))
        {
            if (ModoDisparo == ModoDisparo.Automatico)
                PararDisparoAutomatico();
        }

        if (Input.GetKeyDown(KeyCode.Q)) CambiarModoDisparo();
    }

    public void PararDisparoAutomatico()
    {
        animator.SetBool(name: "disparoAutomatico", value: false);
        disparando = false;
        bloquearMovimiento = false;
    }

    private void Disparar()
    {
        if (!PuedeDisparar) return;

        bloquearMovimiento = true;

        disparando = true;

        switch (ModoDisparo)
        {
            case ModoDisparo.Individual:
                animator.SetTrigger(name: "disparoIndividual");
                Invoke(methodName: "TimerAnimacionDisparo", aDisparoI.length);
                break;

            case ModoDisparo.Rafaga:
                animator.SetTrigger(name: "disparoRafaga");
                Invoke(methodName: "TimerAnimacionDisparo", aDisparoR.length);
                break;

            case ModoDisparo.Automatico:
                animator.SetBool(name: "disparoAutomatico", value: true);
                break;
        }

    }

    public void TimerAnimacionDisparo()
    {
        disparando = false;
        bloquearMovimiento = false;
    }

    private void CambiarModoDisparo()
    {
        switch (ModoDisparo)
        {
            case ModoDisparo.Individual: ModoDisparo = ModoDisparo.Rafaga; break;
            case ModoDisparo.Rafaga: ModoDisparo = ModoDisparo.Automatico; break;
            case ModoDisparo.Automatico: ModoDisparo = ModoDisparo.Individual; break;
        }
    }

    public bool PuedeDisparar
    {
        get
        {
            if (disparando) return false;
            if (BalasCargador <= 0) return false;
            return true;
        }
    }

    private bool recargando = false;

    private void Recargar()
    {
        if (disparando) return;
        if (recargando) return;
        if (Municion <= 0) return;
        recargando = true;
        bloquearMovimiento = true;
        animator.SetTrigger(name: "recargar");
        Invoke(methodName: "TerminarRecarga", aRecarga.length);
    }

    private void TerminarRecarga()
    {
        recargando = false;
        bloquearMovimiento = false;
        if (Municion >= tama�oCargador)
        {
            Municion -= tama�oCargador;
            BalasCargador = tama�oCargador;
        }

        else
        {
            BalasCargador = Municion;
            Municion = 0;
        }
    }

    #endregion DISPARO

    #region DISPARO Municion

    private int tama�oCargador = 25;
    private int _balasCargador = 25;
    private int _municion = 80;
    private int municionMax = 500;

    public bool MunicionCompleta => Municion >= municionMax;

    public int BalasCargador
    {
        get => _balasCargador;
        set
        {
            _balasCargador = value;
            UI.Municion = BalasCargador + " / " + Municion;
        }
    }

    public int Municion
    {
        get => _municion;
        set
        {
            if (value > municionMax) _municion = municionMax;
            else _municion = value;

            UI.Municion = BalasCargador + " / " + Municion;
        }
    }

    private void Trigger_Municion(Municion municionObj)
    {
        if (MunicionCompleta) return;
        Municion += municionObj.Cantidad;
        Destroy(municionObj.gameObject);
    }

    #endregion DISPARO Municion

    #region TRIGGERS

    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.layer)
        {
            case 9: Trigger_Checkpoint(other.GetComponent<Checkpoint>()); break;
            case 10: Trigger_Municion(other.GetComponent<Municion>()); break;
        }
    }



    #endregion TRIGGERS

    #region CHECKPOINTS

    private List<Vector3> listaCheckpoints = new List<Vector3>() { Vector3.zero };
    private void Trigger_Checkpoint(Checkpoint checkpoint)
    {
        checkpoint.Activar();
        listaCheckpoints.Add(item: checkpoint.transform.position + (Vector3.back * 2));
    }

    private Vector3 CheckpointMasCercano()
    {
        Vector3 origen = transform.position;
        return listaCheckpoints.OrderBy(cp => Vector3.Distance(a: cp, b: origen)).FirstOrDefault();
    }



    #endregion CHECKPOINTS

    #region ATAQUE CUERPO A CUERPO

    [Header("Ataque cuerpo a cuerpo")]
    [SerializeField] private float rangoEspada = 2f;
    [SerializeField] private float anguloEspada = 90f; // Semic�rculo frontal
    [SerializeField] private int da�oEspada = 15;
    [SerializeField] private float tiempoEntreAtaques = 0.6f;

    private bool puedeAtacar = true;


    private void Update_Espada()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Clic izquierdo detectado | Puede atacar: " + puedeAtacar);
        }

        if (Input.GetMouseButtonDown(0) && puedeAtacar)
        {
            Debug.Log("Iniciando coroutine de espadazo");
            StartCoroutine(RealizarAtaqueEspada());
        }
    }

    private IEnumerator RealizarAtaqueEspada()
    {
        animator.SetTrigger("atacar");
        puedeAtacar = false;

        // Punto desde donde se eval�a el ataque (ligeramente al frente y elevado)
        Vector3 origen = transform.position + Vector3.up * 1.0f + transform.forward * 0.5f;

        // Obtenemos todos los colliders en un radio amplio
        float radio = rangoEspada;
        Collider[] hits = Physics.OverlapSphere(origen, radio);

        foreach (Collider c in hits)
        {
            if (c.gameObject.layer == 7) // Enemigos
            {
                Vector3 direccionAlObjetivo = (c.transform.position - transform.position).normalized;

                // C�lculo del �ngulo entre el frente del jugador y el objetivo
                float angulo = Vector3.Angle(transform.forward, direccionAlObjetivo);

                // Un cono m�s angosto en la base pero m�s largo (como una pir�mide)
                if (angulo < anguloEspada / 2f)
                {
                    float distancia = Vector3.Distance(transform.position, c.transform.position);
                    if (distancia <= rangoEspada) // Controla el largo del cono
                    {

                        Vampiro z = c.GetComponent<Vampiro>();
                        if (z != null)
                        {
                            z.Vida -= da�oEspada;
                        }

                        //Zombie z = c.GetComponent<Zombie>();
                        //if (z != null)
                        //{
                        //    z.Vida -= da�oEspada;
                        //}
                    }
                }
            }
        }

        yield return new WaitForSeconds(tiempoEntreAtaques);
        puedeAtacar = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 origen = transform.position + Vector3.up * 1.0f + transform.forward * 0.5f;
        Gizmos.DrawWireSphere(origen, rangoEspada);

        // Dibuja l�neas para mostrar el �ngulo
        Vector3 dirIzquierda = Quaternion.Euler(0, -anguloEspada / 2f, 0) * transform.forward;
        Vector3 dirDerecha = Quaternion.Euler(0, anguloEspada / 2f, 0) * transform.forward;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position + Vector3.up * 1.0f, dirIzquierda * rangoEspada);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.0f, dirDerecha * rangoEspada);
    }

    #endregion ATAQUE CUERPO A CUERPO


    #region VIDA

    [Header("Vida")]
    [SerializeField] private Transform canvas;
    [SerializeField] private Image barraVida;

    private int _vida = 100;
    private int vidaMaxima = 100;
    public bool Muerto = false;

    private void LateUpdate()
    {
    }

    private void Trigger_AtaqueZombie()
    {
        vidaMaxima -= 10;
    }

    private void Trigger_AtaqueVampiro()
    {
        vidaMaxima -= 10;
    }

    public int Vida
    {
        get => _vida;
        set
        {
            if (Muerto) return;

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

            UI.ActualizarVida(_vida, vidaMaxima);
        }
    }

    private void Morir()
    {
        Muerto = true;
        cc.enabled = false;
        animator.SetBool(name: "muerto", value: true);
        bloquearMovimiento = true;
        bloquearRotacion = true;

        canvas.gameObject.SetActive(false);

        Invoke(methodName: "Revivir", time: 3);
    }

    private void Revivir()
    {
        transform.position = CheckpointMasCercano();
        Muerto = false;
        cc.enabled = true;
        animator.SetBool(name: "muerto", value: false);

        bloquearMovimiento = false;
        bloquearRotacion = false;


        Vida = vidaMaxima;
    }

}

#endregion VIDA