using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Collider trigger;
    private Material materialCube;

    private void Awake()
    {
        trigger = GetComponents<BoxCollider>()[1];
        materialCube =
            transform.GetChild(0).GetComponent<Renderer>().material;
    }

    public void Activar()
    {
        materialCube.SetColor(name:"_EmissionColor", value:Color.green * 4);
        Destroy(trigger);
        Destroy(obj:this);
    }
}
