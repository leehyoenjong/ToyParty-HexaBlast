using System;
using System.Collections.Generic;
using UnityEngine;

public class SpecialManager : MonoBehaviour
{
    public static SpecialManager instance;
    [SerializeField] List<St_Special_Tile> L_Special_Tile = new List<St_Special_Tile>();

    private List<IPatternRecognizer> patternRecognizers = new List<IPatternRecognizer>();

    void Awake()
    {
        instance = this;
        InitializePatternRecognizers();
    }

    private void InitializePatternRecognizers()
    {
        // 순서가 중요함 - 더 구체적인 패턴이 먼저 체크되어야 함
        patternRecognizers.Add(new Up_Down_Line_Pattern());
        patternRecognizers.Add(new Diagonal_Left_Pattern());
        patternRecognizers.Add(new Diagonal_Right_Pattern());
        // 추가 패턴 인식기들...
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
        // 각 패턴 인식기를 순서대로 시도
        foreach (var recognizer in patternRecognizers)
        {
            if (recognizer.IsMatch(slotlist))
            {
                return recognizer.GetPatternType();
            }
        }

        // 기본값 반환
        return E_Tile_Destory_Type.NONE;
    }
}

[Serializable]
public struct St_Special_Tile
{
    public int DestoryCount;
    public E_Tile_Destory_Type DestoryType;
    public GameObject G_Tile;
}