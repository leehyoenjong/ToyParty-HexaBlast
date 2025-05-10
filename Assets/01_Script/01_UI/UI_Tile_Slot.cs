using UnityEngine;

public class UI_Tile_Slot : MonoBehaviour
{
    [SerializeField] RectTransform Rt_Slot;

    public Vector2 GetPos => Rt_Slot.anchoredPosition;
    public RectTransform GetRect => Rt_Slot;

    public void Initailzed(Vector2 createpos)
    {
        Rt_Slot.anchoredPosition = createpos;
    }
}