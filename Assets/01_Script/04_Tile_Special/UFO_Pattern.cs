using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UFO_Pattern : IPatternRecognizer
{
    public bool IsMatch(HashSet<UI_Tile_Slot> slotlist)
    {
        if (slotlist.Count != 5)
        {
            return false;
        }

        //수직
        if (Check_UpDown(slotlist))
        {
            return true;
        }
        //왼쪽 대각선 
        else if (Check_Left_Diagonal(slotlist))
        {
            return true;
        }
        //오른쪽 대각선
        else if (Check_Right_Diagonal(slotlist))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 수직 
    /// </summary>
    bool Check_UpDown(HashSet<UI_Tile_Slot> slotlist)
    {

        //x값 가져오기
        var x_list = slotlist.Select(x => x.GetPoint.Item1).ToList();

        //모든 삭제되는 타일의 x값이 같을 경우 
        var check_x = x_list.TrueForAll(x => x == x_list[0]);
        if (check_x)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 대각선 왼쪽
    /// </summary>
    bool Check_Left_Diagonal(HashSet<UI_Tile_Slot> slotlist)
    {
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

    /// <summary>
    /// 대각선 오른쪽
    /// </summary>
    bool Check_Right_Diagonal(HashSet<UI_Tile_Slot> slotlist)
    {
        //x값이 작은 값순으로 나열 후 y값만 가져오기 
        var list = slotlist.OrderBy(x => x.GetPoint.Item1).Select(x => x.GetPoint.Item2).ToList();

        for (int i = 0; i < list.Count; i++)
        {
            if (i + 1 >= list.Count)
            {
                break;
            }

            var cur_pos = list[i];
            var rightpos = list[i + 1];

            //현재 위치보다 오른쪽이 작으면 flase
            if (cur_pos > rightpos)
            {
                return false;
            }
        }

        return true;
    }


    public E_Tile_Destory_Type GetPatternType()
    {
        return E_Tile_Destory_Type.UFO;
    }
}