using UnityEngine;
using DG.Tweening;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    [SerializeField] GameObject[] G_Tile_Boom_Effect;
    [SerializeField] Transform Tr_Parent;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 파괴 이펙트 생성
    /// </summary>
    /// <param name="color"></param>
    /// <param name="createpos"></param>
    public void Create_Boom(E_Tile_Color color, Vector2 createpos)
    {
        var effect = Instantiate(G_Tile_Boom_Effect[(int)color], Tr_Parent).GetComponent<UI_Tile_Boom_Effect>();
        effect.Initailized(createpos);
    }
}