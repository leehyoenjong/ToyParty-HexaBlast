using TMPro;
using UnityEngine;

public class UI_Move_Profile : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI T_Count;

    public void Initailized()
    {
        T_Count.text = StageManager.Get_Stage_Data().iMoveCount.ToString();
    }

    public void Update_Move_Count(int count)
    {
        T_Count.text = count.ToString();
    }
}