using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class UI_Tile_Line : UI_Tile
{
    public override E_Tile_Color Set_Tile_Color(E_Tile_Color color)
    {
        switch (color)
        {
            case E_Tile_Color.Red:
                Img_ICON.color = Color.red;
                break;
            case E_Tile_Color.Blue:
                Img_ICON.color = Color.blue;
                break;
            case E_Tile_Color.Yellow:
                Img_ICON.color = Color.yellow;
                break;
            case E_Tile_Color.Green:
                Img_ICON.color = Color.green;
                break;
        }

        return base.Set_Tile_Color(color);
    }

    public override void RemoveTile(UI_Tile_Slot tileslot)
    {
        var pos = Get_Tile_Slot.GetPoint;

        //y값이 작은 것 부터 정렬
        var slotlist = TileManager.instance.Get_Tile_Slot;
        var removelist = new List<UI_Tile_Slot>();

        switch (Get_Tile_Destory_Type)
        {
            //수직 리스트 
            case E_Tile_Destory_Type.Beeline_UpDown:
                removelist.AddRange(slotlist.FindAll(x => x != Get_Tile_Slot && x.GetPoint.Item1 == pos.Item1));
                break;
        }

        //타입에 따른 타일제거
        foreach (var item in removelist)
        {
            item.RemoveTile(tileslot);
        }
        base.RemoveTile(tileslot);
    }
}
