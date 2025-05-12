using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI")]
    [SerializeField] UI_Score_Profile Ui_Score_Profile;
    public UI_Score_Profile GetUI_Score_Profile() => Ui_Score_Profile;

    [Header("획득할 점수")]
    [SerializeField] int Score;

    int CurScore;

    void Awake()
    {
        instance = this;
    }

    public void Initailized()
    {
        GetUI_Score_Profile().Initailized();
    }

    /// <summary>
    /// 점수 업데이트 처리
    /// </summary>
    /// <param name="tilecount"></param>
    public void Update_Score(int tilecount)
    {
        CurScore += tilecount * Score;
        GetUI_Score_Profile().Update_Score(CurScore);
    }
}