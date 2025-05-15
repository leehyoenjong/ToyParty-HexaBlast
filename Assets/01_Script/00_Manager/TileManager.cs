using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager instance;

    //타일 슬롯 리스트
    List<UI_Tile_Slot> L_Tile_Slot = new List<UI_Tile_Slot>();
    public List<UI_Tile_Slot> Get_Tile_Slot => L_Tile_Slot;

    List<UI_Tile_Slot> L_Tile_Slot_Hight = new List<UI_Tile_Slot>();
    public List<UI_Tile_Slot> Get_Hight_Point => L_Tile_Slot_Hight;

    //타일 리스트
    List<UI_Tile> L_Tile = new List<UI_Tile>();
    public List<UI_Tile> Get_Tile => L_Tile;

    WaitForEndOfFrame Wait_End = new WaitForEndOfFrame();
    WaitForSeconds Wait_Five = new WaitForSeconds(0.5f);

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

        // 장애물은 패스
        if (start.GetTile.Get_Tile_Kind() == E_Tile_Kind.Huddle)
        {
            return result;
        }

        var color = start.GetTile.Get_Tile_Color();
        var startPoint = start.GetPoint;

        List<UI_Tile_Slot> line = new List<UI_Tile_Slot>();
        foreach (var (dx, dy) in i_Directions)
        {
            line.Clear();
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
                //하나의 그룹만 넘기기
                return result;
            }
        }

        return result;
    }

    /// <summary>
    /// 전체 슬롯에 담겨있는 타일이랑 같은 타일들이 나열되어 있는지 체크 (직선 연속만 인정)
    /// </summary>
    public (HashSet<UI_Tile_Slot>, E_Tile_Color) All_Scan_Remove()
    {
        HashSet<UI_Tile_Slot> hs_remove_tile = new HashSet<UI_Tile_Slot>();

        foreach (var item in L_Tile_Slot)
        {
            // 이미 처리된 슬롯이거나 타일이 없는 슬롯은 건너뜀
            if (item.GetTile == null || hs_remove_tile.Contains(item))
            {
                continue;
            }

            var group = Set_ScanLineGroup(item);
            if (group.Count >= 3)
            {
                foreach (var slot in group)
                {
                    hs_remove_tile.Add(slot);
                }

                //파괴되는 slot 타일 근처에 E_Tile_Kind.Huddle 타입의 타일이 있을 경우 Set_Crush함수 호출
                //단, 매칭되는 group 하나당 한번만 호출임
                Set_Huddle_Tile_Active(group);

                //한번에 한 그룹씩만 처리하기 위한 break
                break;
            }
        }

        //삭제여부 확인 
        if (hs_remove_tile.Count <= 0)
        {
            return (hs_remove_tile, E_Tile_Color.None);
        }

        //힌트 강제종료
        HintManager.instance.Set_Driect_Hint_Off();

        // 삭제
        var destorycolor = E_Tile_Color.None;
        ScoreManager.instance.Update_Score(hs_remove_tile.Count);
        foreach (var slot in hs_remove_tile)
        {
            if (slot.GetTile == null)
            {
                continue;
            }
            destorycolor = slot.GetTile.Get_Tile_Color();
            slot.RemoveTile(slot);
        }
        ClearManager.instance.Update_Clear_Count();

        return (hs_remove_tile, destorycolor);
    }

    /// <summary>
    /// 특별한 타일 생성
    /// </summary>
    bool Create_Special_Tile(HashSet<UI_Tile_Slot> removeslot, UI_Tile_Slot firsttile, E_Tile_Color createcolor)
    {
        //4개 이상 삭제하는지 체크
        if (removeslot.Count <= 3)
        {
            return false;
        }

        //삭제 갯수와 삭제 종류를 이용해 특수 타일 불러오기
        var destorytypes = SpecialManager.instance.Get_Destory_Types(removeslot);
        var specialtile = SpecialManager.instance.Get_Spceical_Type(destorytypes);

        if (specialtile == null)
        {
            return false;
        }

        //두번째 선택한 타일이 있을 경으 그 위치로 아니라면 removeslot의 마지막 위치로
        var parent_slot = firsttile != null ? firsttile : removeslot.Last();
        var tile = PlayManager.instance.Get_UI_Grid().Create_Tile(parent_slot, specialtile);
        tile.Set_Tile_Color(createcolor);
        tile.Set_Tile_Destory_Type(removeslot);
        return true;
    }

    /// <summary>
    /// 파괴되는 타일 그룹 주변의 Huddle 타입 타일 확인 및 처리
    /// </summary>
    void Set_Huddle_Tile_Active(HashSet<UI_Tile_Slot> destroyGroup)
    {
        // 매칭 그룹 당 한번만 호출
        // 처리할 Huddle 타일 찾기 - LINQ 활용
        var huddletile = L_Tile_Slot
            .Where(slot => slot.GetTile != null && slot.GetTile.Get_Tile_Kind() == E_Tile_Kind.Huddle)
            .Select(slot => slot.GetTile)
            .Where(huddleTile => huddleTile != null)
            .ToList();

        // 이미 처리된 Huddle 타일을 추적하기 위한 집합
        HashSet<UI_Tile> hs_tile = new HashSet<UI_Tile>();

        // 파괴될 타일 그룹에서 하나를 대표로 선택 (그룹 당 한 번만 처리하기 위해)
        foreach (var huddle in huddletile)
        {
            if (hs_tile.Contains(huddle))
            {
                continue;
            }

            // 이 Huddle 타일이 파괴 대상 그룹 중 하나라도 인접해 있는지 확인
            var removetile = destroyGroup
                .Select(slot => slot.GetTile)
                .Where(tile => tile != null)
                .FirstOrDefault(tile => huddle.Check_Remove(tile));

            if (removetile == null)
            {
                continue;
            }

            // Set_Crush 함수 호출 (파괴되는 타일 전달)
            huddle.RemoveTile(removetile.Get_Tile_Slot);
            // 처리된 타일로 표시
            hs_tile.Add(huddle);
        }
    }

    /// <summary>
    /// 시작할때 타일 생성 시 랜덤으로 가져오 되, 전체 생성 갯수에 맞춰 생성
    /// </summary>
    public GameObject Get_Tile_Basic_Random_Start()
    {
        //다이렉트 생성 타일을 제외한 전체 타일 갯수를 4등분하여
        //각 E_Tile_Color None을 제외한 나머지가 골고루 생성될 수 있도록 처리
        var stage_data = StageManager.Get_Stage_Data();
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
    /// 스캔 후 리셋 
    /// </summary>
    /// <returns></returns>
    public IEnumerator IE_Swap()
    {
        // 각 타일의 슬롯 가져오기
        var firstslot = FirstTouch_Tile.Get_Tile_Slot;
        var secondslot = SecondTouch_Tile.Get_Tile_Slot;
        var firsttile = FirstTouch_Tile;
        var secondtile = SecondTouch_Tile;

        //도착지 지정
        firsttile.Set_Swap(secondtile);

        //위치 이동 및 끝날때까지 대기
        yield return this.WaitForAll(firsttile.Move_Tile(secondslot), secondtile.Move_Tile(firstslot));

        //UFO 블록 특수처리
        var check_ufo_frist = firsttile.Get_Tile_Destory_Type == E_Tile_Destory_Type.UFO && secondtile.Get_Tile_Kind() != E_Tile_Kind.Huddle;
        var check_ufo_sconde = secondtile.Get_Tile_Destory_Type == E_Tile_Destory_Type.UFO && firsttile.Get_Tile_Kind() != E_Tile_Kind.Huddle;

        //둘중 UFO가 아닌 색을 매개변수로 넘김
        if (check_ufo_frist)
        {
            secondslot.RemoveTile(firstslot);
        }
        else if (check_ufo_sconde)
        {

            firstslot.RemoveTile(secondslot);
        }

        //삭제처리
        var remove_result = All_Scan_Remove();
        if (remove_result.Item1.Count > 0)
        {
            yield return Wait_Five;
        }

        //특수 블록 생성
        Create_Special_Tile(remove_result.Item1, secondslot, remove_result.Item2);

        //삭제가 되지 않았으면 매칭이 되지 않은 것이기 때문에 원위치 
        if (remove_result.Item1.Count <= 0 && !check_ufo_frist && !check_ufo_sconde)
        {
            yield return Wait_Five;
            //원상복구
            firsttile.Set_Swap(secondtile);

            //위치 이동 및 끝날때까지 대기
            yield return this.WaitForAll(firsttile.Move_Tile(firstslot), secondtile.Move_Tile(secondslot));

            PlayManager.instance.GetStay = false;
            Reset();
            yield break;
        }

        //원상복구가 아니라면 이동횟수 차감처리 
        Reset();
        ClearManager.instance.Update_Move_Count();
        StartCoroutine(IE_Move_And_Boom());
    }

    /// <summary>
    /// 리셋
    /// </summary>
    public void Reset()
    {
        FirstTouch_Tile = null;
        SecondTouch_Tile = null;
    }

    /// <summary>
    /// 이동 후 제거, 생성 반복
    /// </summary>
    public IEnumerator IE_Move_And_Boom()
    {
        while (true)
        {
            //삭제
            var result_remove_list = All_Scan_Remove();

            if (result_remove_list.Item1.Count > 0)
            {
                yield return Wait_Five;
            }

            //특수블록 생성
            Create_Special_Tile(result_remove_list.Item1, null, result_remove_list.Item2);

            // 최소 딜레이로 설정
            yield return Wait_End;

            //빈공간 이동 및 신규 타일 생성
            int createcount = 0;
            System.Action<int> checkcraete = (value) => createcount = value;
            yield return StartCoroutine(Set_All_Move_Tile(checkcraete));

            //파괴한 타일과 생성한 타일이 없다면 종료
            if (result_remove_list.Item1.Count <= 0 && createcount <= 0)
            {
                break;
            }
        }

        //체크, 섞기
        while (true)
        {
            //파괴 가능한 타일 있는지 체크
            var check = Check_Destory_Tile() != null;

            //파괴 가능한 타일이 있다면 종료
            if (check)
            {
                break;
            }

            //파괴 가능한 타일이 없다면 전체 섞기
            Set_All_Tile_Random();
            yield return Wait_End;
        }

        ClearManager.instance.Set_Clear();
        yield return Wait_End;
        PlayManager.instance.GetStay = false;
    }

    /// <summary>
    /// 파괴 가능 타일 체크
    /// </summary>
    public HashSet<UI_Tile_Slot> Check_Destory_Tile()
    {
        // 모든 슬롯 위치 확인
        foreach (var slot in L_Tile_Slot)
        {
            if (slot.GetTile == null || slot.GetTile.Get_Tile_Kind() == E_Tile_Kind.Huddle)
            {
                continue;
            }

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
                    var targetslot = L_Tile_Slot.Find(s => s.GetPoint.Item1 == nx && Mathf.Approximately(s.GetPoint.Item2, ny));
                    if (targetslot == null || targetslot.GetTile == null)
                    {
                        continue;
                    }

                    // 임시로 타일 교환 (애니메이션 없이)
                    var tempTile1 = slot.GetTile;
                    var tempTile2 = targetslot.GetTile;
                    var tempSlot1 = slot;
                    var tempSlot2 = targetslot;

                    // 임시 교환 - 애니메이션 없이 교환
                    tempTile1.Set_Swap(tempTile2);

                    // 매치 확인
                    bool hasMatch = false;
                    var group1 = Set_ScanLineGroup(tempSlot1);
                    var group2 = Set_ScanLineGroup(tempSlot2);

                    if (group1.Count >= 3 || group2.Count >= 3)
                    {
                        hasMatch = true;
                    }

                    // 원래 위치로 복구 - 애니메이션 없이 복구
                    tempTile1.Set_Swap(tempTile2);

                    if (hasMatch)
                    {
                        return group1.Count >= 3 ? group1 : group2;
                    }
                }
            }
        }

        return null;
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

        // 랜덤으로 섞인 슬롯에 타일 재배치 (애니메이션 적용)
        for (int i = 0; i < basicTiles.Count; i++)
        {
            // 타일 섞기는 사용자 경험을 위해 애니메이션 적용
            basicTiles[i].Set_Swap(null);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Set_All_Move_Tile(System.Action<int> action)
    {
        //비어있는 타일 리스트 가져오고 y값이 작은 순서대로 정렬
        //제거 목록
        var remove_tile_list = new List<UI_Tile_Slot>();
        var move_tile_list = new List<UI_Tile_Slot>();
        int createcount = 0;

        while (true)
        {
            //빈 슬롯 챙겨오기
            var none_tile_list = Get_Tile_Slot.Where(x => x.GetTile == null).OrderBy(x => x.GetPoint.Item2).ToList();

            //빈 슬롯이 없다면 종료
            if (none_tile_list.Count <= 0)
            {
                break;
            }
            var yes_tile_list = Get_Tile_Slot.Where(x => x.GetTile != null).OrderBy(x => x.GetPoint.Item2).ToList();
            remove_tile_list.Clear();
            move_tile_list.Clear();
            //이동경로 리스트
            //수직 이동
            yield return StartCoroutine(Tile_Move(0, none_tile_list, yes_tile_list, remove_tile_list, move_tile_list));

            //왼쪽 대각선 이동
            yield return StartCoroutine(Tile_Move(1, none_tile_list, yes_tile_list, remove_tile_list, move_tile_list));

            //수직 이동
            yield return StartCoroutine(Tile_Move(0, none_tile_list, yes_tile_list, remove_tile_list, move_tile_list));

            //왼쪽 대각선 이동
            yield return StartCoroutine(Tile_Move(-1, none_tile_list, yes_tile_list, remove_tile_list, move_tile_list));

            //수직 이동
            yield return StartCoroutine(Tile_Move(0, none_tile_list, yes_tile_list, remove_tile_list, move_tile_list));

            //타일 생성
            var craete = PlayManager.instance.Get_UI_Grid().Create_None_Slot_Tile();
            if (craete)
            {
                createcount++;
            }
        }
        action.Invoke(createcount);
    }

    /// <summary>
    /// 이동이 필요한 타일 이동
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    IEnumerator Tile_Move(float x, List<UI_Tile_Slot> none_tile_list, List<UI_Tile_Slot> yes_tile_list, List<UI_Tile_Slot> remove_tile_list, List<UI_Tile_Slot> move_tile_list)
    {
        foreach (var none_tile in none_tile_list)
        {
            if (remove_tile_list.Contains(none_tile))
            {
                continue;
            }

            foreach (var yes_tile in yes_tile_list)
            {
                if (yes_tile.GetTile == null)
                {
                    continue;
                }

                //x값이 같으면 안되고, 왼쪽 대각선에 위치한 것인지 체크
                var target_x = none_tile.GetPoint.Item1 + x;
                var yestile_x = yes_tile.GetPoint.Item1;
                if (none_tile.GetPoint.Item1 == yestile_x || target_x != yestile_x)
                {
                    continue;
                }
                //y값이 더 큰지 체크
                if (none_tile.GetPoint.Item2 >= yes_tile.GetPoint.Item2)
                {
                    continue;
                }

                //이미 포함된 타일인지 체크
                if (remove_tile_list.Contains(yes_tile))
                {
                    //이동경로 저장
                    move_tile_list.Add(yes_tile);
                    continue;
                }

                //타일이 비어있던 슬롯 도착지 설정
                var tile = yes_tile.GetTile;
                tile.Set_Tile_Slot(none_tile);
                none_tile.SetTile(tile);

                //기존 있던 슬롯의 타일 제거 
                yes_tile.SetTile(null);

                yield return StartCoroutine(tile.Move_Tile(move_tile_list, none_tile));
                remove_tile_list.Add(none_tile);
                break;
            }
        }
    }
}