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
        // 10秒経ったら弾が消える
        if (pastTime >= 5.0f && _ownerId == KartPlayer.localPlayer.id) // ⇦自分自身が発射した弾かどうか
        {
            // 時間がたった弾を消す
            DestroyItem();
        }
        transform.position = _usePosition + pastTime * _useVelocity;

    }


    private void OnTriggerEnter(Collider other)
    {
        KartPlayer otherPlayer = KartPlayer.GetPlayerByGameObject(other.gameObject);

        if(otherPlayer == null)
        {
            if(_ownerId == KartPlayer.localPlayer.id)
            {
                // 壁などに当たった弾を消す
                DestroyItem();
            }
            else if(otherPlayer == KartPlayer.localPlayer)
            {
                // 自分自身が発射した弾には一定時間当たらない
                if(_ownerId == KartPlayer.localPlayer.id && pastTimeF < 0.5f)// 自分自身が発射した弾かどうか
                {
                    return;
                }

                // 自分に当たった球を消す
                DestroyItem();
            }
        }


    }

}
