using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stage_Data", menuName = "ScriptableObjects/Stage Data")]
public class Stage_Data : ScriptableObject
{
    public int StageNum;
    public ST_Tile_Stage[] St_Tile_Stage;

    //ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ
    [Serializable]
    public struct ST_Tile_Stage
    {
        [Header("줄 갯수")]
        public int Grid_Slot_Count;

        [Header("확정 생성 타일")]
        public List<ST_Tile_Direct> St_Tile_Directs;
    }

    [Serializable]
    public struct ST_Tile_Direct
    {
        public GameObject G_Tile;
        public int Index;
    }
}