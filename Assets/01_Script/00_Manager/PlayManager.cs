using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public static PlayManager instance;
    [SerializeField] int StageNum;
    const string str_Stage = "Stage_Data_{0}";

    [Header("필요정보")]
    [SerializeField] UI_Grid Ui_Grid;
    public UI_Grid Get_UI_Grid() => Ui_Grid;

    //ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

    Dictionary<int, Stage_Data> D_Stage_Data = new Dictionary<int, Stage_Data>();

    bool isStay;
    public bool GetStay
    {
        get => isStay;
        set => isStay = value;
    }

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// 스테이지 데이터 가져오기
    /// </summary>
    public Stage_Data Get_Stage_Data()
    {
        if (!D_Stage_Data.ContainsKey(StageNum))
        {
            var data = Resources.Load(string.Format(str_Stage, StageNum)) as Stage_Data;
            D_Stage_Data.Add(StageNum, data);
        }
        return D_Stage_Data[StageNum];
    }

    private void Start()
    {
        StartCoroutine(IE_Play());
        ClearManager.instance.Initiailzed();
    }

    IEnumerator IE_Play()
    {
        isStay = true;
        Ui_Grid.Create_Tile_Slot();
        yield return new WaitForSeconds(1f);
        TileManager.instance.All_Scan_Boom();
        StartCoroutine(TileManager.instance.IE_Move_And_Boom());
    }
}