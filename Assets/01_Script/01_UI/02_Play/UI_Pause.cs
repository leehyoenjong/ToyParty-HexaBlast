using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Pause : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI T_Level;
    [SerializeField] GameObject[] G_Hint;

    private void Start()
    {
        T_Level.text = string.Format("LEVEL {0}", StageManager.Get_Stage_Data().StageNum);
        Time.timeScale = 0;
    }

    /// <summary>
    /// 계속하기
    /// </summary>
    public void Btn_Continue()
    {
        Time.timeScale = 1;
        Destroy(this.gameObject);
    }

    /// <summary>
    /// 다시하기
    /// </summary>
    public void Btn_RePlay()
    {
        SceneManager.LoadScene("01_Play");
    }

    /// <summary>
    /// 종료
    /// </summary>
    public void Btn_End()
    {
        SceneManager.LoadScene("00_Lobby");
    }

    public void Btn_Hint()
    {
        var on = G_Hint[0].activeSelf;
        G_Hint[0].SetActive(!on);
        G_Hint[1].SetActive(on);
    }
}