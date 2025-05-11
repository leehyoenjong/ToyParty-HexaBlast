using UnityEngine;

public class UI_Tile_Paengi : UI_Tile
{
    [Header("파괴 행위 횟수")]
    [SerializeField] int CrushCount;

    /// <summary>
    /// 근처 파괴 시 발동
    /// </summary>
    public void Set_Crush(UI_Tile removetile)
    {
        if (!Check_Cursh(removetile))
        {
            return;
        }

        //횟수를 모두 소모하면 제거
        CrushCount--;

        if (CrushCount > 0)
        {
            return;
        }
        RemoveTile();
    }

    public bool Check_Cursh(UI_Tile removetile)
    {
        //removetile가 한칸위치에 있는 타일이라면 true 아니라면 false
        if (removetile == null || Get_Tile_Slot == null || removetile.Get_Tile_Slot == null)
        {
            return false;
        }

        // 현재 타일과 제거 타일의 위치 가져오기
        var currentPoint = Get_Tile_Slot.GetPoint;
        var removePoint = removetile.Get_Tile_Slot.GetPoint;

        // TileManager에 정의된 4방향 기준으로 검사
        foreach (var direction in TileManager.instance.Get_Directions)
        {
            // 양방향 체크 (+1, -1)
            for (int dir = -1; dir <= 1; dir += 2)
            {
                // 현재 방향으로 한 칸 이동한 위치 계산
                float nx = currentPoint.Item1 + direction.dx * dir;
                float ny = currentPoint.Item2 + direction.dy * dir;

                // 계산된 위치가 제거 타일 위치와 일치하는지 확인
                if (Mathf.Approximately(nx, removePoint.Item1) &&
                    Mathf.Approximately(ny, removePoint.Item2))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
