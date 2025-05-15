using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayManager : MonoBehaviour
{
    public static PlayManager instance;


    [Header("필요정보")]
    [SerializeField] UI_Grid Ui_Grid;
    public UI_Grid Get_UI_Grid() => Ui_Grid;

    [SerializeField] UI_Play Ui_Play;
    public UI_Play GetUI_Play() => Ui_Play;

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

    private void Start()
    {
        StartCoroutine(IE_Play());
        ClearManager.instance.Initaiilzed();
        ScoreManager.instance.Initailized();
    }

    IEnumerator IE_Play()
    {
        isStay = true;
        Ui_Grid.Create_Tile_Slot();
        yield return new WaitForSeconds(1f);
        var result = TileManager.instance.All_Scan_Remove();
        if (result.Item1.Count > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }
        StartCoroutine(TileManager.instance.IE_Move_And_Boom());
    }
}