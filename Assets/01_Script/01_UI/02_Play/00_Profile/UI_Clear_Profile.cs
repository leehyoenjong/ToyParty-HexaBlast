using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Clear_Profile : MonoBehaviour
{
    [SerializeField] Image Img_Clear_ICON;
    [SerializeField] TextMeshProUGUI T_Clear_Count;

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initailzed()
    {
        var stagedata = StageManager.Get_Stage_Data();;
        T_Clear_Count.text = stagedata.iClear_Count.ToString();
        Img_Clear_ICON.sprite = stagedata.Sp_Clear_ICON;
    }

    /// <summary>
    /// 클리어 횟수 표기
    /// </summary>
    /// <param name="count"></param>
    public void Update_Clear_Count(int count)
    {
        T_Clear_Count.text = count.ToString();
    }
}
