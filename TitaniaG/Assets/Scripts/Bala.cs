using UnityEngine;

public class Bala:MonoBehaviour
{
    [SerializeField] private int daño = 10;
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
                // Daño a vampiro si existe
                Vampiro vampiro = other.gameObject.GetComponent<Vampiro>();
                if (vampiro != null)
                    vampiro.Vida -= daño;

                // Daño a generador si existe
                GeneradorVampiros generador = other.gameObject.GetComponent<GeneradorVampiros>();
                if (generador != null)
                    generador.Vida -= daño;
            }

            Destroy(gameObject);
        }
    }
}
