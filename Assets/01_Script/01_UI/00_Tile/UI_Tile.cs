using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using System.Collections;
using JetBrains.Annotations;

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

    public void Initailzed(UI_Tile_Slot slot)
    {
        Rt_Rect.anchoredPosition = slot.GetRect.anchoredPosition;
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
    public void Set_Swap(UI_Tile tile)
    {
        var tempslot = Ui_Tile_Slot;

        //슬롯 위치 변경
        Set_Tile_Slot(tile == null ? null : tile.Get_Tile_Slot);
        tile.Set_Tile_Slot(tempslot);

        //적용 타일 변경 
        tile.Get_Tile_Slot.SetTile(tile);
        Get_Tile_Slot.SetTile(this);
    }

    public void Set_Tile_Slot(UI_Tile_Slot slot)
    {
        Ui_Tile_Slot = slot;
    }

    /// <summary>
    /// 타일 이동함수 
    /// </summary>
    public IEnumerator Move_Tile(List<UI_Tile_Slot> movelist, UI_Tile_Slot result_slot)
    {
        // 타일을 최상위 캔버스의 자식으로 이동 (가시성 확보)
        b_Is_Moving = false;

        foreach (var item in movelist)
        {
            var move = false;
            // DOTween 애니메이션 시작 (월드 좌표로 이동)
            Rt_Rect.DOMove(item.GetRect.position, F_Move_Duration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true) // UpdateType.Normal로 설정하여 Time.timeScale 영향받지 않도록
                .OnComplete(() =>
                {
                    // 상태 초기화
                    Rt_Rect.localPosition = item.GetPos;
                    move = true;
                });

            yield return new WaitUntil(() => move);
        }


        //최종 도착지
        Rt_Rect.DOMove(result_slot.GetRect.position, F_Move_Duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true) // UpdateType.Normal로 설정하여 Time.timeScale 영향받지 않도록
            .OnComplete(() =>
            {
                Rt_Rect.localPosition = result_slot.GetPos;
                b_Is_Moving = true;
            });
    }

    public IEnumerator Move_Tile(UI_Tile_Slot result_slot)
    {
        // 타일을 최상위 캔버스의 자식으로 이동 (가시성 확보)
        b_Is_Moving = false;

        //최종 도착지
        Tw_Move = Rt_Rect.DOMove(result_slot.GetRect.position, F_Move_Duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Rt_Rect.localPosition = result_slot.GetPos;
                b_Is_Moving = true;
            });

        yield return new WaitUntil(() => b_Is_Moving);
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