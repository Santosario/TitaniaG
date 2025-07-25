using UnityEngine;

public class enemianim : MonoBehaviour
{

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Asegúrate de que "Idle" exista en el Animator Controller
        anim.Play("idle");
    }
}
