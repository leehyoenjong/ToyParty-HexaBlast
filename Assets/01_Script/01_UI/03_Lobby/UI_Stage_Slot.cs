using TMPro;
using UnityEngine;

public class UI_Stage_Slot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI T_Level;
    [SerializeField] GameObject G_Close;
    int CurLevel;
    System.Action<int> AC_Chose;

    public void Initialized(int level, System.Action<int> ac_chose)
    {
        CurLevel = level + 1;
        T_Level.text = CurLevel.ToString();
        AC_Chose = ac_chose;

        //!임시 - 스테이지 데이터 있는 것만 ON
        var stagedata = StageManager.Get_Stage_Data(CurLevel);
        G_Close.SetActive(stagedata == null);
    }

    public void Btn_Chose()
    {
        if (G_Close.activeSelf)
        {
            return;
        }
        AC_Chose?.Invoke(CurLevel);
    }
}
