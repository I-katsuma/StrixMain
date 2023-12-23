using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using SoftGear.Strix.Unity.Runtime;
using System.Xml.Linq;
using Unity.VisualScripting;

public class KartPlayer : StrixBehaviour
{
    const float MAX_SPEED = 40.0f;
    public Rigidbody myRigidBody = null;

    public Transform KartRoot = null;
    //public Transform[] KartRoots = null;

    public Transform cameraLocator = null;

    public Transform avatarRoot = null;

    float _accel = 12.0f;
    float _speed = 0.0f;
    public float Speed => _speed;
    Vector3 _moveVelocity = Vector3.zero;
    // 壁から受ける力
    Vector3 _forceVelocity = Vector3.zero;
    Vector3 _mixedVelocity = Vector3.zero;

    float _rotationAccel = 360.0f;
    float _rotationSpeed = 0.0f;
    float _rotation = 0.0f;

    int _sectionIndex = 0;
    int _lapCount = 0;
    float _lapRatio = 0.0f;// 0 < 1の値を計算

    public int sectionIndex => _sectionIndex;
    public int lapCount => _lapCount;
    public float lapRatio => _lapRatio;
    // UI
    private Text _nameText = null;

    // Avatar 今表示されているアバターの情報
    private int _currentAvatarIndex = -1; // 実際のインデックス これを感知して下のSelectedIndexと入れ替え
    private GameObject _avatar = null;

    //Crash
    private float _crashTimer = 0.0f;
    private bool isCrashing => (_crashTimer > 0.0f);

    // 同期情報
    [StrixSyncField] private int _syncState = 0;
    [StrixSyncField] float _syncLapCountRatio = 0.0f;
    public float syncLapCountAndRatio => _syncLapCountRatio;
    [StrixSyncField] private int _syncGoalTimMsec = 0; // ゴールした時間
    public int syncGoalTimeMsec => _syncGoalTimMsec;

    // どのキャラクターを選択したかを示す
    [StrixSyncField] private int _syncSelectedAvatartIndex = 0;
    public int syncSelectedAvatarIndex { set { _syncSelectedAvatartIndex = value;  }} // セットプロパティ

    // プレイヤー全体管理
    private static List<KartPlayer> _playerList = new List<KartPlayer>();
    public static List<KartPlayer> playerList => _playerList;
    private static KartPlayer _localPlayer = null;
    public static KartPlayer localPlayer => _localPlayer;
    
    /// <summary>
    /// プレイヤーの最小ステートを求める  (1人だけでも準備になってなかったらデフォルトを返す)
    /// </summary>
    /// <returns></returns>
    public static int GetMinState ()
    {
        int minState = (int)KartTask.eState.Max;
        foreach (var p in _playerList)
        {
            minState = Mathf.Min(minState, p._syncState);
        }
        return minState;
    }

    /// <summary>
    /// ランクに応じたプレイヤーを取得
    /// </summary>
    /// <param name="rank"></param>
    /// <returns></returns>
    public static KartPlayer GetPlayerByRank(int rank)
    {
        List<KartPlayer> playerListSortedByRank = new List<KartPlayer>(_playerList);
        playerListSortedByRank.Sort((a, b) =>
        {
            // ゴールしてる場合のほうが順位が上
            // ゴールしてる場合はゴールタイムがちいさい方が上
            // ゴールしてない場合の方が順位が下
            // ゴールしてない場合は周回数が大きい方が上
           int scoreA = (a._syncGoalTimMsec > 0) ? (int.MaxValue - a._syncGoalTimMsec) : (int)(a._syncLapCountRatio * 10000.0f); // Aのスコア
           int scoreB = (b._syncGoalTimMsec > 0) ? (int.MaxValue - b._syncGoalTimMsec) : (int)(b._syncLapCountRatio * 10000.0f); // Bのスコア
            return scoreB - scoreA; // スコアの降順で並ぶ
        });
        return playerListSortedByRank[Mathf.Clamp(rank -1, 0, playerListSortedByRank.Count -1)];
    }

    private static void SortPlayerList()
    {
        _playerList.Sort((a, b) =>
        {
            long diff = a.strixReplicator.roomMember.GetPrimaryKey() - b.strixReplicator.roomMember.GetPrimaryKey();
            // return (diff < 0) ? -1 : (diff > 0) ? 1 : 0;
            if (diff < 0)
            {
                return -1;
            }
            if (diff > 0)
            {
                return 1;
            }
            return 0;
        });
    }

    //プレイヤーUtil 



    private void OnDestroy() // 入室中プレイヤーリストから抜ける（ログアウトした）
    {
        // 登録解除
        if (_localPlayer = this)
        {
            _localPlayer = null;
        }
        _playerList.Remove(this);
        SortPlayerList();

        // UI
        if(_nameText != null)
        {
            GameObject.Destroy(_nameText.gameObject);
        }

        //UI
        if(_nameText != null)
        {
            GameObject.Destroy(_nameText.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // 登録
        if (isLocal)
        {
            _localPlayer = this;
        }
        _playerList.Add(this);
        SortPlayerList();

        if(!isLocal)
        {
            // 他人の場合は物理挙動を無効化
            myRigidBody.isKinematic = true;
        }

        // 階層 (PlayerRoot直下にPlayerが生成される)
        // Playerの生成はTaskからがおすすめ　Playerからだと他のプレイヤーに反映されない？
        transform.SetParent(KartTask.instance.playerRoot, true);

        // UI
        if (!isLocal) // 自分ではない
        {
            // 他人のカートのUI処理
            Text textBase = KartTask.instance.playerNameTextBase;
            _nameText = GameObject.Instantiate<Text>(textBase, textBase.transform.parent, true);
            _nameText.transform.localPosition = Vector3.zero;
            _nameText.text = strixReplicator.roomMember.GetName();
        }
        
    }


    // Update is called once per frame
    void Update()
    {
        // TEST キーボードでアバター変更
        if (isLocal)
        {
            // アバター関係
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                _syncSelectedAvatartIndex = 0;
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _syncSelectedAvatartIndex = 1;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _syncSelectedAvatartIndex = 2;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _syncSelectedAvatartIndex = 3;
            }

            // アイテム関係
            if(Input.GetKeyDown(KeyCode.Z))
            {
                UseItem(KartItem.eType.Bullet);
            }
        }

        if (_syncSelectedAvatartIndex != _currentAvatarIndex)
        {
            //アバター選択された
            ChangeAvatar();
        }
    }

    private void LateUpdate()
    {
        if (!isLocal) // ネットワーク越しの他のプレイヤーのカートなら
        {
            // UI　 以下のUIの処理が実行される
            Vector3 kartPos = transform.position;
            Camera camera = Camera.main;
            Transform cameraTransform = camera.transform;
            // Vector3.Dotは2つのベクトル内積の角を求める 　カートの位置からカメラの位置を引いている
            float dot = Vector3.Dot(cameraTransform.forward, kartPos - cameraTransform.position);

            if(dot > 0.0f)
            {
                // カメラよりもカートが前にいる
                _nameText.gameObject.SetActive(true);
                Vector3 targetScreenPos = camera.WorldToScreenPoint(kartPos + Vector3.up);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(KartTask.instance.playerNameTextRoot, targetScreenPos, null, out Vector2 uiLocalPos);
                _nameText.rectTransform.anchoredPosition = uiLocalPos;

            }
            else
            {
                _nameText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 値が近いか調べる
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private bool CheckNearlyEqual(float a, float b)
    {
        return Mathf.Abs(a - b) < 0.01f;
    }

    private void FixedUpdate()
    {
        // クラッシュ処理
        _crashTimer = Mathf.Max(_crashTimer - Time.fixedDeltaTime, 0.0f);

        KartCource.eAttribute attr = KartCource.instance.GetAttribute(transform.position);
        bool isDart = (attr == KartCource.eAttribute.Dart);

        // 他人の場合は行わない
        if (!isLocal)
        {
            // Visual
            UpdateVisual(attr);

            return;
        }

        // ここから下はレース準備中は行わない
        if(_syncState == (int)KartTask.eState.Ready)
        {
            // Camera
            UpdateCamera();

            UpdateLap();

            return;
        }
        // 他人の場合は物理挙動を無効化

        // スペースキーが押されたら加速
        bool isAccel = Input.GetKey(KeyCode.Space);
        if (isAccel)
        {
            _speed = Mathf.Min(_speed + _accel * Time.fixedDeltaTime, MAX_SPEED);
        }

        // 壁からの力の判定
        Vector3 velocity = myRigidBody.velocity;
        velocity.y = 0.0f; // Yゼロに
        if(CheckNearlyEqual( _mixedVelocity.x, velocity.x) && CheckNearlyEqual(_mixedVelocity.z, velocity.z))
        {
            //　壁から何も力を受けてない

        }
        else
        {
            // 壁から何らかの力を受けた(ぶつかった時)
            Vector3 diff = velocity - _mixedVelocity;
            Vector3 diffN = diff.normalized;
            float diffM = diff.magnitude;
            _forceVelocity = 4.0f * Mathf.Sqrt(diffM) * diffN; 
            // Debug.Log($"{_mixedVelocity}, {velocity},{_forceVelocity}");
        }

        // 抵抗処理（減速）
        _speed *= isDart ? 0.98f : 0.99f;
        _forceVelocity *= isDart ? 0.91f : 0.97f;

        // 反映
        _moveVelocity = _speed * transform.forward;
        _mixedVelocity = _moveVelocity + _forceVelocity; // 今までの速さ+壁にぶつかった時のチカラ
        myRigidBody.velocity = _mixedVelocity; // <= 変更
        myRigidBody.angularVelocity = Vector3.zero;

        // 左右の矢印キーでステアリング
        float handle = 0.0f;
        if (Input.GetKey(KeyCode.LeftArrow))
        { 
        handle -= 1.0f;
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            handle += 1.0f;
        }
        if(isCrashing)
        {
            handle = 0.0f;
        }
        if(handle == 0.0f)
        {
            _rotationSpeed *= 0.9f;
        }

        _rotationSpeed += _rotationAccel * handle * Time.fixedDeltaTime;
        _rotationSpeed = Mathf.Clamp(_rotationSpeed, -120.0f, 120.0f);

        _rotation += _rotationSpeed * Time.fixedDeltaTime;
        myRigidBody.MoveRotation(Quaternion.Euler(0.0f, _rotation, 0.0f));

        // Visual
        UpdateVisual(attr);

         // Camera
        UpdateCamera();

        UpdateLap();

        // ラップの処理
        int sectionIndex;
        float rationInSection, ratioOfAll;
        if(KartCource.instance.GetPercentOnCource(transform.position, out sectionIndex, out rationInSection, out ratioOfAll))
        { 
            // 周回陛下判定（スタートラインを横切ったかどうか）
            if(_sectionIndex == KartCource.instance.sectionCount - 1 && sectionIndex == 0) 
            {
                _lapCount++;
            }
            else if(_sectionIndex == 0 && sectionIndex == KartCource.instance.sectionCount - 1)
            {
                _lapCount--;
            }
            _sectionIndex = sectionIndex;
            _syncLapCountRatio = ratioOfAll;
        }
    }

    private void UpdateVisual(KartCource.eAttribute attr)
    {
        bool isDart = (attr == KartCource.eAttribute.Dart);

        // 見た目の更新(ダートゾーンの処理)
        float swing = isDart ? 0.03f : 0.01f;
        float speedClip = isDart ? 10.0f : 20.0f;
        float ratio = Mathf.Min(_speed, speedClip) / speedClip;

        // 上下にスウィング
        KartRoot.transform.localPosition = new Vector3(0.0f, ((_speed >= 1.0f) ? (Random.Range(-swing, swing) * ratio) : 0.0f), 0.0f);

        float angle = _rotationSpeed * 0.1f;
        angle += _crashTimer * _crashTimer * 360.0f;
        KartRoot.transform.localEulerAngles = new Vector3(0.0f, angle, 0.0f);
    }

    private void UpdateCamera()
    {
        // カメラ補間
        cameraLocator.localPosition = new Vector3(0.0f, 0.0f, 0.035f * _speed);
        float t = Mathf.Min(30.0f * Time.fixedDeltaTime, 1.0f);
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, cameraLocator.position, t);
        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, cameraLocator.rotation, t);
    }

    private void UpdateLap()
    {
        int sectionIndex;
        float rationInSection, ratioOfAll;
        if (KartCource.instance.GetPercentOnCource(transform.position, out sectionIndex, out rationInSection, out ratioOfAll))
        {
            // 周回経過判定（スタートラインを横切ったかどうか）
            if (_sectionIndex == KartCource.instance.sectionCount - 1 && sectionIndex == 0)
            {
                _lapCount++;
                if(_lapCount > 1 /* ゴールまでの周回数 */&& _syncGoalTimMsec == 0)
                {
                    // ゴール！
                    _syncGoalTimMsec = KartTask.instance.currentRaceTimeMsec; // 現在の時間(msec)をゴールした時間として記録
                }
            }
            else if (_sectionIndex == 0 && sectionIndex == KartCource.instance.sectionCount - 1)
            {
                _lapCount--;
            }
            _sectionIndex = sectionIndex;
            _syncLapCountRatio = ratioOfAll;
        }
        _syncLapCountRatio = Mathf.Max(_lapCount + _lapRatio, 0.0f);
    }

    /// <summary>
    /// スターティンググリッドにならばせる
    /// </summary>
    public void SetToReady()
    {
        // 初期化
        _speed = 0.0f;
        _rotation = 0.0f;
        _rotationSpeed = 0.0f;
        _moveVelocity = Vector3.zero;
        _forceVelocity = Vector3.zero;
        _mixedVelocity = Vector3.zero;
        myRigidBody.velocity = Vector3.zero;
        myRigidBody.angularVelocity = Vector3.zero;
        _sectionIndex = KartCource.instance.sectionCount - 1;
        _lapCount = 0;
        _syncLapCountRatio = 0.0f;
        _syncGoalTimMsec = 0;
        _crashTimer = 0.0f;

        // プレイヤーの位置設定
        int thisPlayerIndex = _playerList.IndexOf(this); // 自分自身が何番目か調べる
        myRigidBody.MovePosition(KartCource.instance.GetStartingPoint(thisPlayerIndex));
        myRigidBody.MoveRotation(Quaternion.identity);

        // State
        _syncState = (int)KartTask.eState.Ready;
    }

    public void SetToStart()
    {
        // 動けるようにする
        _syncState = (int)KartTask.eState.Racing;
    }

    /// <summary>
    /// アバターを変更
    /// </summary>
    private void ChangeAvatar()
    {
        
        var avatartInfoList = AvatarSelecter.instance.avatartInfoList;

        //値が変化していない　または　値が無効な場合　なにもしない
        if(_syncSelectedAvatartIndex == _currentAvatarIndex || _syncSelectedAvatartIndex < 0 || avatartInfoList.Length <= _syncSelectedAvatartIndex)
        {
            return;
        }

        // 更新
        _currentAvatarIndex = _syncSelectedAvatartIndex;
　
            // デフォを削除
            if(_avatar != null)
             {
                GameObject.Destroy(_avatar);
                _avatar = null;
            }

        var avatarPrefab = avatartInfoList[_currentAvatarIndex].avatartPrefab; // 選択したアバターのインデック
        _avatar = GameObject.Instantiate(avatarPrefab, avatarRoot, false); // Prefab生成

        // VRM (アニメーター情報取得)
        Animator animator = _avatar.GetComponentInChildren<Animator>(); // アバターからアニメータ～を取得
        if(animator == null)
        {
            // アニメーターがなければVRMじゃない
            return;
        }

        // 姿勢制御用のオブジェクトを生成
        GameObject baseObj = GameObject.Instantiate(AvatarSelecter.instance.kartRidingVrmSase);

        // 姿勢情報のコピー
        Transform baseChild = baseObj.transform.GetChild(0); // vrmオブジェクトを取得
        _avatar.transform.localPosition = baseChild.localPosition;
        _avatar.transform.localScale = baseChild.localScale;

        Animator baseAnimator = baseObj.GetComponentInChildren<Animator>();
        AvatarSelecter.CopyPose(baseAnimator, animator);

        GameObject.Destroy(baseObj);
        
    }

    #region RPC

    public void UseItem(KartItem.eType itemType)
    {
        // RPC(全員に送る) アイテムを使うたび、１ずつ加算される
        RpcToAll(nameof(UseItemRPC), KartItemManager.localItemInstanceIndex++, itemType, KartTask.instance.currentRaceTimeMsec, 
            transform.position, 25.0f * transform.forward); // ⇦25 m/s で前方に発射
    }

    [StrixRpc]
    private void UseItemRPC(int instanceIndex, KartItem.eType itemType, int useTimeMsec, Vector3 usePosition, Vector3 useVelocity, StrixRpcContext strixRpcContext)
    {
        // 送った人のIDを取得
        // だれがRPC を実行したか（誰がアイテムを使ったか）
        long senderId = strixRpcContext.sender.GetPrimaryKey();

        KartItemManager.instance.UseItem(senderId, instanceIndex, itemType, useTimeMsec, usePosition, useVelocity);
    }
    #endregion


    #region
    /// <summary>
    /// クラッシュ開始
    /// </summary>
    public void StartCrash()
    {
        // 全員に送る
        RpcToAll(nameof(StartCrashRpc));
    }

    /// <summary>
    /// 実際のクラッシュ
    /// </summary>
    [StrixRpc]
    private void StartCrashRpc()
    {
        // タイマー設定
        _crashTimer = 2.0f;
    }
    #endregion
}