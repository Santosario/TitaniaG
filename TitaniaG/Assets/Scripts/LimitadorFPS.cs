using UnityEngine;

public class LimitadorFPS : MonoBehaviour
{
    public int LimitaFPS = 120;
    public bool vSync = false;
    private void Start()
    {
        //Limitamos el framerate a 120 fps
        Application.targetFrameRate = 120;

        QualitySettings.vSyncCount = vSync ? 1 : 0;
    }
}
