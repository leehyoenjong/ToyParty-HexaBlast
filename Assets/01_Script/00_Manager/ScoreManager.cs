using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI")]
    [SerializeField] UI_Score_Profile Ui_Score_Profile;
    public UI_Score_Profile GetUI_Score_Profile() => Ui_Score_Profile;

    [Header("획득할 점수")]
    [SerializeField] int Score;


    [Header("메달 색상 설정")]
    [SerializeField]
    Color[] Medal_Colors = new Color[3] {
        new Color(0.8f, 0.5f, 0.2f), // 동메달 색상
        new Color(0.75f, 0.75f, 0.75f), // 은메달 색상
        new Color(1.0f, 0.84f, 0.0f), // 금메달 색상
    };
    [SerializeField] Color Default_Medal_Color = Color.gray; // 기본 메달 색상

    int CurScore;
    public int GetScore() => CurScore;

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

    /// <summary>
    /// 점수에 따른 메달 색상 반환
    /// </summary>
    /// <param name="score">현재 점수</param>
    /// <param name="medalScores">메달 점수 배열</param>
    /// <returns>메달 색상</returns>
    public Color GetMedalColor()
    {
        // 현재 점수가 도달한 가장 높은 메달 등급 찾기
        int medalIndex = System.Array.FindLastIndex(StageManager.Get_Stage_Data().iMedalScore, m => CurScore >= m);

        // 메달 색상 반환
        if (medalIndex >= 0 && medalIndex < Medal_Colors.Length)
        {
            return Medal_Colors[medalIndex];
        }
        else
        {
            // 메달 획득 실패 시 기본 색상
            return Default_Medal_Color;
        }
    }
}