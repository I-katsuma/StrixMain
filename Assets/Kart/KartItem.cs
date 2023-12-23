using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムの基底クラス
/// </summary>
public class KartItem : MonoBehaviour
{
    // アイテムのタイプ
    public enum eType
    {
        None,

        Bullet,

        Max
    }

    protected long _ownerId = 0L;
    protected int _instanceIndex = 0;
    protected int _useTimeMsec = 0;
    protected Vector3 _usePosition = Vector3.zero;
    protected Vector3 _useVelocity = Vector3.zero;

    //　発射されてからの時間(ゲームが始まってからの時間ーアイテムを使用した時間)
    protected float pastTimeF => (KartTask.instance.currentRaceTimeMsec - _useTimeMsec) * 0.001f;// msec から secに変換

    public void DestroyItem()
    {
        KartPlayer.localPlayer.DestroyItem(_ownerId, _instanceIndex); // 誰のアイテムの何番目を削除するかRPCで
    }

    public void Setup(long ownerId, int instanceIndex, int useTimeMsec, Vector3 usePosition, Vector3 useVelocity)
    {
        _ownerId = ownerId;
        _instanceIndex = instanceIndex;
        _useTimeMsec = useTimeMsec;
        _usePosition = usePosition;
        _useVelocity = useVelocity;

        PostSetup();
    }


    protected virtual void PostSetup()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
