using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CourseSelector : SelectorBase
{
    /// <summary>
    ///  コースに関する情報
    /// </summary>
    [System.Serializable]
    public class CourseInfo
    {
        public string name = string.Empty; // 管理用
        public string disName = string.Empty; //表示用
        public Texture thumbnailTexture = null; // サムネイル
    }

    public CourseInfo[] courseInfoList = null; // 実際のコース情報

    public override int buttonCount => courseInfoList.Length; // ボタンの数

    public static CourseSelector instance = null; // シングルトン

    private void OnDestroy()
    {
        instance = null;
    }
    private void Awake()
    {
        instance = this;
    }

    protected override void SetupButton(SelectorButton button, int index)
    {
        // コース情報
        var courseInfo = courseInfoList[index];

        // ボタンの初期化
        button.Initialze(this, index, courseInfo.thumbnailTexture, courseInfo.disName);
    }
 
}
