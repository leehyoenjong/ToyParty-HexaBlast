using UnityEngine;

public class TouchManasger : MonoBehaviour
{
    public static TouchManasger instance;

    UI_Tile FirstTouch_Tile;
    UI_Tile SecondTouch_Tile;
    
    // 입력 상태 추적 변수
    private bool isInputActive = false;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        // 현재 입력 상태 확인 (터치 또는 마우스 왼쪽 버튼)
        bool currentInputActive = Input.touchCount > 0 || Input.GetMouseButton(0);
        
        // 입력이 끝났을 때만 초기화
        if (isInputActive && !currentInputActive)
        {
            Debug.Log("입력 종료 - 삭제완료");
            Reset();
        }
        
        // 현재 입력 상태 저장
        isInputActive = currentInputActive;
    }

    /// <summary>
    /// 첫 번째 타일 선택 (터치/클릭 시작)
    /// </summary>
    public void OnTileDown(UI_Tile tile)
    {
        // 첫 번째 타일 저장
        FirstTouch_Tile = tile;
        SecondTouch_Tile = null; // 두 번째 타일 초기화
        Debug.Log($"첫번째 타일 선택 - {tile.gameObject.name}");
    }

    /// <summary>
    /// 드래그 중 타일 진입 (두 번째 타일 선택)
    /// </summary>
    public void OnTileEnter(UI_Tile tile)
    {
        // 현재 입력 중인지 확인 (터치 또는 마우스 클릭)
        if (!isInputActive || FirstTouch_Tile == null)
        {
            return; // 입력이 없거나 첫 번째 타일이 없으면 무시
        }

        // 첫 번째 타일과 다른 타일이면 두 번째로 저장
        if (FirstTouch_Tile != tile)
        {
            SecondTouch_Tile = tile;
            Debug.Log($"두번째 타일 선택 - {tile.gameObject.name}");
            // 두 타일의 위치 교환
            SwapTiles();
        }
    }

    /// <summary>
    /// 두 타일의 위치 교환
    /// </summary>
    private void SwapTiles()
    {
        if (FirstTouch_Tile != null && SecondTouch_Tile != null)
        {
            // 각 타일의 슬롯 가져오기
            var firstslot = FirstTouch_Tile.Get_Tile_Slot;
            var secondslot = SecondTouch_Tile.Get_Tile_Slot;

            // 위치 교환
            FirstTouch_Tile.Set_Swap(secondslot);
            SecondTouch_Tile.Set_Swap(firstslot);

            Reset();
            Debug.Log("위치 이동 완료 및 초기화");
        }
    }

    /// <summary>
    /// 모두 초기화
    /// </summary>
    void Reset()
    {
        FirstTouch_Tile = null;
        SecondTouch_Tile = null;
    }
}