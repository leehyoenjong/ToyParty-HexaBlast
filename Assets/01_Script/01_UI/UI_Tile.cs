using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Tile : MonoBehaviour
{
    [SerializeField] RectTransform Rt_Rect;
    [SerializeField] EventTrigger Et_EventTrigger;
    UI_Tile_Slot Ui_Tile_Slot;
    
    public UI_Tile_Slot Get_Tile_Slot => Ui_Tile_Slot;
    
    public void Initailzed(UI_Tile_Slot slot)
    {
        Ui_Tile_Slot = slot;
        SetupEventTrigger();
    }

    /// <summary>
    /// 부모위치 변경
    /// </summary>
    public void Set_Swap(UI_Tile_Slot slot)
    {
        Ui_Tile_Slot = slot;
        Rt_Rect.SetParent(Ui_Tile_Slot.GetRect);
        Rt_Rect.localPosition = Vector3.zero;
    }

    private void SetupEventTrigger()
    {
        // 이벤트 트리거 엔트리 목록 초기화
        if (Et_EventTrigger.triggers == null)
            Et_EventTrigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();
        else
            Et_EventTrigger.triggers.Clear(); // 기존 트리거 제거

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

    private void OnPointerDownDelegate(PointerEventData data)
    {
        // 첫 번째 타일 선택
        TouchManasger.instance.OnTileDown(this);
    }

    private void OnPointerEnterDelegate(PointerEventData data)
    {
        // 드래그 중 타일 선택 (두 번째 타일)
        TouchManasger.instance.OnTileEnter(this);
    }
}