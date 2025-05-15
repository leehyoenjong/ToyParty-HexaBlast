using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Tile_Diagonal : UI_Tile
{
    public override void Set_Tile_Destory_Type(HashSet<UI_Tile_Slot> value)
    {
        base.Set_Tile_Destory_Type(value);

        //키 값 작은 순서대로 정렬
        var list = value.OrderBy(x => x.GetPoint.Item1).ToList();

        //중간 두개의 값들을 가져와
        var left = list[1].GetPoint.Item2;
        var right = list[2].GetPoint.Item2;

        //왼쪽게 더 크면 왼쪽 대각선, 오른쪽이 더 크면 오른쪽 대각선
        Get_Tile_Destory_Type = left > right ? E_Tile_Destory_Type.Diagonal_Left_Line : E_Tile_Destory_Type.Diagonal_Right_Line;
    }

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
        var slotlist = TileManager.instance.Get_Tile_Slot;
        var removelist = new List<UI_Tile_Slot>();

        switch (Get_Tile_Destory_Type)
        {
            //오른쪽 대각선 전체 리스트
            case E_Tile_Destory_Type.Diagonal_Right_Line:
                //딕셔너리 형태로 x.GetPoint.Item1이 key값, List<UI_Tile_Slot>를 value값으로 하되, value값은 x.GetPoint.Item2가 작은거부터 나열 
                var dict = slotlist.GroupBy(x => x.GetPoint.Item1).ToDictionary(g => g.Key, g => g.OrderBy(x => x.GetPoint.Item2).ToList());

                //오른쪽 체크
                var max = dict.Max(x => x.Value.Max(x => x.GetPoint.Item1));
                var add_y = -pos.Item2;
                var prevCount = dict[(int)pos.Item1].Count;
                for (int i = (int)pos.Item1 + 1; i <= max; i++)
                {
                    var currCount = dict[i].Count;
                    bool prevEven = prevCount % 2 == 0;
                    bool currEven = currCount % 2 == 0;

                    //이전과 지금이 둘다 짝수거나 홀수면 1증가
                    if (prevEven == currEven)
                        add_y += 1f;
                    //이전이 짝수고 지금이 홀수이거나 또는 반대면 0.5증가
                    else
                        add_y += 0.5f;

                    foreach (var item in dict[i])
                    {
                        if (add_y > item.GetPoint.Item2)
                            continue;

                        removelist.Add(item);
                        break;
                    }
                    prevCount = currCount;
                }

                //왼쪽 체크
                dict = slotlist.GroupBy(x => x.GetPoint.Item1).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.GetPoint.Item2).ToList());
                var min = dict.Min(x => x.Value.Min(x => x.GetPoint.Item1));
                add_y = -pos.Item2;
                prevCount = dict[(int)pos.Item1].Count;
                for (int i = (int)pos.Item1 - 1; i >= min; i--)
                {
                    var currCount = dict[i].Count;
                    bool prevEven = prevCount % 2 == 0;
                    bool currEven = currCount % 2 == 0;

                    if (prevEven == currEven)
                        add_y -= 1f;
                    else
                        add_y -= 0.5f;

                    foreach (var item in dict[i])
                    {
                        if (add_y < item.GetPoint.Item2)
                            continue;

                        removelist.Add(item);
                        break;
                    }
                    prevCount = currCount;
                }
                break;
            //왼쪽 대각선 전체 리스트
            case E_Tile_Destory_Type.Diagonal_Left_Line:
                //딕셔너리 형태로 x.GetPoint.Item1이 key값, List<UI_Tile_Slot>를 value값으로 하되, value값은 x.GetPoint.Item2가 작은거부터 나열 
                dict = slotlist.GroupBy(x => x.GetPoint.Item1).ToDictionary(g => g.Key, g => g.OrderBy(x => x.GetPoint.Item2).ToList());

                //오른쪽 체크
                min = dict.Min(x => x.Value.Min(x => x.GetPoint.Item1));

                add_y = -pos.Item2;
                prevCount = dict[(int)pos.Item1].Count;
                for (int i = (int)pos.Item1 - 1; i >= min; i--)

                {
                    var currCount = dict[i].Count;
                    bool prevEven = prevCount % 2 == 0;
                    bool currEven = currCount % 2 == 0;

                    //이전과 지금이 둘다 짝수거나 홀수면 1증가
                    if (prevEven == currEven)
                        add_y += 1f;
                    //이전이 짝수고 지금이 홀수이거나 또는 반대면 0.5증가
                    else
                        add_y += 0.5f;

                    foreach (var item in dict[i])
                    {
                        if (add_y > item.GetPoint.Item2)
                            continue;

                        removelist.Add(item);
                        break;
                    }
                    prevCount = currCount;
                }

                //왼쪽 체크
                dict = slotlist.GroupBy(x => x.GetPoint.Item1).ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.GetPoint.Item2).ToList());
                add_y = -pos.Item2;
                prevCount = dict[(int)pos.Item1].Count;
                max = dict.Max(x => x.Value.Max(x => x.GetPoint.Item1));
                for (int i = (int)pos.Item1 + 1; i <= max; i++)
                {
                    var currCount = dict[i].Count;
                    bool prevEven = prevCount % 2 == 0;
                    bool currEven = currCount % 2 == 0;

                    if (prevEven == currEven)
                        add_y -= 1f;
                    else
                        add_y -= 0.5f;

                    foreach (var item in dict[i])
                    {
                        if (add_y < item.GetPoint.Item2)
                            continue;

                        removelist.Add(item);
                        break;
                    }
                    prevCount = currCount;
                }
                break;
        }

        //타입에 따른 타일제거
        foreach (var item in removelist)
        {
            item.RemoveTile(item);
        }
        base.RemoveTile(tileslot);
    }
}