using UnityEngine;

public class UI_Tile_Slot : MonoBehaviour
{
    [SerializeField] RectTransform Rt_Slot;

    public Vector2 GetPos => Rt_Slot.anchoredPosition;
    public RectTransform GetRect => Rt_Slot;

    UI_Tile Ui_Tile;
    public UI_Tile GetTile => Ui_Tile;

    (float, float) i_Point;
    public (float, float) GetPoint => i_Point;

    /// <summary>
    /// 초기화
    /// </summary>
    /// <param name="createpos"></param>
    public void Initailzed(Vector2 createpos, (float, float) point)
    {
        Rt_Slot.anchoredPosition = createpos;
        i_Point = point;
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

    /// <summary>
    /// 타일 삭제
    /// </summary>
    public void RemoveTile()
    {
        if (Ui_Tile != null)
        {
            Ui_Tile.RemoveTile();
            Ui_Tile = null;
        }
    }

    /// <summary>
    /// 제거
    /// </summary>
    public void Reset()
    {
        Ui_Tile = null;
    }
}