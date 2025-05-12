using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stage_Data", menuName = "ScriptableObjects/Stage Data")]
public class Stage_Data : ScriptableObject
{
    [Header("스테이지 정보")]
    public int StageNum;
    public ST_Tile_Stage[] St_Tile_Stage;

    [Header("클리어 정보")]
    public Sprite Sp_Clear_ICON;
    public int iClear_Count;
    public E_Tile_Kind Clear_Kind;


    [Header("이동 횟수")]
    public int iMoveCount;

    [Header("최고점수")]
    public int iMaxScore;
    public int[] iMedalScore = new int[3];

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