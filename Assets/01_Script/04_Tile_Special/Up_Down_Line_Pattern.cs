using System.Collections.Generic;
using System.Linq;

public class Up_Down_Line_Pattern : IPatternRecognizer
{
    public bool IsMatch(HashSet<UI_Tile_Slot> slotlist)
    {
        if (slotlist.Count != 4)
        {
            return false;
        }

        //y값 모음
        var points = slotlist.Select(x => x.GetPoint.Item1).ToList();

        // y좌표가 같을 경우 
        bool check = points.TrueForAll(x => x == points[0]);

        // 여기에 패턴 일치 조건을 구현합니다.
        return check;
    }

    public E_Tile_Destory_Type GetPatternType()
    {
        return E_Tile_Destory_Type.Beeline_UpDown;
    }
}