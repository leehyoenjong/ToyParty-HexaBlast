using System.Collections;
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

    // 4방향(가로, 세로, 대각선2개)만 검사 (양방향 포함)
    (float dx, float dy)[] i_Directions = new (float, float)[]
    {
        (1f, 0f),    // 가로
        (0f, 1f),    // 세로
        (1f, 0.5f),  // 대각선 우상단
        (1f, -0.5f), // 대각선 우하단
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
        var startPoint = start.GetPoint;

        foreach (var (dx, dy) in i_Directions)
        {
            List<UI_Tile_Slot> line = new List<UI_Tile_Slot>();
            line.Add(start);

            // +방향
            var nx = startPoint.Item1 + dx;
            var ny = startPoint.Item2 + dy;
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
    public bool All_Scan(UI_Tile[] tiles = null)
    {
        bool isremove = false;
        HashSet<UI_Tile_Slot> removeSet = new HashSet<UI_Tile_Slot>();

        foreach (var item in L_Tile_Slot)
        {
            if (item.GetTile == null)
            {
                continue;
            }

            var group = ScanLineGroup(item);
            if (group.Count >= 3)
            {
                foreach (var slot in group)
                {
                    removeSet.Add(slot);
                }
            }
        }

        // 삭제
        foreach (var slot in removeSet)
        {
            slot.RemoveTile();
            if (tiles == null)
            {
                continue;
            }

            //타일처리
            foreach (var tile in tiles)
            {
                if (tile.Get_Tile_Slot == slot)
                {
                    isremove = true;
                }
            }
        }
        return isremove;
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
        if (FirstTouch_Tile == null || SecondTouch_Tile == null)
        {
            return;
        }

        // 각 타일의 슬롯 가져오기
        var firstslot = FirstTouch_Tile.Get_Tile_Slot;
        var secondslot = SecondTouch_Tile.Get_Tile_Slot;

        // 위치 교환
        FirstTouch_Tile.Set_Swap(secondslot);
        SecondTouch_Tile.Set_Swap(firstslot);

        Debug.Log("위치이동");
        StartCoroutine(IE_Swap());
    }

    /// <summary>
    /// 스캔 후 리셋 
    /// </summary>
    /// <returns></returns>
    IEnumerator IE_Swap()
    {
        // 각 타일의 슬롯 가져오기
        var firstslot = FirstTouch_Tile.Get_Tile_Slot;
        var secondslot = SecondTouch_Tile.Get_Tile_Slot;

        var first = FirstTouch_Tile;
        var second = SecondTouch_Tile;

        Debug.Log("제거 체크");
        if (!All_Scan(new UI_Tile[] { first, second }))
        {
            yield return new WaitForSeconds(1f);
            Debug.Log("위치 원상복구");
            first.Set_Swap(secondslot);
            second.Set_Swap(firstslot);
        }
        Reset();
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