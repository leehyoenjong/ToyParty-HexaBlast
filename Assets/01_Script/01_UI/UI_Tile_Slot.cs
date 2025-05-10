using UnityEngine;

public class UI_Tile_Slot : MonoBehaviour
{
    [SerializeField] RectTransform Rt_Slot;

    public Vector2 GetPos => Rt_Slot.anchoredPosition;
    public RectTransform GetRect => Rt_Slot;

    UI_Tile Ui_Tile;

    /// <summary>
    /// 초기화
    /// </summary>
    /// <param name="createpos"></param>
    public void Initailzed(Vector2 createpos)
    {
        Rt_Slot.anchoredPosition = createpos;
    }

    /// <summary>
    /// 타일이 있는지 체크
    /// </summary>
    public bool CheckTile() => Ui_Tile != null;

    /// <summary>
    /// 타일 설정 
    /// </summary>
    public void SetTile(UI_Tile uI_Tile)
    {
        Ui_Tile = uI_Tile;
    }
}