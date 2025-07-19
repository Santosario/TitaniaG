using UnityEngine;

public class Autodestruccion:MonoBehaviour
{
    [SerializeField] private float tiempo = 5;

    private void Start()
    {
        Invoke(methodName: "Destruir", tiempo);
    }

    private void Destruir()
    {
        Destroy(gameObject);
    }
}