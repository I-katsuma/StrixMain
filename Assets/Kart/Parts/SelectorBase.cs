using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorBase : MonoBehaviour
{
    public Canvas canvas = null;
    public SelectorButton buttonBase = null;
    public bool isCloseOnSelected = false; // ボタンが押されたときに閉じるか

    // ボタン配置用の変数
    public int buttonIntervalX = 100; // ボタンの間隔 X
    public int buttonIntervalY = 100; // Y
    public int buttonCountMaxX = 5; // ボタンの配置数

    public virtual int buttonCount => 0; // ボタンの数（全体）継承先で変更

    protected System.Action<int> _onSelectedCallback = null; // ボタンが押されたときのコールバック

    // Start is called before the first frame update
    void Start()
    {
        canvas.gameObject.SetActive(true);

        // ボタン初期化
        int n = buttonCount;
        int nx = Mathf.Min(n, buttonCountMaxX);
        int ny = (n + buttonCountMaxX - 1) / buttonCountMaxX;
        for (int i = 0; i < n; i++)
        {
            // ぼたんの生成
            var newButton = GameObject.Instantiate<SelectorButton>(buttonBase, buttonBase.transform.parent, true);
            // ボタンの座標の設定
            newButton.rectTransform.anchoredPosition = new Vector2(
                (i % nx - (nx - 1) * 0.5f) * buttonIntervalX,
                (i / nx - (ny - 1) * 0.5f) * buttonIntervalY);
            // 準備 SET UP
            SetupButton(newButton, i);
        }
        // 非表示化
        buttonBase.gameObject.SetActive(false);
        canvas.gameObject.SetActive(false);

    }

    /// <summary>
    /// 選択を開始する処理
    /// </summary>
    /// <param name="onSelectedCallBack"></param>
    public void StartSelect(System.Action<int> onSelectedCallBack)
    {
        // コールバックを設定
        _onSelectedCallback = onSelectedCallBack;

        // 開く
        canvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// ボタンをセットアップ
    /// </summary>
    /// <param name="button"></param>
    /// <param name="index"></param>
    protected virtual void SetupButton(SelectorButton button, int index)
    {
        // do nothing 継承先で実装
    }


    #region
    /// <summary>
    /// ボタンが押されたときのコールバック
    /// </summary>
    /// <param name="index"></param>
    public void OnClickButton(int index)
    {
        Debug.Log("OnClickButton :"  + index);

        _onSelectedCallback?.Invoke(index);

        // 終了
        if(isCloseOnSelected)
        {
            // 閉じる
            canvas.gameObject.SetActive(false);
        }
    }
    #endregion
}
