using System.Linq;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    public static HintManager instance;

    void Awake()
    {
        instance = this;
    }

    const float MaxDelay = 1;
    float CurDelay;
    bool isHint;
    public bool Get_Hint() => isHint;

    static bool isHint_OnOff = true;
    public static bool Get_Hint_OnOff
    {
        get => isHint_OnOff;
        set => isHint_OnOff = value;
    }

    private void Update()
    {
        if (isHint || PlayManager.instance.GetStay || !isHint_OnOff)
        {
            return;
        }

        CurDelay += Time.deltaTime;

        if (CurDelay < MaxDelay)
        {
            return;
        }

        isHint = true;
        Set_HintOn();
    }

    /// <summary>
    /// 힌트 활성화 
    /// </summary>
    public void Set_HintOn()
    {
        //파괴 가능 리스트 가져오기 
        var list = TileManager.instance.Check_Destory_Tile();
        if (list == null)
        {
            return;
        }

        //힌트켜기
        foreach (var item in list)
        {
            item.GetTile.Set_Hint(true);
        }
    }

    /// <summary>
    /// 타일에 적용된 힌트 강제종료
    /// </summary>
    public void Set_Driect_Hint_Off()
    {
        //힌트 열어놨는지 확인
        if (!Get_Hint())
        {
            return;
        }

        //힌트 종료
        Reset();

        //힌트 켜진 타일들 끄기
        var slotlist = TileManager.instance.Get_Tile_Slot;
        var hintlist = slotlist.Where(x => x.GetTile != null && x.GetTile.Check_Hint());
        foreach (var item in hintlist)
        {
            item.GetTile.Set_Hint(false);
        }
    }

    /// <summary>
    /// 초기화
    /// </summary>
    public void Reset()
    {
        isHint = false;
        CurDelay = 0;
    }
}