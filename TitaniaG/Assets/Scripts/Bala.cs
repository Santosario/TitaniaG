using UnityEngine;

public class Bala:MonoBehaviour
{
    [SerializeField] private int da�o = 10;
    [SerializeField] private float velocidad = 20;
    [SerializeField] private ParticleSystem explosion;

    private void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.linearVelocity = transform.forward * velocidad;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer is 0 or 7)
        {
            Instantiate(explosion, transform.position, transform.rotation);

            if (other.gameObject.layer == 7)
            {
                // Da�o a vampiro si existe
                Vampiro vampiro = other.gameObject.GetComponent<Vampiro>();
                if (vampiro != null)
                    vampiro.Vida -= da�o;

                // Da�o a generador si existe
                GeneradorVampiros generador = other.gameObject.GetComponent<GeneradorVampiros>();
                if (generador != null)
                    generador.Vida -= da�o;
            }

            Destroy(gameObject);
        }
    }
}
