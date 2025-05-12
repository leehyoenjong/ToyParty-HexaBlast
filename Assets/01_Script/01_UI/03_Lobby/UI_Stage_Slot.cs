using TMPro;
using UnityEngine;

public class UI_Stage_Slot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI T_Level;
    int CurLevel;
    System.Action<int> AC_Chose;

    public void Initialized(int level, System.Action<int> ac_chose)
    {
        CurLevel = level + 1;
        T_Level.text = CurLevel.ToString();
        AC_Chose = ac_chose;
    }

    public void Btn_Chose()
    {
        AC_Chose?.Invoke(CurLevel);
    }
}
