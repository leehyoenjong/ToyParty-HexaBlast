using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Score_Profile : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI T_Score;
    [SerializeField] RectTransform[] Rt_Medal;
    [SerializeField] RectTransform[] Rt_MedalPoint;
    [SerializeField] Image Img_Gage;
    [SerializeField] RectTransform Rt_GageParent; // 게이지의 부모 RectTransform
    [SerializeField] bool useParentSpace = true; // 부모 공간 기준으로 계산할지 여부

    // 디버깅 및 수동 조정용 변수
    [SerializeField] bool useDebugMode = false; // 디버그 모드 활성화
    [SerializeField] float manualOffsetX = 0f; // 수동 X축 오프셋
    [SerializeField] float manualScaleX = 1f; // 수동 X축 배율
    [SerializeField] bool useAbsolutePosition = true; // 절대 위치 사용 여부
    [SerializeField] bool centerMedals = true; // 메달을 중앙에 배치할지 여부

    public void Initailized()
    {
        var stagedata = PlayManager.instance.Get_Stage_Data();
        //메달 위치 셋팅
        var max = stagedata.iMaxScore;
        var medal = stagedata.iMedalScore;

        // 게이지의 부모가 없으면 자동으로 할당
        if (Rt_GageParent == null)
        {
            Rt_GageParent = Img_Gage.transform.parent.GetComponent<RectTransform>();
        }

        // 게이지 RectTransform 가져오기
        RectTransform gageRect = Img_Gage.rectTransform;
        RectTransform parentRect = Rt_GageParent;

        // 게이지와 메달의 캔버스 기준 위치를 계산
        Canvas canvas = GetComponentInParent<Canvas>();

        if (useAbsolutePosition && canvas != null)
        {
            // 게이지의 월드 위치 (캔버스 공간)
            Vector3[] gageCorners = new Vector3[4]; // 0:좌하단, 1:좌상단, 2:우상단, 3:우하단
            gageRect.GetWorldCorners(gageCorners);

            // 게이지의 실제 왼쪽 가장자리와 너비 (월드 좌표계)
            float gageLeftEdge = gageCorners[0].x;
            float gageWidth = gageCorners[3].x - gageCorners[0].x;

            if (useDebugMode)
            {
                Debug.Log($"Gage Left: {gageLeftEdge}, Width: {gageWidth}");
                for (int i = 0; i < 4; i++)
                {
                    Debug.Log($"Gage Corner {i}: {gageCorners[i]}");
                }
            }

            for (int i = 0; i < medal.Length; i++)
            {
                var per = (float)medal[i] / (float)max;

                // 메달 위치 계산 (월드 공간)
                float worldX = gageLeftEdge + (gageWidth * per * manualScaleX) + manualOffsetX;

                // 메달 위치 설정 (월드 좌표계에서 로컬 좌표계로 변환)
                if (i < Rt_Medal.Length)
                {
                    SetWorldPositionX(Rt_Medal[i], worldX, centerMedals);
                }

                // 메달 포인트 위치 설정
                if (i < Rt_MedalPoint.Length)
                {
                    SetWorldPositionX(Rt_MedalPoint[i], worldX, centerMedals);
                }
            }
        }
        else
        {
            // 기존 방식 (로컬 좌표계)
            // 부모 공간에서의 게이지 위치 계산
            float parentWidth = parentRect.rect.width;
            float gageWidth = gageRect.rect.width;

            // 게이지의 앵커 위치 (부모 기준)
            Vector2 anchorMin = gageRect.anchorMin;
            Vector2 anchorMax = gageRect.anchorMax;
            Vector2 pivot = gageRect.pivot;

            // 게이지의 왼쪽 가장자리 계산
            float anchorWidth = (anchorMax.x - anchorMin.x) * parentWidth;
            float leftAnchorPos = anchorMin.x * parentWidth;
            float pivotOffset = pivot.x * gageRect.sizeDelta.x;
            float leftEdge = leftAnchorPos + gageRect.anchoredPosition.x - pivotOffset;

            if (useDebugMode)
            {
                Debug.Log($"Anchor Width: {anchorWidth}, Left Anchor: {leftAnchorPos}");
                Debug.Log($"Pivot Offset: {pivotOffset}, Left Edge: {leftEdge}");
            }

            for (int i = 0; i < medal.Length; i++)
            {
                var per = (float)medal[i] / (float)max;

                // 메달 위치 계산
                float medalX;

                if (useParentSpace)
                {
                    // 부모 공간 기준으로 계산
                    medalX = leftEdge + (gageWidth * per * manualScaleX) + manualOffsetX;
                }
                else
                {
                    // 게이지 자체 공간 기준으로 계산
                    medalX = leftEdge + (anchorWidth * per * manualScaleX) + manualOffsetX;
                }

                // 메달 위치 설정 (x값만 변경)
                if (i < Rt_Medal.Length)
                {
                    Vector2 medalPos = Rt_Medal[i].anchoredPosition;

                    // 중앙 위치로 조정
                    if (centerMedals)
                    {
                        medalX -= Rt_Medal[i].rect.width * 0.5f;
                    }

                    medalPos.x = medalX;
                    Rt_Medal[i].anchoredPosition = medalPos;
                }

                // 메달 포인트 위치 설정 (x값만 변경)
                if (i < Rt_MedalPoint.Length)
                {
                    Vector2 medalPointPos = Rt_MedalPoint[i].anchoredPosition;

                    // 중앙 위치로 조정
                    if (centerMedals)
                    {
                        medalX -= Rt_MedalPoint[i].rect.width * 0.5f;
                    }

                    medalPointPos.x = medalX;
                    Rt_MedalPoint[i].anchoredPosition = medalPointPos;
                }
            }
        }
    }

    // 월드 좌표계 X 위치를 설정하는 헬퍼 메소드
    private void SetWorldPositionX(RectTransform rectTransform, float worldX, bool centered = true)
    {
        // 현재 월드 위치 가져오기
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        float currentWorldX;

        if (centered)
        {
            // 중앙 위치로 설정
            float width = corners[3].x - corners[0].x;
            currentWorldX = corners[0].x + (width * 0.5f);
        }
        else
        {
            // 왼쪽 가장자리 위치로 설정
            currentWorldX = corners[0].x;
        }

        // 현재 로컬 위치
        Vector2 localPos = rectTransform.anchoredPosition;

        // 위치 차이만큼 로컬 위치 조정
        float offset = worldX - currentWorldX;
        localPos.x += offset;

        // 새 위치 설정
        rectTransform.anchoredPosition = localPos;
    }

    public void Update_Score(int score)
    {
        //점수 텍스트 
        T_Score.text = score.ToString();

        //게이지
        var stagedata = PlayManager.instance.Get_Stage_Data();
        Img_Gage.fillAmount = (float)score / (float)stagedata.iMaxScore;
    }
}