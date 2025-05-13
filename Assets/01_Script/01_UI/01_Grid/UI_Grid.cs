using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class UI_Grid : MonoBehaviour
{
    [Header("타일")]
    [SerializeField] Transform Tr_Grid;

    [Header("프리팹")]
    [SerializeField] GameObject G_Tile_Slot;

    int CreateIDX = 0;

    /// <summary>
    /// 타일 슬롯 생성
    /// </summary>
    public void Create_Tile_Slot()
    {
        //타일 사이즈
        var rt = G_Tile_Slot.GetComponent<RectTransform>();
        var width = rt.rect.width;
        var height = rt.rect.width;

        var stagedata = StageManager.Get_Stage_Data();

        //최대 갯수 
        var max = stagedata.St_Tile_Stage.Length;

        //중앙값 가져오기 
        int middlenum_x = max / 2;

        for (int i = 0; i < max; i++)
        {
            var curstage = stagedata.St_Tile_Stage[i];

            //행 갯수에 다른 중앙값 처리 
            var maxy = curstage.Grid_Slot_Count;
            int middlenum_y = maxy / 2;

            var directtile = curstage.St_Tile_Directs;

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
                slot.Initailzed(pos, (x * 0.01f, y * 0.01f));
                slot.gameObject.name = $"Slot_{x * 0.01f}_{y * 0.01f}";
                Debug.Log($"생성위치 :{x * 0.01f},{y * 0.01f}");

                //강제 타일 생성
                if (directtile.Count > 0)
                {
                    //번호 맞는지 체크
                    var idx = directtile.FindIndex(x => x.Index == j);
                    if (idx == -1)
                    {
                        continue;
                    }

                    //타일 생성
                    var g_tile = directtile[idx].G_Tile;
                    CreateTile(slot, g_tile);
                }
            }
        }

        //높이가 제일 높은 슬롯정보 담기
        var slots = TileManager.instance.Get_Tile_Slot;
        if (slots.Count > 0)
        {
            float maxY = slots.Max(x => x.GetRect.anchoredPosition.y);
            var hightslot = slots.Where(x => Mathf.Approximately(x.GetRect.anchoredPosition.y, maxY));
            foreach (var item in hightslot)
            {
                TileManager.instance.Get_Hight_Point.Add(item);
            }
        }

        //타일 생성
        Create_Tile_List();
    }

    /// <summary>
    /// 기본 타일 생성
    /// </summary>
    void Create_Tile_List()
    {
        //슬롯 리스트 가져오기 
        var slotlist = TileManager.instance.Get_Tile_Slot;
        var max = slotlist.Count;

        for (int i = 0; i < max; i++)
        {
            var item = slotlist[i];

            //이미 타일 생성되어 있는지 체크
            if (item.CheckTile())
            {
                continue;
            }

            //기본 타일 랜덤생성
            CreateTile(item, TileManager.instance.Get_Tile_Basic_Random_Start());
        }
    }

    /// <summary>
    /// 타일 생성 
    /// </summary>
    void CreateTile(UI_Tile_Slot slot, GameObject g_Tile)
    {
        //타일 생성
        var tile = Instantiate(g_Tile, slot.GetRect).GetComponent<UI_Tile>();
        TileManager.instance.Get_Tile.Add(tile);

        tile.Initailzed(slot);
        slot.SetTile(tile);
    }

    /// <summary>
    /// 비어있는 타일에 생성하기 
    /// </summary>
    public bool Create_None_Slot_Tile()
    {
        var tileslot = TileManager.instance.Get_Tile_Slot;
        var hightslot = TileManager.instance.Get_Hight_Point;

        var max = tileslot.Count;
        for (int i = 0; i < max; i++)
        {
            if (tileslot[i].GetTile != null)
            {
                continue;
            }
            if (CreateIDX >= hightslot.Count)
            {
                CreateIDX = 0;
            }

            if (hightslot[CreateIDX].GetTile != null)
            {
                return true;
            }

            //기본 타일 랜덤생성
            CreateTile(hightslot[CreateIDX], TileManager.instance.Get_Tile_Basic_Random());
            CreateIDX++;
            return true;
        }

        return false;
    }
}