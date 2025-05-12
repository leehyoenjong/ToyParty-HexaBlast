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
    public (float dx, float dy)[] Get_Directions => i_Directions;

    Dictionary<E_Tile_Color, int> D_Tile_Color = new Dictionary<E_Tile_Color, int>()
    {
        {E_Tile_Color.Red, 0},
        {E_Tile_Color.Blue, 0},
        {E_Tile_Color.Yellow, 0},
        {E_Tile_Color.Green, 0},
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
    public HashSet<UI_Tile_Slot> Set_ScanLineGroup(UI_Tile_Slot start)
    {
        HashSet<UI_Tile_Slot> result = new HashSet<UI_Tile_Slot>();
        if (start == null || start.GetTile == null)
        {
            return result;
        }

        // Basic 타입의 타일만 체크
        if (start.GetTile.Get_Tile_Kind() != E_Tile_Kind.Basic)
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
                // Basic 타입과 같은 색상의 타일만 체크
                if (neighbor.GetTile.Get_Tile_Kind() != E_Tile_Kind.Basic || neighbor.GetTile.Get_Tile_Color() != color)
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
                // Basic 타입과 같은 색상의 타일만 체크
                if (neighbor.GetTile.Get_Tile_Kind() != E_Tile_Kind.Basic || neighbor.GetTile.Get_Tile_Color() != color)
                {
                    break;
                }
                line.Add(neighbor);
                nx -= dx;
                ny -= dy;
            }

            // 3개 이상 연속이면 결과에 추가 - 이 방향에서 매치가 발견됨
            if (line.Count >= 3)
            {
                foreach (var slot in line)
                {
                    result.Add(slot);
                }
            }
            // 3개 미만이면 이 방향에서는 매치가 없음 - 건너뜀
        }

        // 아무 방향에서도 매치가 없으면 빈 결과 반환
        // 매치가 있는 방향에서만 해당 슬롯들이 결과에 추가됨
        return result;
    }

    /// <summary>
    /// 전체 슬롯에 담겨있는 타일이랑 같은 타일들이 나열되어 있는지 체크 (직선 연속만 인정)
    /// </summary>
    public bool All_Scan_Boom(UI_Tile[] tiles = null)
    {
        HashSet<UI_Tile_Slot> hs_remove_tile = new HashSet<UI_Tile_Slot>();
        HashSet<UI_Tile_Slot> hs_match_tile = new HashSet<UI_Tile_Slot>();

        foreach (var item in L_Tile_Slot)
        {
            // 이미 처리된 슬롯이거나 타일이 없는 슬롯은 건너뜀
            if (item.GetTile == null || hs_match_tile.Contains(item))
            {
                continue;
            }

            var group = Set_ScanLineGroup(item);
            if (group.Count >= 3)
            {
                foreach (var slot in group)
                {
                    hs_remove_tile.Add(slot);
                    hs_match_tile.Add(slot); // 이미 매치된 슬롯으로 표시
                }

                //파괴되는 slot 타일 근처에 E_Tile_Kind.Huddle 타입의 타일이 있을 경우 Set_Crush함수 호출
                //단, 매칭되는 group 하나당 한번만 호출임
                Set_Huddle_Tile_Active(group);
            }
        }
        // 삭제
        bool isremove = false;
        ScoreManager.instance.Update_Score(hs_remove_tile.Count);
        foreach (var slot in hs_remove_tile)
        {
            slot.RemoveTile();
            isremove = Check_TargetSlot(tiles, slot, isremove);
        }
        ClearManager.instance.Update_Clear_Count();
        return isremove;
    }

    /// <summary>
    /// 파괴되는 타일 그룹 주변의 Huddle 타입 타일 확인 및 처리
    /// </summary>
    private void Set_Huddle_Tile_Active(HashSet<UI_Tile_Slot> destroyGroup)
    {
        // 매칭 그룹 당 한번만 호출
        // 처리할 Huddle 타일 찾기 - LINQ 활용
        var huddletile = L_Tile_Slot
            .Where(slot => slot.GetTile != null && slot.GetTile.Get_Tile_Kind() == E_Tile_Kind.Huddle)
            .Select(slot => slot.GetTile as UI_Tile_Paengi)
            .Where(huddleTile => huddleTile != null)
            .ToList();

        // 이미 처리된 Huddle 타일을 추적하기 위한 집합
        HashSet<UI_Tile_Paengi> hs_paengi = new HashSet<UI_Tile_Paengi>();

        // 파괴될 타일 그룹에서 하나를 대표로 선택 (그룹 당 한 번만 처리하기 위해)
        foreach (var huddle in huddletile)
        {
            if (hs_paengi.Contains(huddle))
                continue;

            // 이 Huddle 타일이 파괴 대상 그룹 중 하나라도 인접해 있는지 확인
            var removetile = destroyGroup
                .Select(slot => slot.GetTile)
                .Where(tile => tile != null)
                .FirstOrDefault(tile => huddle.Check_Cursh(tile));

            if (removetile != null)
            {
                // Set_Crush 함수 호출 (파괴되는 타일 전달)
                huddle.Set_Crush(removetile);
                // 처리된 타일로 표시
                hs_paengi.Add(huddle);
            }
        }
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
    /// 시작할때 타일 생성 시 랜덤으로 가져오 되, 전체 생성 갯수에 맞춰 생성
    /// </summary>
    public GameObject Get_Tile_Basic_Random_Start()
    {
        //다이렉트 생성 타일을 제외한 전체 타일 갯수를 4등분하여
        //각 E_Tile_Color None을 제외한 나머지가 골고루 생성될 수 있도록 처리
        var stage_data = PlayManager.instance.Get_Stage_Data();
        var maxtile = stage_data.St_Tile_Stage.Sum(x => x.Grid_Slot_Count);
        maxtile -= stage_data.St_Tile_Stage.Sum(x => x.St_Tile_Directs.Count);

        // 각 색상별 목표 타일 수 계산 (총 타일 수 / 4)
        int targetPerColor = maxtile / 4;

        // 가장 적은 수의 타일을 가진 색상 찾기
        var minColorCount = D_Tile_Color.OrderBy(x => x.Value).First();

        // 모든 색상이 목표치에 도달했는지 확인
        bool allFull = D_Tile_Color.All(x => x.Value >= targetPerColor);

        int idx;
        if (allFull)
        {
            // 모든 색상이 목표치에 도달했으면 완전 랜덤
            idx = UnityEngine.Random.Range(0, G_Tile.Length);
        }
        else
        {
            // 목표치에 도달하지 않은 색상 중에서 랜덤 선택
            var availableColors = D_Tile_Color
                .Where(x => x.Value < targetPerColor)
                .Select(x => x.Key)
                .ToList();

            E_Tile_Color selectedColor = availableColors[UnityEngine.Random.Range(0, availableColors.Count)];
            idx = (int)selectedColor - 1; // 인덱스로 변환 (E_Tile_Color.Red는 1이므로 인덱스는 0)

            // 선택된 색상의 카운트 증가
            D_Tile_Color[selectedColor]++;
        }

        return G_Tile[idx];
    }

    /// <summary>
    /// 랜덤으로 가져오기
    /// </summary>
    /// <returns></returns>
    public GameObject Get_Tile_Basic_Random()
    {
        var idx = UnityEngine.Random.Range(0, G_Tile.Length);
        var kind = (E_Tile_Color)idx + 1;
        if (D_Tile_Color.ContainsKey(kind))
        {
            D_Tile_Color[kind]++;
        }
        return G_Tile[idx];
    }


    /// <summary>
    /// 타일 제거 시 갯수처리
    /// </summary>
    /// <param name="tile"></param>
    public void Destory_Tile_Count(UI_Tile tile)
    {
        var kind = tile.Get_Tile_Color();
        if (!D_Tile_Color.ContainsKey(kind))
        {
            return;
        }
        D_Tile_Color[kind]--;
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

        var isboom = All_Scan_Boom(new UI_Tile[] { first, second });
        Reset();
        if (!isboom)
        {
            yield return new WaitForSeconds(0.5f);
            Debug.Log("위치 원상복구");
            first.Set_Swap(secondslot);
            second.Set_Swap(firstslot);
            PlayManager.instance.GetStay = false;
            yield break;
        }
        //원상복구가 아니라면 이동횟수 차감처리 
        ClearManager.instance.Update_Move_Count();
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
        var wait = new WaitForSeconds(0.25f);

        //이동, 생성, 삭제
        while (true)
        {
            //이동
            yield return wait;
            var ismove = All_Scan_Move();

            //생성
            yield return wait;
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

        //체크, 섞기
        while (true)
        {
            //파괴 가능한 타일 있는지 체크
            var check = Check_Destory_Tile();

            //파괴 가능한 타일이 있다면 종료
            if (check)
            {
                break;
            }

            //파괴 가능한 타일이 없다면 전체 섞기
            Set_All_Tile_Random();
            yield return wait;
        }

        ClearManager.instance.Set_Clear();
        yield return wait;
        PlayManager.instance.GetStay = false;
    }

    /// <summary>
    /// 파괴 가능 타일 체크
    /// </summary>
    bool Check_Destory_Tile()
    {
        // 모든 슬롯 위치 확인
        foreach (var slot in L_Tile_Slot)
        {
            if (slot.GetTile == null)
                continue;

            // 현재 타일 색상
            var currentColor = slot.GetTile.Get_Tile_Color();
            var currentPoint = slot.GetPoint;

            // 4방향으로 이동 가능한 위치 확인
            foreach (var (dx, dy) in i_Directions)
            {
                // 양방향 체크
                for (int dir = -1; dir <= 1; dir += 2)
                {
                    float nx = currentPoint.Item1 + dx * dir;
                    float ny = currentPoint.Item2 + dy * dir;

                    // 이동할 위치의 슬롯 찾기
                    var targetSlot = L_Tile_Slot.Find(s => s.GetPoint.Item1 == nx && Mathf.Approximately(s.GetPoint.Item2, ny));
                    if (targetSlot == null || targetSlot.GetTile == null)
                        continue;

                    // 임시로 타일 교환
                    var tempTile1 = slot.GetTile;
                    var tempTile2 = targetSlot.GetTile;
                    var tempSlot1 = slot;
                    var tempSlot2 = targetSlot;

                    // 임시 교환
                    tempTile1.Set_Swap(tempSlot2);
                    tempTile2.Set_Swap(tempSlot1);

                    // 매치 확인
                    bool hasMatch = false;
                    var group1 = Set_ScanLineGroup(tempSlot1);
                    var group2 = Set_ScanLineGroup(tempSlot2);

                    if (group1.Count >= 3 || group2.Count >= 3)
                    {
                        hasMatch = true;
                    }

                    // 원래 위치로 복구
                    tempTile1.Set_Swap(tempSlot1);
                    tempTile2.Set_Swap(tempSlot2);

                    if (hasMatch)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 타일 전체 섞기
    /// </summary>
    void Set_All_Tile_Random()
    {
        // 모든 Basic 타일만 찾기
        List<UI_Tile> basicTiles = new List<UI_Tile>();
        List<UI_Tile_Slot> basicTileSlots = new List<UI_Tile_Slot>();

        foreach (var tile in L_Tile)
        {
            if (tile != null && tile.Get_Tile_Kind() == E_Tile_Kind.Basic)
            {
                basicTiles.Add(tile);
                basicTileSlots.Add(tile.Get_Tile_Slot);
            }
        }

        // 슬롯 리스트를 랜덤으로 섞기
        System.Random random = new System.Random();
        basicTileSlots = basicTileSlots.OrderBy(x => random.Next()).ToList();

        // 먼저 모든 Basic 타일을 슬롯에서 분리
        foreach (var tile in basicTiles)
        {
            tile.Get_Tile_Slot.Reset();
        }

        // 랜덤으로 섞인 슬롯에 타일 재배치
        for (int i = 0; i < basicTiles.Count; i++)
        {
            basicTiles[i].Set_Swap(basicTileSlots[i]);
        }

        Debug.Log($"타일 {basicTiles.Count}개 섞기 완료");
    }
}