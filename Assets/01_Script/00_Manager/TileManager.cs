using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SearchService;
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
    public bool All_Scan_Boom(UI_Tile[] tiles = null)
    {
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
        bool isremove = false;
        foreach (var slot in removeSet)
        {
            slot.RemoveTile();
            isremove = Check_TargetSlot(tiles, slot, isremove);
        }
        return isremove;
    }

    /// <summary>
    /// 지정한 타일이 삭제목록에 있는지 체크
    /// </summary>
    bool Check_TargetSlot(UI_Tile[] tiles, UI_Tile_Slot slot, bool istargets)
    {
        if (tiles == null || tiles.Length == 0)
        {
            return istargets;
        }

        foreach (var tile in tiles)
        {
            if (tile == null)
            {
                continue;
            }

            if (tile.Get_Tile_Slot == slot)
            {
                istargets = true;
            }
        }
        return istargets;
    }


    /// <summary>
    /// 매치 성공 후 빈자리 찾아가기
    /// </summary>
    public bool All_Scan_Move()
    {
        bool isMove = false;
        bool checkmove = true;
        while (true)
        {
            if (!checkmove)
            {
                break;
            }
            checkmove = false;

            //L_Tile_Slot를 딕셔너리 형태로 key가 GetPoint의 item1, value가 리스트형태로 GetPoint의 item2를 가지는 딕셔너리 생성
            var dic = L_Tile_Slot
                        .GroupBy(slot => slot.GetPoint.Item1)
                        .ToDictionary(group => group.Key, group => group.Select(slot => slot).ToList());

            //위에서 아래로
            foreach (var item in dic)
            {
                var slot = item.Value;
                //제일 아래부터 체크 (Item2가 작은 것부터 정렬)
                var orderedSlots = slot.OrderBy(x => x.GetPoint.Item2).ToList();

                foreach (var items in orderedSlots)
                {
                    if (items.GetTile != null)
                    {
                        continue;
                    }

                    //빈공간일 경우 y값 기준 가장 가까운 Tile찾기
                    var aboveTiles = orderedSlots
                        .Where(s => s.GetPoint.Item2 > items.GetPoint.Item2 && s.GetTile != null)
                        .OrderBy(s => s.GetPoint.Item2)
                        .FirstOrDefault();

                    //가장 가까운 tile이 없으면 continue;
                    if (aboveTiles == null)
                    {
                        continue;
                    }

                    //가장 가까운 tile을
                    var tile = aboveTiles.GetTile;

                    //가장 가까운 tile이 있으면 Set_Swap함수를 이용해 이동시키기
                    tile.Set_Swap(items);

                    //기존 슬롯은 정보 리셋
                    aboveTiles.Reset();
                    checkmove = true;
                    isMove = true;
                }
            }

            //오른쪽 대각선
            foreach (var item in dic.OrderByDescending(x => x.Key))
            {
                var slot = item.Value;
                //제일 위부터 체크 (Item2가 큰 것부터 정렬)
                var orderedSlots = slot.OrderByDescending(x => x.GetPoint.Item2).ToList();

                foreach (var items in orderedSlots)
                {
                    if (items.GetTile != null)
                    {
                        continue;
                    }

                    //빈공간일 경우 오른쪽 위(0.5f 위) 타일 찾기
                    var topRightSlot = L_Tile_Slot
                        .Where(s =>
                            s.GetPoint.Item1 == items.GetPoint.Item1 + 1f &&
                            Mathf.Approximately(s.GetPoint.Item2, items.GetPoint.Item2 + 0.5f) &&
                            s.GetTile != null)
                        .FirstOrDefault();

                    //가장 가까운 tile이 없으면 continue;
                    if (topRightSlot == null)
                    {
                        continue;
                    }

                    //가장 가까운 tile을
                    var tile = topRightSlot.GetTile;

                    //가장 가까운 tile이 있으면 Set_Swap함수를 이용해 이동시키기
                    tile.Set_Swap(items);

                    //기존 슬롯은 정보 리셋
                    topRightSlot.Reset();

                    //대각선은 한번이라도 움직이면 다른 것들 다시 체크
                    checkmove = true;
                    isMove = true;
                    break;
                }
                if (checkmove)
                {
                    break;
                }
            }


            //왼쪽 대각선
            foreach (var item in dic.OrderBy(x => x.Key))
            {
                var slot = item.Value;
                //제일 위부터 체크 (Item2가 큰 것부터 정렬)
                var orderedSlots = slot.OrderByDescending(x => x.GetPoint.Item2).ToList();

                foreach (var items in orderedSlots)
                {
                    if (items.GetTile != null)
                    {
                        continue;
                    }

                    //빈공간일 경우 왼쪽 위(0.5f 위) 타일 찾기
                    var topleftSlot = L_Tile_Slot
                        .Where(s =>
                            s.GetPoint.Item1 == items.GetPoint.Item1 - 1f &&
                            Mathf.Approximately(s.GetPoint.Item2, items.GetPoint.Item2 + 0.5f) &&
                            s.GetTile != null)
                        .FirstOrDefault();

                    //가장 가까운 tile이 없으면 continue;
                    if (topleftSlot == null)
                    {
                        continue;
                    }

                    //가장 가까운 tile을
                    var tile = topleftSlot.GetTile;

                    //가장 가까운 tile이 있으면 Set_Swap함수를 이용해 이동시키기
                    tile.Set_Swap(items);

                    //기존 슬롯은 정보 리셋
                    topleftSlot.Reset();

                    //대각선은 한번이라도 움직이면 다른 것들 다시 체크
                    checkmove = true;
                    isMove = true;
                    break;
                }
                if (checkmove)
                {
                    break;
                }
            }
        }
        return isMove;
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

        PlayManager.instance.GetStay = true;

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
        //이동 후 잠깐 딜레이
        yield return new WaitForSeconds(0.5f);

        // 각 타일의 슬롯 가져오기
        var firstslot = FirstTouch_Tile.Get_Tile_Slot;
        var secondslot = SecondTouch_Tile.Get_Tile_Slot;

        var first = FirstTouch_Tile;
        var second = SecondTouch_Tile;

        if (!All_Scan_Boom(new UI_Tile[] { first, second }))
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("위치 원상복구");
            first.Set_Swap(secondslot);
            second.Set_Swap(firstslot);
        }
        Reset();
        StartCoroutine(IE_Move_And_Boom());
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


    /// <summary>
    /// 이동 후 제거, 생성 반복
    /// </summary>
    public IEnumerator IE_Move_And_Boom()
    {
        var wait = new WaitForSeconds(0.5f);
        while (true)
        {
            //이동
            yield return wait;
            var ismove = All_Scan_Move();

            yield return wait;
            //생성
            var iscraete = PlayManager.instance.Get_UI_Grid().Create_None_Slot_Tile();

            //삭제
            yield return wait;
            var isboom = All_Scan_Boom();

            //생성 삭제 이동 아무것도 없다면 종료
            if (!ismove && !iscraete && !isboom)
            {
                break;
            }
        }

        yield return wait;
        PlayManager.instance.GetStay = false;
    }
}