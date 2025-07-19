using UnityEngine;

public class SpawnBala : MonoBehaviour
{
    [SerializeField] private Jugador jugador;
    [SerializeField] private Bala pfBala;

    private void OnEnable()
    {
        if(jugador.BalasCargador <= 0)
        {
            if (jugador.ModoDisparo == ModoDisparo.Automatico)
            {
                jugador.PararDisparoAutomatico();
            }

            return;
        }

        Instantiate(pfBala, transform.position, transform.rotation);
        jugador.BalasCargador--;
    }
}
