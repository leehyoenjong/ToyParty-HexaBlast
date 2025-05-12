using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Play_Popup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI T_Level;
    [SerializeField] Image Img_Clear_Item;
    [SerializeField] TextMeshProUGUI T_Clear_Count;
    int CurLevel;
    public void Initialized(int level)
    {
        //스테이지 정보 추가 
        CurLevel = level;
        StageManager.Get_Stage_Data(CurLevel);

        //레벨표기
        T_Level.text = CurLevel.ToString();

        //클리어 정보 입력
        var stagedata = StageManager.Get_Stage_Data();
        Img_Clear_Item.sprite = stagedata.Sp_Clear_ICON;
        T_Clear_Count.text = stagedata.iClear_Count.ToString();
    }

    public void Btn_Play()
    {

        SceneManager.LoadScene("01_Play");
    }

    public void Btn_Exit()
    {
        Destroy(this.gameObject);
    }
}