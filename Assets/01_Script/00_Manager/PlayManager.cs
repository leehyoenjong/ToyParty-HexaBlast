using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public static PlayManager instance;
    [SerializeField] int StageNum;
    const string str_Stage = "Stage_Data_{0}";

    [SerializeField] UI_Grid Ui_Grid;

    Dictionary<int, Stage_Data> D_Stage_Data = new Dictionary<int, Stage_Data>();

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
        Ui_Grid.Create_Tile_Slot();
        TileManager.instance.All_Scan_Boom();
        StartCoroutine(TileManager.instance.IE_Move_And_Boom());
    }

}