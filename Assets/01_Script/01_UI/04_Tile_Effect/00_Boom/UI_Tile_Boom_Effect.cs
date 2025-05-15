using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UI_Tile_Boom_Effect : MonoBehaviour
{
    [SerializeField] RectTransform Rt_Rect;
    [SerializeField] Image Img_ICON;

    /// <summary>
    /// 초기화 
    /// </summary>
    public void Initailized(Vector2 createpos)
    {
        Rt_Rect.anchoredPosition = createpos;
        Color c = Img_ICON.color;
        c.a = 0f;
        Img_ICON.color = c;
        // 페이드 인
        Img_ICON.DOFade(1f, 0.25f)
            .OnComplete(() =>
            {
                // 페이드 아웃
                Img_ICON.DOFade(0f, 0.25f)
                    .OnComplete(() => Destroy(this.gameObject));
            });
    }
}