using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 爆弾クラス
/// </summary>
public class KartItemBomb : KartItem
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
        if (pastTime >= 1.0f && _ownerId == KartPlayer.localPlayer.id) // ⇦自分自身が発射した弾かどうか
        {
            // 時間がたった爆弾を消す
            DestroyItem();
        }
        // 動き
        transform.position = _usePosition + pastTime * _useVelocity + 3.5f * MathF.Sin(MathF.PI * pastTime) * Vector3.up;

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
                // 弾に当たったらクラッシュ
                otherPlayer.StartCrash();

                // 自分に当たった球を消す
                DestroyItem();
            }
        }
    }
}
