using System.Collections.Generic;
using System.Linq;

public class Diagonal_Left_Line_Pattern : IPatternRecognizer
{
    public bool IsMatch(HashSet<UI_Tile_Slot> slotlist)
    {
        if (slotlist.Count != 4)
        {
            return false;
        }

        //x값이 큰 순으로 나열 후 y값만 가져오기 
        var list = slotlist.OrderByDescending(x => x.GetPoint.Item1).Select(x => x.GetPoint.Item2).ToList();

        for (int i = 0; i < list.Count; i++)
        {
            if (i + 1 >= list.Count)
            {
                break;
            }

            var cur_pos = list[i];
            var rightpos = list[i + 1];

            //현재 내 위치보다 작으면 flase
            if (cur_pos > rightpos)
            {
                return false;
            }
        }

        return true;
    }

    public E_Tile_Destory_Type GetPatternType()
    {
        return E_Tile_Destory_Type.Diagonal_Left_Line;
    }
}