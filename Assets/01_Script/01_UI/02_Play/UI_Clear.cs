using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Clear : MonoBehaviour
{
    [Header("성공")]
    [SerializeField] GameObject G_Sucess;
    [SerializeField] TextMeshProUGUI T_Scuess_Level;
    [SerializeField] TextMeshProUGUI T_Sucess_Score;
    [SerializeField] Image Img_Medal;
    [SerializeField] GameObject[] G_SucessBtn;

    [Header("실패")]
    [SerializeField] GameObject G_Faild;
    [SerializeField] Image Img_Faild_Clear;

    [Header("실패 팝업")]
    [SerializeField] GameObject G_Faild_Popup;
    [SerializeField] TextMeshProUGUI T_FaildPopup_Level;

    string str_Level = "Level {0}";

    /// <summary>
    /// 실패처리
    /// </summary>
    public void Set_Faild()
    {
        this.gameObject.SetActive(true);
        G_Faild.SetActive(true);
        G_Sucess.SetActive(false);

        Img_Faild_Clear.sprite = StageManager.Get_Stage_Data().Sp_Clear_ICON;
    }

    /// <summary>
    /// 성공처리
    /// </summary>
    public void Set_Sucess()
    {
        this.gameObject.SetActive(true);
        var curstage = StageManager.Get_Stage_Data();
        G_Sucess.SetActive(true);
        G_Faild.SetActive(false);

        //레벨 표기 
        T_Scuess_Level.text = string.Format(str_Level, StageManager.Get_Stage_Data().StageNum.ToString());

        //점수 표시
        T_Sucess_Score.text = ScoreManager.instance.GetScore().ToString();

        //메달 색 표기
        Img_Medal.color = ScoreManager.instance.GetMedalColor();

        //다음 레벨 or 로비로 이동
        foreach (var item in G_SucessBtn)
        {
            item.SetActive(false);
        }
        var idx = StageManager.Check_MaxLevel(curstage.StageNum + 1) ? 1 : 0;
        G_SucessBtn[idx].SetActive(true);
    }


    /// <summary>
    /// 실패 시 이어하기 
    /// </summary>
    public void Btn_Faild_Continue()
    {
        this.gameObject.SetActive(false);
        ClearManager.instance.Update_Move_Count(5);
    }

    /// <summary>
    /// 실패 팝업창
    /// </summary>
    public void Btn_FaildPopup()
    {
        G_Faild.SetActive(false);
        G_Sucess.SetActive(false);
        G_Faild_Popup.SetActive(true);
        T_FaildPopup_Level.text = string.Format(str_Level, StageManager.Get_Stage_Data().StageNum.ToString());
    }

    /// <summary>
    /// 다음레벨 넘어가기
    /// </summary>
    public void Btn_NextLevel()
    {
        var curlevel = StageManager.Get_Stage_Data().StageNum;
        StageManager.Get_Stage_Data(curlevel + 1);
        SceneManager.LoadScene("01_Play");
    }

    /// <summary>
    /// 로비로 이동
    /// </summary>
    public void Btn_Exit()
    {
        SceneManager.LoadScene("00_Lobby");
    }

    /// <summary>
    /// 로비로 이동
    /// </summary>
    public void Btn_RePlay()
    {
        SceneManager.LoadScene("01_Play");
    }
}
