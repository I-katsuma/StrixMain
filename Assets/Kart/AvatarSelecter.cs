using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarSelecter : SelectorBase
{
    /// <summary>
    /// アバター情報
    /// </summary>
    [System.Serializable]
    public class AvatarInfo
    {
        public GameObject avatartPrefab = null; // アバター自体のプレハブ
        public Texture thumbnailTexure = null;  // サムネイル画像
        public string avatartname  = string.Empty; // アバターの名前
    }

    public GameObject kartRidingVrmSase = null; // 座っている姿勢情報
    public AvatarInfo[] avatartInfoList = null; // 実際のアバター情報の配列

    public override int buttonCount => avatartInfoList.Length; // ボタンの数

    public static AvatarSelecter instance = null; // シングルトン

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    protected override void SetupButton(SelectorButton button, int index)
    {
        // コース情報
        var avatarInfo = avatartInfoList[index];

        // ボタンの初期化
        button.Initialze(this, index, avatarInfo.thumbnailTexure, avatarInfo.avatartname);
    }

    #region Util

    /// <summary>
    /// ポーズをコピー
    /// </summary>
    /// <param name="srcAnimator"></param>
    /// <param name="dstAnimator"></param>
    public static void CopyPose(Animator srcAnimator, Animator dstAnimator)
    {
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++) // 全部の姿勢をコピー
        {
            Transform srcTransform  = srcAnimator.GetBoneTransform((HumanBodyBones)i);
            Transform dstTransform = dstAnimator.GetBoneTransform((HumanBodyBones)i);
            if(srcTransform != null && dstTransform != null) 
            {
                dstTransform.localRotation = srcTransform.localRotation; // 人間の関節は回転しかない
            }
        }
    }
    #endregion
}
