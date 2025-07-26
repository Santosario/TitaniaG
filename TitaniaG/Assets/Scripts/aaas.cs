using UnityEngine;

public class aaas : MonoBehaviour
{
    Animator anim;

    float timer = 0f;
    float timeout = 5f;
    bool isUsingSword = false;
    bool isUsingGun = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Movimiento básico (W para correr)
        anim.SetBool("isRunning", Input.GetKey(KeyCode.W));

        // TECLA Q: Saca espada y ataca
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isUsingSword = true;
            isUsingGun = false;
            timer = 0f;

            anim.SetBool("hasSword", true);
            anim.SetBool("hasGun", false);

            // Espera a que termine la animación de sacar espada antes de atacar
            StartCoroutine(AttackSwordDelayed(0.5f)); // 0.5s es un estimado, ajústalo según la animación
        }

        // TECLA E: Saca pistola y dispara
        if (Input.GetKeyDown(KeyCode.E))
        {
            isUsingGun = true;
            isUsingSword = false;
            timer = 0f;

            anim.SetBool("hasGun", true);
            anim.SetBool("hasSword", false);

            StartCoroutine(ShootDelayed(0.1f));
        }

        // Temporizador para volver a Idle
        if (isUsingSword || isUsingGun)
        {
            timer += Time.deltaTime;
            if (timer >= timeout)
            {
                isUsingSword = false;
                isUsingGun = false;

                anim.SetBool("hasSword", false);
                anim.SetBool("hasGun", false);
                timer = 0f;
            }
        }
    }

    System.Collections.IEnumerator AttackSwordDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isUsingSword)
        {
            anim.SetTrigger("attackSword");
        }
    }

    System.Collections.IEnumerator ShootDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isUsingGun)
        {
            anim.SetTrigger("Shoot");
        }
    }
}
