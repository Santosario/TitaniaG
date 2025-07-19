using UnityEngine;

public class TriggerAtaque : MonoBehaviour
{
    [SerializeField] private int da�o = 10;

    private void OnTriggerEnter(Collider other)
    {
       if (other.gameObject.layer == 6)
        {
            GameManager.Jugador.Vida -= da�o;
        }
    }
}
