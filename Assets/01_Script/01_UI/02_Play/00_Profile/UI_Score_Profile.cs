using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UI_Score_Profile : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI T_Score;
    [SerializeField] RectTransform[] Rt_Medal;
    [SerializeField] RectTransform[] Rt_MedalPoint;
    [SerializeField] Image Img_Main_Madel;
    [SerializeField] Image Img_Gage;
    [SerializeField] RectTransform Rt_GageParent; // 게이지의 부모 RectTransform

    public void Initailized()
    {
        var stagedata = StageManager.Get_Stage_Data();
        //메달 위치 셋팅
        var max = stagedata.iMaxScore;
        var medal = stagedata.iMedalScore;

        // 게이지의 부모가 없으면 자동으로 할당
        if (Rt_GageParent == null)
        {
            Rt_GageParent = Img_Gage.transform.parent.GetComponent<RectTransform>();
        }

        // 게이지 RectTransform 가져오기
        RectTransform rect_gage = Img_Gage.rectTransform;

        // 게이지의 월드 위치 계산
        Vector3[] corners = new Vector3[4]; // 0:좌하단, 1:좌상단, 2:우상단, 3:우하단
        rect_gage.GetWorldCorners(corners);

        // 게이지의 왼쪽 가장자리와 너비
        float left_edge = corners[0].x;
        float width = corners[3].x - corners[0].x;

        // 메달 위치 설정
        for (int i = 0; i < medal.Length; i++)
        {
            float per = (float)medal[i] / (float)max;
            float pos_x = left_edge + (width * per);

            // 메달 위치 설정
            if (i < Rt_Medal.Length)
            {
                Set_Medal_Position(Rt_Medal[i], pos_x);
            }

            // 메달 포인트 위치 설정
            if (i < Rt_MedalPoint.Length)
            {
                Set_Medal_Position(Rt_MedalPoint[i], pos_x);
            }
        }
    }

    // 메달 위치 설정 (중앙 정렬)
    private void Set_Medal_Position(RectTransform rect, float world_x)
    {
        // 현재 월드 위치 가져오기
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        // 중앙 위치 계산
        float width = corners[3].x - corners[0].x;
        float current_x = corners[0].x + (width * 0.5f);

        // 위치 차이 계산
        float offset = world_x - current_x;

        // 현재 위치에 오프셋 적용
        Vector2 pos = rect.anchoredPosition;
        pos.x += offset;
        rect.anchoredPosition = pos;
    }

    public void Update_Score(int score)
    {
        //점수 텍스트 
        T_Score.text = score.ToString();

        //게이지
        var stagedata = StageManager.Get_Stage_Data();
        Img_Gage.fillAmount = (float)score / (float)stagedata.iMaxScore;

        //점수에 따른 메달 색 설정 - ScoreManager에서 색상 가져오기
        Img_Main_Madel.color = ScoreManager.instance.GetMedalColor();
    }
}