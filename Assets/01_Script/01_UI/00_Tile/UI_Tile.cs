using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UI_Tile : MonoBehaviour
{

    [Header("타일 정보")]
    [SerializeField] E_Tile_Color En_Tile_Color;
    [SerializeField] E_Tile_Kind En_Tile_Kind;

    [Header("UI")]
    [SerializeField] RectTransform Rt_Rect;
    [SerializeField] EventTrigger Et_EventTrigger;

    [Header("애니메이션 설정")]
    [SerializeField] float F_Move_Speed = 30f;      // 이동 속도
    [SerializeField] float F_Min_Move_Distance = 0.01f; // 최소 이동 거리
    [SerializeField] float F_Move_Duration = 0.15f;  // 이동 지속 시간 (0.025f에서 0.15f로 늘림)
    [SerializeField] Ease E_Move_Ease = Ease.Linear; // 이동 이징 효과 (InOutSine에서 Linear로 변경)

    // 부모 슬롯
    UI_Tile_Slot Ui_Tile_Slot;
    public UI_Tile_Slot Get_Tile_Slot => Ui_Tile_Slot;

    // 애니메이션 관련 변수
    bool b_Is_Moving = false;
    Tween Tw_Move;

    // 애니메이션 중 임시 저장 변수
    Transform Tr_Original_Parent;
    Vector3 Vc_Original_LocalPosition;
    Vector3 Vc_Target_World_Position;

    public void Initailzed(UI_Tile_Slot slot)
    {
        Ui_Tile_Slot = slot;
        SetupEventTrigger();
    }

    /// <summary>
    /// 종류 및 색깔 가져오기
    /// </summary>
    /// <returns></returns>
    public E_Tile_Color Get_Tile_Color() => En_Tile_Color;
    public E_Tile_Kind Get_Tile_Kind() => En_Tile_Kind;

    /// <summary>
    /// 부모위치 변경
    /// </summary>
    /// <param name="slot">이동할 타일 슬롯</param>
    /// <param name="withAnimation">애니메이션 적용 여부 (기본값: true)</param>
    /// <param name="isDiagonal">대각선 이동 여부 (기본값: false)</param>
    public void Set_Swap(UI_Tile_Slot slot, bool withAnimation = true, bool isDiagonal = false)
    {
        // 이동 중이면 중복 호출 방지
        if (b_Is_Moving)
        {
            return;
        }

        // 이미 실행 중인 애니메이션이 있다면 중지
        if (Tw_Move != null && Tw_Move.IsActive())
        {
            Tw_Move.Kill(false); // 현재 값으로 중지
        }

        // 슬롯에 타일 연결 (항상 즉시 설정)
        Ui_Tile_Slot = slot;
        slot.SetTile(this);

        // 애니메이션을 사용하지 않는 경우 즉시 이동
        if (!withAnimation)
        {
            Rt_Rect.SetParent(slot.GetRect);
            Rt_Rect.localPosition = Vector3.zero;
            return;
        }

        // 현재 월드 위치와 목표 슬롯의 월드 위치 계산
        Vector3 currentWorldPos = Rt_Rect.position;
        Vc_Target_World_Position = slot.GetRect.position;

        // 이동 거리가 너무 작으면 즉시 이동 (작은 조정은 애니메이션 없이)
        float distance = Vector3.Distance(currentWorldPos, Vc_Target_World_Position);
        if (distance < F_Min_Move_Distance)
        {
            Rt_Rect.SetParent(slot.GetRect);
            Rt_Rect.localPosition = Vector3.zero;
            return;
        }

        // 임시 저장: 원래 부모와 로컬 위치
        Tr_Original_Parent = Rt_Rect.parent;
        Vc_Original_LocalPosition = Rt_Rect.localPosition;

        // 이동 시간 계산
        // 대각선 이동일 경우 더 빠르게 처리
        float duration = isDiagonal
            ? F_Move_Duration * 0.35f  // 대각선 이동 속도를 2배 더 빠르게 (0.7f → 0.35f)
            : F_Move_Duration;

        // 애니메이션 시작 전 상태 설정
        b_Is_Moving = true;

        // 타일을 최상위 캔버스의 자식으로 이동 (가시성 확보)
        Rt_Rect.SetParent(PlayManager.instance.GetUI_Play().GetCanvas().transform, true); // 월드 위치 유지
        Rt_Rect.SetAsLastSibling(); // 가장 위에 표시

        // 대각선 이동을 위한 이징 설정
        Ease easeToUse = isDiagonal ? Ease.OutQuad : E_Move_Ease;

        // DOTween 애니메이션 시작 (월드 좌표로 이동)
        Tw_Move = Rt_Rect.DOMove(Vc_Target_World_Position, duration)
            .SetEase(easeToUse)
            .SetUpdate(true) // UpdateType.Normal로 설정하여 Time.timeScale 영향받지 않도록
            .OnComplete(() =>
            {
                try
                {
                    // 애니메이션 완료 후 최종 위치로 설정
                    if (slot != null && slot.GetRect != null)
                    {
                        Rt_Rect.SetParent(slot.GetRect);
                        Rt_Rect.localPosition = Vector3.zero;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"타일 애니메이션 완료 오류: {ex.Message}");
                }
                finally
                {
                    // 상태 초기화
                    b_Is_Moving = false;
                    Tw_Move = null;
                }
            });
    }


    /// <summary>
    /// 타일이 이동 중인지 여부 확인
    /// </summary>
    public bool Check_Is_Moving()
    {
        // DOTween 상태와 내부 플래그 모두 확인
        bool isTweenActive = Tw_Move != null && Tw_Move.IsActive();

        // 디버그 로그로 상태 확인
        if (isTweenActive != b_Is_Moving)
        {
            Debug.LogWarning($"{name}: 이동 상태 불일치 - 내부:{b_Is_Moving}, Tween:{isTweenActive}");
            b_Is_Moving = isTweenActive; // 동기화
        }

        return b_Is_Moving;
    }

    void SetupEventTrigger()
    {
        // 이벤트 트리거 엔트리 목록 초기화
        if (Et_EventTrigger.triggers == null)
        {
            Et_EventTrigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();
        }
        else
        {
            Et_EventTrigger.triggers.Clear(); // 기존 트리거 제거
        }

        // PointerDown 이벤트 추가 (터치/클릭 시작)
        EventTrigger.Entry entryPointerDown = new EventTrigger.Entry();
        entryPointerDown.eventID = EventTriggerType.PointerDown;
        entryPointerDown.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
        Et_EventTrigger.triggers.Add(entryPointerDown);

        // PointerEnter 이벤트 추가 (드래그 중 진입)
        EventTrigger.Entry entryPointerEnter = new EventTrigger.Entry();
        entryPointerEnter.eventID = EventTriggerType.PointerEnter;
        entryPointerEnter.callback.AddListener((data) => { OnPointerEnterDelegate((PointerEventData)data); });
        Et_EventTrigger.triggers.Add(entryPointerEnter);
    }

    void OnPointerDownDelegate(PointerEventData data)
    {
        // 이동 중에는 터치를 무시
        if (Check_Is_Moving())
        {
            Debug.Log($"{name}: 이동 중이라 터치 무시");
            return;
        }

        // 첫 번째 타일 선택
        TouchManasger.instance.OnTileDown(this);
    }

    void OnPointerEnterDelegate(PointerEventData data)
    {
        // 이동 중에는 터치를 무시
        if (Check_Is_Moving())
        {
            Debug.Log($"{name}: 이동 중이라 진입 무시");
            return;
        }

        // 드래그 중 타일 선택 (두 번째 타일)
        TouchManasger.instance.OnTileEnter(this);
    }

    /// <summary>
    /// 타일제거
    /// </summary>
    public virtual void RemoveTile()
    {
        // 이미 실행 중인 애니메이션이 있다면 중지
        if (Tw_Move != null && Tw_Move.IsActive())
        {
            Tw_Move.Kill(false);
            Tw_Move = null;
        }

        TileManager.instance.Destory_Tile_Count(this);
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        // 게임 오브젝트가 제거될 때 모든 트윈 중지
        if (Tw_Move != null && Tw_Move.IsActive())
        {
            Tw_Move.Kill(false);
            Tw_Move = null;
        }
    }
}