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
    [SerializeField] E_Tile_Destory_Type En_Tile_Destory_Type = E_Tile_Destory_Type.NONE;

    [Header("UI")]
    [SerializeField] protected Image Img_ICON;
    [SerializeField] RectTransform Rt_Rect;
    [SerializeField] EventTrigger Et_EventTrigger;

    [Header("애니메이션 설정")]
    [SerializeField] float F_Move_Duration = 0.15f;  // 이동 지속 시간 (0.025f에서 0.15f로 늘림)

    // 부모 슬롯
    UI_Tile_Slot Ui_Tile_Slot;
    public UI_Tile_Slot Get_Tile_Slot => Ui_Tile_Slot;

    //색
    public E_Tile_Color Get_Tile_Color() => En_Tile_Color;
    public virtual E_Tile_Color Set_Tile_Color(E_Tile_Color color) => En_Tile_Color = color;

    //종류
    public E_Tile_Kind Get_Tile_Kind() => En_Tile_Kind;

    //삭제 종류
    public E_Tile_Destory_Type Get_Tile_Destory_Type
    {
        get => En_Tile_Destory_Type;
        set => En_Tile_Destory_Type = value;
    }
    public virtual void Set_Tile_Destory_Type(HashSet<UI_Tile_Slot> value) { }

    public void Initailzed(UI_Tile_Slot slot)
    {
        Rt_Rect.anchoredPosition = slot.GetRect.anchoredPosition;
        Ui_Tile_Slot = slot;
        SetupEventTrigger();
    }


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

            while (!move)
            {
                yield return null;
            }
        }

        //최종 도착지
        Rt_Rect.DOMove(result_slot.GetRect.position, F_Move_Duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true) // UpdateType.Normal로 설정하여 Time.timeScale 영향받지 않도록
            .OnComplete(() =>
            {
                Rt_Rect.localPosition = result_slot.GetPos;
            });
    }

    public IEnumerator Move_Tile(UI_Tile_Slot result_slot)
    {
        // 타일을 최상위 캔버스의 자식으로 이동 (가시성 확보)
        var b_moving = false;

        //최종 도착지
        Rt_Rect.DOMove(result_slot.GetRect.position, F_Move_Duration)
        .SetEase(Ease.OutQuad)
        .SetUpdate(true)
        .OnComplete(() =>
        {
            Rt_Rect.localPosition = result_slot.GetPos;
            b_moving = true;
        });

        while (!b_moving)
        {
            yield return null;
        }
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
        // 첫 번째 타일 선택
        TouchManasger.instance.OnTileDown(this);
    }

    void OnPointerEnterDelegate(PointerEventData data)
    {
        // 드래그 중 타일 선택 (두 번째 타일)
        TouchManasger.instance.OnTileEnter(this);
    }

    /// <summary>
    /// 타일제거
    /// </summary>
    public virtual void RemoveTile(UI_Tile removetile)
    {
        TileManager.instance.Destory_Tile_Count(this);

        if (this == null || this.gameObject == null)
        {
            return;
        }
        Destroy(this.gameObject);
    }

    /// <summary>
    /// 삭제해도 되는지 체크
    /// </summary>
    public virtual bool Check_Remove(UI_Tile removetile)
    {
        return true;
    }
}