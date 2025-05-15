using System.Collections.Generic;
using UnityEngine;

public class UI_Tile_UFO : UI_Tile
{
    public override E_Tile_Color Set_Tile_Color(E_Tile_Color color)
    {
        return base.Set_Tile_Color(E_Tile_Color.None);
    }

    public override void RemoveTile(UI_Tile_Slot tileslot)
    {
        var list = TileManager.instance.Get_Tile_Slot;
        var removelist = new HashSet<UI_Tile_Slot>();

        //순회하며 같은 색 찾기
        foreach (var item in list)
        {
            if (item.GetTile == null || item.GetTile == this)
            {
                continue;
            }

            //다른 색 타일은 넘기기
            if (item.GetTile.Get_Tile_Color() != tileslot.GetTile.Get_Tile_Color())
            {
                continue;
            }

            removelist.Add(item);
        }

        //삭제 목록 제거 
        foreach (var item in removelist)
        {
            if (item.GetTile == null)
            {
                continue;
            }
            item.RemoveTile(item);
        }
        base.RemoveTile(tileslot);
    }
}
