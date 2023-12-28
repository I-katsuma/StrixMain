using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテム全体の管理
/// </summary>
public class KartItemManager : MonoBehaviour
{
    public KartItem kartItemBullet = null; // 手裏剣のプレハブ
    public KartItem kartItemBomb = null;
    public Exprotion explosion = null; // 爆発プレハブ

    public static int localItemInstanceIndex = 0; // 自分自身が使うｱｲﾃﾑが次何番目か

    private Dictionary<long, Dictionary<int, KartItem>> _itemDic = new Dictionary<long, Dictionary<int, KartItem>>();  // <ownerId, <InstanceIndex, KartItem>>

    public static KartItemManager instance;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// アイテムを使う
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="instanceIndex"></param>
    /// <param name="itemType"></param>
    /// <param name="useTimeMsec"></param>
    /// <param name="usePosition"></param>
    /// <param name="useVelocity"></param>
    public void UseItem(long ownerId, int instanceIndex, KartItem.eType itemType, int useTimeMsec, Vector3 usePosition, Vector3 useVelocity)
    {
        KartItem newItem = null;
        switch (itemType)
        {
            case KartItem.eType.Bullet:
                // 手裏剣を生成
                newItem = GameObject.Instantiate<KartItem>(kartItemBullet);
                usePosition += 0.5f * Vector3.up; // 手裏剣の位置上方向に修正
                break;

            case KartItem.eType.Bomb:
                // 爆弾を生成
                newItem = GameObject.Instantiate<KartItem>(kartItemBomb);
                usePosition += 0.5f * Vector3.up; // 手裏剣の位置上方向に修正
                break;

            default: break;
        }

        newItem.Setup(ownerId, instanceIndex, useTimeMsec, usePosition, useVelocity);

        if(!_itemDic.ContainsKey(ownerId))
        {
            _itemDic.Add(ownerId, new Dictionary<int, KartItem>());
        }
        _itemDic[ownerId].Add(instanceIndex, newItem);
    }

    /// <summary>
    /// アイテムの削除
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="instanceIndex"></param>
    public void DestroyItem(long ownerId, int instanceIndex)
    {
        // 存在チェック
        if (_itemDic.ContainsKey(ownerId) && _itemDic[ownerId].ContainsKey(instanceIndex))
        {
            // 削除して管理からも除外
            KartItem item = _itemDic[ownerId][instanceIndex];

            switch (item)
            {
                // itemの中身の型がKartItemBombだったら、itemにキャストしたものがbombに入ってくれる
                // var test = item as KartItemBomb; ⇦これをswitchで書くとこうなる
                // if(test != null
                //{
                // item はKartItemBombの型
                //}
                case KartItemBomb bomb:
                    // 消そうとしているitemはkartItemBombの型
                    // 爆風を発生させる
                    InstantiateExplosion(bomb.transform.position - 0.5f * Vector3.up);
                    break;
                    default : break;
            }
            GameObject.Destroy(item.gameObject);
            _itemDic[ownerId].Remove(instanceIndex);
        }
    }
    
    /// <summary>
    /// 爆風を発生させる
    /// </summary>
    /// <param name="position"></param>
    public void InstantiateExplosion(Vector3 position)
    {
        var exp = GameObject.Instantiate<Exprotion>(explosion, null, false);
        exp.transform.position = position;
    }
}
