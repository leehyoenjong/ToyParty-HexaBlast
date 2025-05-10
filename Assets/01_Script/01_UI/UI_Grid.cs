using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Grid : MonoBehaviour
{
    [Header("타일")]
    [SerializeField] Transform Tr_Grid;

    [Header("프리팹")]
    [SerializeField] GameObject G_Tile_Slot;
    [SerializeField] GameObject G_Tile;

    [Header("타일 정보")]
    [SerializeField] int[] Grid_Slot_Count;

    void Start()
    {
        Create_Tile_Slot();
    }

    /// <summary>
    /// 타일 슬롯 생성
    /// </summary>
    void Create_Tile_Slot()
    {
        //타일 사이즈
        var rt = G_Tile_Slot.GetComponent<RectTransform>();
        var width = rt.rect.width;
        var height = rt.rect.width;

        //최대 갯수 
        var max = Grid_Slot_Count.Length;

        //중앙값 가져오기 
        int middlenum_x = max / 2;

        for (int i = 0; i < max; i++)
        {
            //행 갯수에 다른 중앙값 처리 
            var maxy = Grid_Slot_Count[i];
            int middlenum_y = maxy / 2;

            //타일 생성
            for (int j = 0; j < maxy; j++)
            {
                //슬롯 정보 저장
                var slot = Instantiate(G_Tile_Slot, Tr_Grid).GetComponent<UI_Tile_Slot>();
                TileManager.instance.Get_Tile_Slot.Add(slot);

                // x값 계산: 중앙을 기준으로 -300, -200, ..., 0, ..., 200, 300
                float x = (i - middlenum_x) * width;
                float y = maxy % 2 == 1 ? (j - middlenum_y) : (middlenum_y - j - 0.5f);
                y *= height;

                //위치 값 설정 및 초가화 작업
                var pos = new Vector2(x, y);
                slot.Initailzed(pos);
            }
        }

        //타일 생성
        Create_Tile();
    }

    /// <summary>
    /// 타일 생성
    /// </summary>
    void Create_Tile()
    {
        //슬롯 리스트 가져오기 
        var slotlist = TileManager.instance.Get_Tile_Slot;
        var max = slotlist.Count;

        for (int i = 0; i < max; i++)
        {
            var item = slotlist[i];

            //타일 생성
            var tile = Instantiate(G_Tile, item.GetRect).GetComponent<UI_Tile>();
            TileManager.instance.Get_Tile.Add(tile);

            tile.Initailzed(item);
        }
    }
}