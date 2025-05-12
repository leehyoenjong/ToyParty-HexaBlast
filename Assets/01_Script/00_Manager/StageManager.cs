using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    const string str_Stage = "Stage_Data_{0}";
    static Dictionary<int, Stage_Data> D_Stage_Data = new Dictionary<int, Stage_Data>();

    static int CurStage;

    /// <summary>
    /// 스테이지 데이터 가져오기
    /// </summary>
    public static Stage_Data Get_Stage_Data(int stage)
    {
        CurStage = stage;
        if (!D_Stage_Data.ContainsKey(CurStage))
        {
            var data = Resources.Load(string.Format(str_Stage, CurStage)) as Stage_Data;
            D_Stage_Data.Add(CurStage, data);
        }
        return D_Stage_Data[CurStage];
    }

    /// <summary>
    /// 스테이지 데이터 가져오기
    /// </summary>
    public static Stage_Data Get_Stage_Data()
    {
        if (!D_Stage_Data.ContainsKey(CurStage))
        {
            var data = Resources.Load(string.Format(str_Stage, CurStage)) as Stage_Data;
            D_Stage_Data.Add(CurStage, data);
        }
        return D_Stage_Data[CurStage];
    }
}