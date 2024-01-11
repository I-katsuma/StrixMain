using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectorButton : MonoBehaviour
{
    public RectTransform rectTransform = null;
    public RawImage rawImage = null;
    public Text nameText = null;

    protected SelectorBase _selector = null;
    protected int _index = 0;

    public void Initialze(SelectorBase selector, int index, Texture texture, string name)
    {
        _selector = selector;
        _index = index;
        rawImage.texture = texture;
       nameText.text = name;
    }

    /// <summary>
    /// ボタンが押されたときのコールバック
    /// </summary>
    public void OnClick()
    {
        _selector.OnClickButton(_index);
    }

}
