using UnityEngine;
using UnityEngine.UI;

public class UI_Grid : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] Transform Tr_Grid; // 그리드 부모 오브젝트 (RectTransform)
    [SerializeField] GameObject G_Tile; // 타일 프리팹
    [SerializeField] int[] Grid_Slot_Count;

    void Start()
    {
        CreateHexGrid();
    }

    void CreateHexGrid()
    {
        //타일 사이즈
        var rt = G_Tile.GetComponent<RectTransform>();
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
                var tile = Instantiate(G_Tile, Tr_Grid).GetComponent<RectTransform>();

                // x값 계산: 중앙을 기준으로 -300, -200, ..., 0, ..., 200, 300
                float x = (i - middlenum_x) * width;
                float y = maxy % 2 == 1 ? (j - middlenum_y) : (middlenum_y - j - 0.5f);
                y *= height;
                var pos = new Vector2(x, y);

                tile.anchoredPosition = pos;
            }
        }
    }
}