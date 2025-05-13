using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

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
                //하나의 그룹만 넘기기
                return result;
            }
        }

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
    void Set_Huddle_Tile_Active(HashSet<UI_Tile_Slot> destroyGroup)
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
    /// 매치 성공 후 빈자리로 타일 이동
    /// </summary>
    public bool All_Scan_Move()
    {
        bool anyMovement = false;

        // 1. 모든 수직 이동 처리
        bool verticalMoved = Set_Move_All_Down();
        if (verticalMoved)
        {
            anyMovement = true;
        }

        // 2. 모든 오른쪽 대각선 이동 처리
        bool diagonalRightMoved = Set_All_Right_Moves();
        if (diagonalRightMoved)
        {
            anyMovement = true;
        }

        // 3. 모든 왼쪽 대각선 이동 처리
        bool diagonalLeftMoved = Set_All_Left_Moves();
        if (diagonalLeftMoved)
        {
            anyMovement = true;
        }

        return anyMovement;
    }

    /// <summary>
    /// 수직 방향(위에서 아래로) 타일 이동
    /// </summary>
    bool Set_Move_Down(Dictionary<float, List<UI_Tile_Slot>> columnDict)
    {
        bool hasMoved = false;

        // 각 열에 대해 처리
        foreach (var column in columnDict)
        {
            // 아래에서 위로 슬롯 정렬 (y좌표 기준)
            var orderedSlots = column.Value.OrderBy(x => x.GetPoint.Item2).ToList();

            // 각 슬롯에 대해 빈 공간 처리
            foreach (var emptySlot in orderedSlots)
            {
                // 이미 타일이 있는 슬롯은 건너뜀
                if (emptySlot.GetTile != null)
                    continue;

                // 빈 슬롯 위에 있는 가장 가까운 타일 찾기
                var filledSlot = orderedSlots
                    .Where(s => s.GetPoint.Item2 > emptySlot.GetPoint.Item2 && s.GetTile != null)
                    .OrderBy(s => s.GetPoint.Item2)
                    .FirstOrDefault();

                // 위에 타일이 없으면 처리 건너뜀
                if (filledSlot == null || filledSlot.GetTile == null)
                    continue;

                // 타일 이동
                var tile = filledSlot.GetTile;
                tile.Set_Swap(emptySlot);
                filledSlot.Reset();

                hasMoved = true;
            }
        }

        return hasMoved;
    }

    /// <summary>
    /// 모든 타일의 애니메이션이 완료될 때까지 기다림
    /// </summary>
    IEnumerator IE_Wait_For_Tile_Animations()
    {
        // 시작하기 전에 짧은 대기 (애니메이션이 시작될 시간 확보)
        yield return new WaitForSeconds(0.02f);

        // 최대 대기 시간 설정 (무한 대기 방지)
        float maxWaitTime = 0.5f; // 다시 0.5초로 충분한 대기 시간 제공
        float elapsedTime = 0f;

        // 추가 안전 대기 횟수 - 모든 타일이 멈춘 것으로 확인되었지만, 추가로 대기하는 프레임 수
        int safetyFrames = 3;
        int currentSafetyFrame = 0;

        while (elapsedTime < maxWaitTime)
        {
            bool anyMoving = false;

            // 모든 타일 체크
            foreach (var tile in L_Tile)
            {
                if (tile != null && tile.Check_Is_Moving())
                {
                    anyMoving = true;
                    currentSafetyFrame = 0; // 움직이는 타일이 있으면 안전 카운터 리셋
                    break;
                }
            }

            // 모든 타일이 정지 상태라면 안전 프레임 카운트 증가
            if (!anyMoving)
            {
                currentSafetyFrame++;

                // 안전 프레임 카운트가 목표에 도달하면 종료
                if (currentSafetyFrame >= safetyFrames)
                {
                    yield break;
                }
            }

            // 다음 프레임까지 대기
            yield return null;
            elapsedTime += Time.deltaTime;
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
        // 애니메이션 완료 대기
        yield return StartCoroutine(IE_Wait_For_Tile_Animations());

        //이동 후 잠깐 딜레이
        yield return new WaitForEndOfFrame();

        // 각 타일의 슬롯 가져오기
        var firstslot = FirstTouch_Tile.Get_Tile_Slot;
        var secondslot = SecondTouch_Tile.Get_Tile_Slot;

        var first = FirstTouch_Tile;
        var second = SecondTouch_Tile;

        var isboom = All_Scan_Boom(new UI_Tile[] { first, second });
        Reset();
        if (!isboom)
        {
            yield return new WaitForEndOfFrame();
            Debug.Log("위치 원상복구");

            // 원상복구 시에도 애니메이션 적용 (이 부분은 사용자 경험을 위해 애니메이션 적용)
            first.Set_Swap(secondslot, true);
            second.Set_Swap(firstslot, true);

            // 원상복구 애니메이션 대기
            yield return StartCoroutine(IE_Wait_For_Tile_Animations());

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
        // 최소 딜레이로 설정
        var shortWait = new WaitForEndOfFrame();

        //이동, 생성, 삭제
        while (true)
        {
            //이동
            yield return shortWait;

            // 단계별 이동 처리
            bool anyMovement = false;

            // 1. 모든 가능한 수직 이동 처리
            bool verticalMoved = Set_Move_All_Down();
            if (verticalMoved)
            {
                anyMovement = true;
                // 수직 이동 애니메이션 대기
                yield return StartCoroutine(IE_Wait_For_Tile_Animations());
            }

            // 2. 모든 오른쪽 대각선 이동을 한 번에 처리
            bool diagonalRightMoved = Set_All_Right_Moves();
            if (diagonalRightMoved)
            {
                anyMovement = true;
                // 오른쪽 대각선 이동 애니메이션 대기
                yield return StartCoroutine(IE_Wait_For_Tile_Animations());
            }

            // 3. 모든 왼쪽 대각선 이동을 한 번에 처리
            bool diagonalLeftMoved = Set_All_Left_Moves();
            if (diagonalLeftMoved)
            {
                anyMovement = true;
                // 왼쪽 대각선 이동 애니메이션 대기
                yield return StartCoroutine(IE_Wait_For_Tile_Animations());
            }

            // 모든 이동 종류 재시도 - 대각선 이동 후 수직 이동이 가능할 수 있음
            bool additionalMoves = false;
            do
            {
                additionalMoves = false;

                // 다시 수직 이동 확인
                verticalMoved = Set_Move_All_Down();
                if (verticalMoved)
                {
                    anyMovement = true;
                    additionalMoves = true;
                    yield return StartCoroutine(IE_Wait_For_Tile_Animations());
                }

                // 다시 오른쪽 대각선 이동 확인
                if (!verticalMoved)
                {
                    diagonalRightMoved = Set_All_Right_Moves();
                    if (diagonalRightMoved)
                    {
                        anyMovement = true;
                        additionalMoves = true;
                        yield return StartCoroutine(IE_Wait_For_Tile_Animations());
                    }

                    // 다시 왼쪽 대각선 이동 확인
                    if (!diagonalRightMoved)
                    {
                        diagonalLeftMoved = Set_All_Left_Moves();
                        if (diagonalLeftMoved)
                        {
                            anyMovement = true;
                            additionalMoves = true;
                            yield return StartCoroutine(IE_Wait_For_Tile_Animations());
                        }
                    }
                }
            } while (additionalMoves);

            bool ismove = anyMovement;

            // 모든 이동이 확실히 완료된 후에 생성 단계 진행
            yield return new WaitForSeconds(0.05f);

            //생성
            var iscraete = PlayManager.instance.Get_UI_Grid().Create_None_Slot_Tile();

            // 새로 생성된 타일의 애니메이션이 완료될 때까지 대기
            yield return StartCoroutine(IE_Wait_For_Tile_Animations());
            yield return new WaitForSeconds(0.05f);

            //삭제
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
            yield return shortWait;

            // 타일 섞기 후 애니메이션이 완료될 때까지 대기
            yield return StartCoroutine(IE_Wait_For_Tile_Animations());
        }

        ClearManager.instance.Set_Clear();
        yield return shortWait;
        PlayManager.instance.GetStay = false;
    }

    /// <summary>
    /// 모든 수직 이동 실행
    /// </summary>
    bool Set_Move_All_Down()
    {
        // 타일 슬롯을 x좌표(Item1)별로 그룹화하여 딕셔너리 생성
        var columnDict = L_Tile_Slot
                    .GroupBy(slot => slot.GetPoint.Item1)
                    .ToDictionary(group => group.Key, group => group.Select(slot => slot).ToList());

        return Set_Move_Down(columnDict);
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

                    // 임시로 타일 교환 (애니메이션 없이)
                    var tempTile1 = slot.GetTile;
                    var tempTile2 = targetSlot.GetTile;
                    var tempSlot1 = slot;
                    var tempSlot2 = targetSlot;

                    // 임시 교환 - 애니메이션 없이 교환
                    tempTile1.Set_Swap(tempSlot2, false);
                    tempTile2.Set_Swap(tempSlot1, false);

                    // 매치 확인
                    bool hasMatch = false;
                    var group1 = Set_ScanLineGroup(tempSlot1);
                    var group2 = Set_ScanLineGroup(tempSlot2);

                    if (group1.Count >= 3 || group2.Count >= 3)
                    {
                        hasMatch = true;
                    }

                    // 원래 위치로 복구 - 애니메이션 없이 복구
                    tempTile1.Set_Swap(tempSlot1, false);
                    tempTile2.Set_Swap(tempSlot2, false);

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

        // 랜덤으로 섞인 슬롯에 타일 재배치 (애니메이션 적용)
        for (int i = 0; i < basicTiles.Count; i++)
        {
            // 타일 섞기는 사용자 경험을 위해 애니메이션 적용
            basicTiles[i].Set_Swap(basicTileSlots[i], true);
        }

        Debug.Log($"타일 {basicTiles.Count}개 섞기 완료");
    }

    /// <summary>
    /// 오른쪽 대각선 방향 타일 이동 - 모든 가능한 타일을 한 번에 처리
    /// </summary>
    bool Set_All_Right_Moves()
    {
        bool anyMoved = false;

        // 타일 슬롯을 x좌표(Item1)별로 그룹화하여 딕셔너리 생성
        var columnDict = L_Tile_Slot
                    .GroupBy(slot => slot.GetPoint.Item1)
                    .ToDictionary(group => group.Key, group => group.Select(slot => slot).ToList());

        // 이동 가능한 모든 타일과 목표 슬롯을 미리 계산
        List<(UI_Tile tile, UI_Tile_Slot targetSlot, UI_Tile_Slot sourceSlot)> movesMap = new List<(UI_Tile, UI_Tile_Slot, UI_Tile_Slot)>();

        // 오른쪽에서 왼쪽으로 열 정렬 (x좌표 기준)
        foreach (var column in columnDict.OrderByDescending(x => x.Key))
        {
            // 위에서 아래로 슬롯 정렬 (y좌표 기준)
            var orderedSlots = column.Value.OrderByDescending(x => x.GetPoint.Item2).ToList();

            foreach (var emptySlot in orderedSlots)
            {
                // 이미 타일이 있는 슬롯은 건너뜀
                if (emptySlot.GetTile != null)
                    continue;

                // 오른쪽 위(대각선) 타일 찾기
                var topRightSlot = L_Tile_Slot
                    .Where(s =>
                        s.GetPoint.Item1 == emptySlot.GetPoint.Item1 + 1f &&
                        Mathf.Approximately(s.GetPoint.Item2, emptySlot.GetPoint.Item2 + 0.5f) &&
                        s.GetTile != null)
                    .FirstOrDefault();

                if (topRightSlot == null)
                    continue;

                // 이동할 타일과 슬롯을 이동 맵에 추가 - 이미 다른 이동에 포함된 타일인지 확인
                if (movesMap.Any(m => m.tile == topRightSlot.GetTile || m.targetSlot == emptySlot))
                    continue;

                movesMap.Add((topRightSlot.GetTile, emptySlot, topRightSlot));
            }
        }

        // 모든 이동을 한 번에 실행
        foreach (var move in movesMap)
        {
            // isDiagonal = true로 설정하여 대각선 이동임을 전달
            move.tile.Set_Swap(move.targetSlot, true, true);
            move.sourceSlot.Reset();
            anyMoved = true;
        }

        return anyMoved;
    }

    /// <summary>
    /// 왼쪽 대각선 방향 타일 이동 - 모든 가능한 타일을 한 번에 처리
    /// </summary>
    bool Set_All_Left_Moves()
    {
        bool anyMoved = false;

        // 타일 슬롯을 x좌표(Item1)별로 그룹화하여 딕셔너리 생성
        var columnDict = L_Tile_Slot
                    .GroupBy(slot => slot.GetPoint.Item1)
                    .ToDictionary(group => group.Key, group => group.Select(slot => slot).ToList());

        // 이동 가능한 모든 타일과 목표 슬롯을 미리 계산
        List<(UI_Tile tile, UI_Tile_Slot targetSlot, UI_Tile_Slot sourceSlot)> movesMap = new List<(UI_Tile, UI_Tile_Slot, UI_Tile_Slot)>();

        // 왼쪽에서 오른쪽으로 열 정렬 (x좌표 기준)
        foreach (var column in columnDict.OrderBy(x => x.Key))
        {
            // 위에서 아래로 슬롯 정렬 (y좌표 기준)
            var orderedSlots = column.Value.OrderByDescending(x => x.GetPoint.Item2).ToList();

            foreach (var emptySlot in orderedSlots)
            {
                // 이미 타일이 있는 슬롯은 건너뜀
                if (emptySlot.GetTile != null)
                    continue;

                // 왼쪽 위(대각선) 타일 찾기
                var topLeftSlot = L_Tile_Slot
                    .Where(s =>
                        s.GetPoint.Item1 == emptySlot.GetPoint.Item1 - 1f &&
                        Mathf.Approximately(s.GetPoint.Item2, emptySlot.GetPoint.Item2 + 0.5f) &&
                        s.GetTile != null)
                    .FirstOrDefault();

                if (topLeftSlot == null)
                    continue;

                // 이동할 타일과 슬롯을 이동 맵에 추가 - 이미 다른 이동에 포함된 타일인지 확인
                if (movesMap.Any(m => m.tile == topLeftSlot.GetTile || m.targetSlot == emptySlot))
                    continue;

                movesMap.Add((topLeftSlot.GetTile, emptySlot, topLeftSlot));
            }
        }

        // 모든 이동을 한 번에 실행
        foreach (var move in movesMap)
        {
            // isDiagonal = true로 설정하여 대각선 이동임을 전달
            move.tile.Set_Swap(move.targetSlot, true, true);
            move.sourceSlot.Reset();
            anyMoved = true;
        }

        return anyMoved;
    }
}