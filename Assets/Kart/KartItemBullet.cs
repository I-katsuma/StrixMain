using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手裏剣ｸﾗｽ
/// </summary>
public class KartItemBullet : KartItem 
{
    protected override void PostSetup()
    {
        UpdatePosition();
    }


    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
    }

    /// <summary>
    /// 位置などを更新
    /// </summary>
    private void UpdatePosition()
    {
        // アイテムが発射されて何秒経過したか
        float pastTime = pastTimeF;

        transform.position = _usePosition + pastTime * _useVelocity;

        /*
        // 10秒経ったら弾が消える
        if(pastTime >= 10.0f)
        {

        }
        */
    }

    /*
    private void OnTriggerEnter(Collider other)
    {
        KartPlayer otherPlayer = null;
        if(otherPlayer == null)
        {
            if(_ownerId == KartPlayer.localPlayer.id)
            {
                DestroyItem();
            }
        }
        else if(otherPlayer == KartPlayer.localPlayer) 
        { 
            if(_oenerId == KartPlayer.localPlayer.id. && pastTimeF < 0.5f)
            {
                return;
            }

            // 弾に当たったらクラッシュ
            otherPlayer.StartCrash();

            // 自分に当たったら弾を消す
            DestroyItem();
        }
    }
    */
}
