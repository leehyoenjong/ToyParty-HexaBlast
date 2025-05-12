using UnityEngine;

public class UI_Pause_Btn : MonoBehaviour
{
    [SerializeField] GameObject G_Pause;

    public void Btn_Pause()
    {
        Instantiate(G_Pause, null);
    }
}