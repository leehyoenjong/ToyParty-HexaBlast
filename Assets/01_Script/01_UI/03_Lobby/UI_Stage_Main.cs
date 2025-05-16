using System.Collections.Generic;
using UnityEngine;

public class UI_Stage_Main : MonoBehaviour
{
    [Header("슬롯 부모")]
    [SerializeField] GameObject G_Stage_Parent;
    [Header("플레이 팝업창")]
    [SerializeField] GameObject G_Play_Popup;

    UI_Stage_Slot[] L_Stage_Slot;

    /// <summary>
    /// 초기화
    /// </summary>
    public void Initialized()
    {
        Set_Stage_Slot();
    }

    /// <summary>
    /// 슬롯 정보처리
    /// </summary>
    void Set_Stage_Slot()
    {
        L_Stage_Slot = G_Stage_Parent.GetComponentsInChildren<UI_Stage_Slot>(true);

        var max = L_Stage_Slot.Length;
        for (int i = 0; i < max; i++)
        {
            var slot = L_Stage_Slot[i];
            slot.Initialized(i, Creaet_Play_Popup);
        }
    }

    /// <summary>
    /// 플레이 팝업창 생성 
    /// </summary>
    /// <param name="level"></param>
    void Creaet_Play_Popup(int level)
    {
        var popup = GameObject.Instantiate(G_Play_Popup, null).GetComponent<UI_Play_Popup>();
        popup.Initialized(level);
    }
}