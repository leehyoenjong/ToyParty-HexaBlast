using UnityEngine;

public class UI_Lobby : MonoBehaviour
{
    [SerializeField] UI_Stage_Main uI_Stage_Main;

    private void Start()
    {
        uI_Stage_Main.Initialized();
    }
}