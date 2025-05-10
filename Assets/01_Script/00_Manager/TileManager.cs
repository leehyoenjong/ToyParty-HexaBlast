using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager instance;

    //타일 슬롯 리스트
    List<UI_Tile_Slot> L_Tile_Slot = new List<UI_Tile_Slot>();

    public List<UI_Tile_Slot> Get_Tile_Slot => L_Tile_Slot;

    //타일 리스트
    List<UI_Tile> L_Tile = new List<UI_Tile>();
    public List<UI_Tile> Get_Tile => L_Tile;


    private void Awake()
    {
        instance = this;
    }
}