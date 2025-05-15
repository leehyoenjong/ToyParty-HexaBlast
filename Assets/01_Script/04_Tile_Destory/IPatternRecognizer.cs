using System.Collections.Generic;

interface IPatternRecognizer
{
    bool IsMatch(HashSet<UI_Tile_Slot> slotlist);
    E_Tile_Destory_Type GetPatternType();
}