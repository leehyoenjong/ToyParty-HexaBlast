using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager instance;

    //타일 슬롯 리스트
    List<UI_Tile_Slot> L_Tile_Slot = new List<UI_Tile_Slot>();
    public List<UI_Tile_Slot> Get_Tile_Slot => L_Tile_Slot;

    //타일 리스트
    List<UI_Tile> L_Tile = new List<UI_Tile>();
    public List<UI_Tile> Get_Tile => L_Tile;

    UI_Tile firstTouch_Tile;
    public UI_Tile FirstTouch_Tile
    {
        get => firstTouch_Tile;
        set => firstTouch_Tile = value;
    }

    UI_Tile secondTouch_Tile;
    public UI_Tile SecondTouch_Tile
    {
        get => secondTouch_Tile;
        set => secondTouch_Tile = value;
    }

    // 8방향 델타
    (int dx, int dy)[] i_Directions = new (int, int)[]
    {
        (0, 1),    // 위
        (1, 1),    // 오른쪽 위
        (1, 0),    // 오른쪽
        (1, -1),   // 오른쪽 아래
        (0, -1),   // 아래
        (-1, -1),  // 왼쪽 아래
        (-1, 0),   // 왼쪽
        (-1, 1),   // 왼쪽 위
    };

    [Header("기본 색 타일")]
    [SerializeField] GameObject[] G_Tile;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 한 슬롯을 기준으로 8방향(직선)으로 연속된 같은 색 타일 그룹을 반환
    /// </summary>
    public HashSet<UI_Tile_Slot> ScanLineGroup(UI_Tile_Slot start)
    {
        HashSet<UI_Tile_Slot> result = new HashSet<UI_Tile_Slot>();
        if (start == null || start.GetTile == null)
        {
            return result;
        }

        var color = start.GetTile.Get_Tile_Color();
        (int, float) startPoint = start.GetPoint;

        // 4방향(가로, 세로, 대각선2개)만 검사 (양방향 포함)
        (int dx, float dy)[] directions = new (int, float)[]
        {
            (1, 0f),   // 가로
            (0, 1f),   // 세로
            (1, 1f),   // 대각선 ↘
            (1, -1f),  // 대각선 ↗
        };

        foreach (var (dx, dy) in directions)
        {
            List<UI_Tile_Slot> line = new List<UI_Tile_Slot>();
            line.Add(start);

            // +방향
            int nx = startPoint.Item1 + dx;
            float ny = startPoint.Item2 + dy;
            while (true)
            {
                var neighbor = L_Tile_Slot.Find(slot => slot.GetPoint.Item1 == nx && Mathf.Approximately(slot.GetPoint.Item2, ny));
                if (neighbor == null || neighbor.GetTile == null)
                {
                    break;
                }
                if (neighbor.GetTile.Get_Tile_Color() != color)
                {
                    break;
                }
                line.Add(neighbor);
                nx += dx;
                ny += dy;
            }

            // -방향
            nx = startPoint.Item1 - dx;
            ny = startPoint.Item2 - dy;
            while (true)
            {
                var neighbor = L_Tile_Slot.Find(slot => slot.GetPoint.Item1 == nx && Mathf.Approximately(slot.GetPoint.Item2, ny));
                if (neighbor == null || neighbor.GetTile == null)
                {
                    break;
                }
                if (neighbor.GetTile.Get_Tile_Color() != color)
                {
                    break;
                }
                line.Add(neighbor);
                nx -= dx;
                ny -= dy;
            }

            // 3개 이상 연속이면 결과에 추가
            if (line.Count >= 3)
            {
                foreach (var slot in line)
                {
                    result.Add(slot);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 전체 슬롯에 담겨있는 타일이랑 같은 타일들이 나열되어 있는지 체크 (직선 연속만 인정)
    /// </summary>
    public void All_Scan()
    {
        HashSet<UI_Tile_Slot> removeSet = new HashSet<UI_Tile_Slot>();

        foreach (var item in L_Tile_Slot)
        {
            if (item.GetTile == null) continue;
            var group = ScanLineGroup(item);
            if (group.Count >= 3)
            {
                foreach (var slot in group)
                    removeSet.Add(slot);
            }
        }

        // 삭제
        foreach (var slot in removeSet)
            slot.RemoveTile();
    }

    /// <summary>
    /// 랜덤으로 타일 가져오기 
    /// </summary>
    public GameObject Get_Tile_Basic_Random()
    {
        var idx = UnityEngine.Random.Range(0, G_Tile.Length);
        return G_Tile[idx];
    }

    /// <summary>
    /// 두 타일의 위치 교환
    /// </summary>
    public void SwapTiles()
    {
        if (FirstTouch_Tile != null && SecondTouch_Tile != null)
        {
            // 각 타일의 슬롯 가져오기
            var firstslot = FirstTouch_Tile.Get_Tile_Slot;
            var secondslot = SecondTouch_Tile.Get_Tile_Slot;

            // 위치 교환
            FirstTouch_Tile.Set_Swap(secondslot);
            SecondTouch_Tile.Set_Swap(firstslot);
            Debug.Log("위치이동");

            Reset();
        }
    }

    /// <summary>
    /// 리셋
    /// </summary>
    public void Reset()
    {
        Debug.Log("리셋처리");
        FirstTouch_Tile = null;
        SecondTouch_Tile = null;
    }
}