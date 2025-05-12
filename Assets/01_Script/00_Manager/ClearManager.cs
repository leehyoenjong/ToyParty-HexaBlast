using UnityEngine;

public class ClearManager : MonoBehaviour
{
    public static ClearManager instance;

    [SerializeField] UI_Clear_Profile Ui_Clear_Profile;
    public UI_Clear_Profile Get_UI_Clear_Profile() => Ui_Clear_Profile;

    [SerializeField] UI_Move_Profile Ui_Move_Profile;
    public UI_Move_Profile Get_UI_Move_Profile() => Ui_Move_Profile;

    int Cur_Move_Count;
    int Cur_Claer_Count;

    private void Awake()
    {
        instance = this;
    }

    public void Initaiilzed()
    {
        var stage_data = PlayManager.instance.Get_Stage_Data();
        Cur_Move_Count = stage_data.iMoveCount;
        Cur_Claer_Count = stage_data.iClear_Count;

        Ui_Clear_Profile.Initailzed();
        Ui_Move_Profile.Initailized();
    }

    /// <summary>
    /// 이동 업데이트
    /// </summary>
    public void Update_Move_Count()
    {
        Cur_Move_Count--;
        Ui_Move_Profile.Update_Move_Count(Cur_Move_Count);
    }

    /// <summary>
    /// 클리어 횟수 업데이트 
    /// </summary>
    public void Update_Clear_Count()
    {
        //스테이지 정보에서 클리어 kind 가져와 갯수 확인
        var stage_data = PlayManager.instance.Get_Stage_Data();
        var tilelist = TileManager.instance.Get_Tile;
        var count = tilelist.FindAll(x => x != null && x.Get_Tile_Kind() == stage_data.Clear_Kind);

        Cur_Claer_Count = count.Count;
        Ui_Clear_Profile.Update_Clear_Count(Cur_Claer_Count);
    }

    /// <summary>
    /// 모든 처리가 끝난 후 클리어 조건 체크
    /// </summary>
    public void Set_Clear()
    {
        //이동 횟수 및 클리어 횟수 남아 있다면 return
        if (Cur_Move_Count > 0 && Cur_Claer_Count > 0)
        {
            return;
        }

        //클리어 조건 만족 시 
        if (Cur_Claer_Count <= 0)
        {
            //클리어 처리 
            return;
        }

        //이동횟수가 없다면
        if (Cur_Claer_Count <= 0)
        {
            //실패처리
        }
    }
}