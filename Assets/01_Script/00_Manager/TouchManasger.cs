using UnityEngine;

public class TouchManasger : MonoBehaviour
{
    public static TouchManasger instance;

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
        if (isInputActive && !currentInputActive && !PlayManager.instance.GetStay)
        {
            TileManager.instance.Reset();
        }

        // 현재 입력 상태 저장
        isInputActive = currentInputActive;
    }

    /// <summary>
    /// 첫 번째 타일 선택 (터치/클릭 시작)
    /// </summary>
    public void OnTileDown(UI_Tile tile)
    {
        if (PlayManager.instance.GetStay)
        {
            return;
        }

        // 첫 번째 타일 저장
        TileManager.instance.FirstTouch_Tile = tile;
        TileManager.instance.SecondTouch_Tile = null; // 두 번째 타일 초기화
    }

    /// <summary>
    /// 드래그 중 타일 진입 (두 번째 타일 선택)
    /// </summary>
    public void OnTileEnter(UI_Tile tile)
    {
        if (PlayManager.instance.GetStay)
        {
            return;
        }

        var first = TileManager.instance.FirstTouch_Tile;
        // 현재 입력 중인지 확인 (터치 또는 마우스 클릭)
        if (!isInputActive || first == null)
        {
            return; // 입력이 없거나 첫 번째 타일이 없으면 무시
        }

        // 첫 번째 타일과 다른 타일이면 두 번째로 저장
        if (first != tile)
        {
            TileManager.instance.SecondTouch_Tile = tile;
            PlayManager.instance.GetStay = true;
            StartCoroutine(TileManager.instance.IE_Swap());
        }
    }
}