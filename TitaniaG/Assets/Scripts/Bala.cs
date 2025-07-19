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
                other.gameObject.GetComponent<Zombie>().Vida -= daño;

            Destroy(gameObject);
        }
        
    }
}
