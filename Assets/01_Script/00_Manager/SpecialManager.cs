using System;
using System.Collections.Generic;
using UnityEngine;

public class SpecialManager : MonoBehaviour
{
    public static SpecialManager instance;
    [SerializeField] List<St_Special_Tile> L_Special_Tile = new List<St_Special_Tile>();

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 삭제 갯수와 종류에 맞는 타일 가져오기 
    /// </summary>
    public GameObject Get_Spceical_Type(int destorycount, E_Tile_Destory_Type types)
    {
        return L_Special_Tile.Find(x => x.DestoryCount == destorycount && x.DestoryType == types).G_Tile;
    }

    /// <summary>
    /// 삭제되는 타일을 이용해 직,대각선인지 체크 
    /// </summary>
    public E_Tile_Destory_Type Get_Destory_Types(HashSet<UI_Tile_Slot> slotlist)
    {
        // 모든 슬롯의 좌표를 리스트로 변환
        var points = new List<(float, float)>();
        foreach (var slot in slotlist)
        {
            points.Add(slot.GetPoint);
        }

        // x좌표가 모두 같으면 세로 직선
        bool allSameX = points.TrueForAll(p => Mathf.Approximately(p.Item1, points[0].Item1));
        // y좌표가 모두 같으면 가로 직선
        bool allSameY = points.TrueForAll(p => Mathf.Approximately(p.Item2, points[0].Item2));

        if (allSameX || allSameY)
        {
            return E_Tile_Destory_Type.Beeline;
        }

        return E_Tile_Destory_Type.Diagonal;
    }
}

[Serializable]
public struct St_Special_Tile
{
    public int DestoryCount;
    public E_Tile_Destory_Type DestoryType;
    public GameObject G_Tile;
}