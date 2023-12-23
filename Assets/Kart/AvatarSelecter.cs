using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSelecter : MonoBehaviour
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
    public AvatarInfo[] avatartInfoList = null;

    public static AvatarSelecter instance = null;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
