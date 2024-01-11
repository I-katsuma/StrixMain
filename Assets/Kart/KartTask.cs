using SoftGear.Strix.Unity.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KartTask : MonoBehaviour // ゲームの流れを制御
{
    public enum eState
    {
        Default, // デフォルト 0
        Ready, // 準備完了 1
        Racing, // レース中 2

        Max
    }

    private static readonly string[] RANK_SUFFIX_LIST = new string[]
    {
        "", "st", "nd", "rd", "th"
    };
    public static string GetRankText(int rank)
    {
        return $"{rank}{RANK_SUFFIX_LIST[Mathf.Clamp(rank, 0, RANK_SUFFIX_LIST.Length - 1)]}";
    }

    // UI
    public Text speedText = null;

    public Text percentText = null;
    public Text lapText = null;
    public Text sectionText = null;
    public GameObject captionRoot = null;
    public Text captionText = null;

    public RectTransform playerNameTextRoot = null;
    public Text playerNameTextBase = null;

    public Text rankingTextL = null;
    public Text rankingTextC= null;
    public Text rankingTextR = null;
    public Text rankingText = null;
    public RawImage itemImage = null;
    public Text[] itemCountTextList = null;

    public Transform playerRoot = null;
    public KartPlayer playerPrefab  = null;
    public  KartPlayer localPlayer = null;

    // ネットワーク関係
    private bool isRoomOwner => StrixNetwork.instance.isRoomOwner;

    // ゲームの状態
    public int state
    {
        get
        {
            return StrixNetwork.instance.room.GetState();
        }
        set
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("state", value);
            StrixNetwork.instance.SetRoom(StrixNetwork.instance.room.GetPrimaryKey(), properties, null, null);
        }
    }
    /// <summary>
    /// カスタムプロパティを取得
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <param name="defaltValue"></param>
    /// <returns></returns>
    private T GetCustomProperties<T>(string propertyName, T defaltValue)
    {
        Dictionary<string, object> properties = StrixNetwork.instance.room.GetProperties();
        object value;
        if(properties.TryGetValue(propertyName, out value) && value.GetType() == typeof(T))
        {
            return (T)value;
        }
        return defaltValue;
    }


    /// <summary>
    /// カスタムプロパティを送信
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyName"></param>
    /// <param name="sendValue"></param>

    private void SendCustomProperty<T>(string propertyName, T sendValue)
    {
        Dictionary<string, object> roomProperties = new Dictionary<string, object>();
        Dictionary<string, object> properties = new Dictionary<string, object>();
        properties.Add(propertyName, sendValue);
        roomProperties.Add("properties", properties);
        StrixNetwork.instance.SetRoom(StrixNetwork.instance.room.GetPrimaryKey(), roomProperties, null, null);
    }

    /// <summary>
    /// レース開始時間(Tick) <- 100ナノ秒
    /// </summary>
    public long raceStartTick
    {
        get
        {
            return GetCustomProperties("raceStartTick", 0L);
        }
        set { 
            SendCustomProperty("raceStartTick", value);
        }

    }

    /// <summary>
    /// レースが始まってからの時間(msec)
    /// </summary>
    public int currentRaceTimeMsec
    {
        get
        {
            return (int)((StrixNetwork.instance.roomSession.syncTimeClient.SychronizedTime.Ticks - raceStartTick) / (1000L * 10L));
        }
    }

    /// <summary>
    /// レースするコース
    /// </summary>
    public long raceCourse
    {
        get
        {
            return GetCustomProperties(nameof(raceCourse), 0L);
        }
        set
        {
            SendCustomProperty(nameof(raceCourse), value);
        }
    }


    // #16で消去
    // Player
    // public KartPlayer localPlayer = null;

    public static KartTask instance = null;

    void Awake()
    {
        // ネットワークが初期化されていなければタイトルに遷移
        if (StrixNetwork.instance.room == null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
            return;
        }

        // プレイヤー生成
        localPlayer = GameObject.Instantiate<KartPlayer>(playerPrefab);

        instance = this;
    }
    // Start is called before the first frame update

    void Start()
    {
        StartCoroutine(MainProc());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        // m / s => km / h
        speedText.text = $"{(int)(localPlayer.Speed * 3600.0f / 1000.0f)}";
        percentText.text = $"{(int)(localPlayer.lapRatio* 100.0f)}%";
        lapText.text = $"{localPlayer.lapCount}";
        sectionText.text = $"{localPlayer.sectionIndex}";

        // Ranking
        int myRank  = 1;
        string rankingStrL = string.Empty;
        string rankingStrC = string.Empty;
        string rankingStrR = string.Empty;

        int n = KartPlayer.playerList.Count;
        for (int i = 0; i < n; i++)
        {
            int rank = i + 1;
            KartPlayer player = KartPlayer.GetPlayerByRank(rank);
            if (player == localPlayer)
            {
                myRank = rank; // 自分のランク
            }
            rankingStrL += $"{GetRankText(rank)}\n";
            rankingStrC += $"{player.strixReplicator.roomMember.GetName()}\n";
            int playerSyncGoalTime = player.syncGoalTimeMsec;
            if(playerSyncGoalTime > 0)
            {
                // ゴールしているなら
                int minutes = playerSyncGoalTime / (60 * 1000);
                int seconds = playerSyncGoalTime / 1000 % 60;
                int cSeconds = playerSyncGoalTime / 10 % 100;
                rankingStrR += $"{minutes:D2}:{seconds:D2}:{cSeconds:D2}\n";
            }
            else
            {
                // まだゴールしてない人の
                rankingStrR += $"{(int)(player.syncLapCountAndRatio)} : {((int)(player.syncLapCountAndRatio * 100.0f) % 100):D2}\n";

            }
        }
        rankingTextL.text  = rankingStrL ;
        rankingTextC.text = rankingStrC ;
        rankingTextR.text = rankingStrR ;
        rankingText.text = GetRankText(myRank);

        // Item
        itemImage.texture = KartItemManager.instance.itemTextureList[(int)localPlayer.showItemType];
        int itemCount = localPlayer.showItemCount;
        for (int i = 0; i < itemCountTextList.Length; i++) 
        {
            itemCountTextList[i].text = (itemCount > 0) ? $"{itemCount}" : string.Empty; // アイテムを持っていたら所持数を表示
        }

    }

    private IEnumerator MainProc()
    {
        // ルームが締め切られるまで待機
        while (StrixNetwork.instance.room.GetIsJoinable())
        {
            // ホストの場合は締切処理
            if(isRoomOwner)
            {
                if(Input.GetKeyDown(KeyCode.Return))
                {
                    //　締め切る
                    //var properties = StrixNetwork.instance.room.GetProperties();
                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    properties.Add("isJoinable", false);
                    StrixNetwork.instance.SetRoom(StrixNetwork.instance.room.GetPrimaryKey(), properties, null, null);
                }
            }
            yield return null;
        }

        // 自分をスタートライングリットへ並ばせる
        localPlayer.SetToReady();

        // 全員が準備完了するまで待機
        yield return new WaitWhile(() => { return KartPlayer.GetMinState() < (int)eState.Ready; });

        // スタートタイムタイミングを合わせる 開始時間を求める
        while(state < (int)eState.Ready)
        {
            if(isRoomOwner)
            {
                long tick = StrixNetwork.instance.roomSession.syncTimeClient.SychronizedTime.Ticks; // 詳細な現在の時間を取る
                raceStartTick = tick + 3L /* 3sec */ * 1000L * 1000L * 10L; // 3秒後にスタート
                state = (int)eState.Ready; //ステート切り替え
            }

            yield return null;
        }

        // カウントダウンの演出
        captionRoot.SetActive(true);
        while(true)
            {
            // syncTimeClient Strixサーバーの時間
            long tick = StrixNetwork.instance.roomSession.syncTimeClient.SychronizedTime.Ticks;
            long leftCountLong = (raceStartTick - tick) / (1000L * 1000L * 10L) + 1L;
            int leftCount = Mathf.Clamp((int)leftCountLong, 1, 3);
            captionText.text = $"{leftCount}";
            if(tick >= raceStartTick) // レース開始時間になるまで待機
            {
                break;
            }
            yield return null;
        }
        captionRoot.SetActive(false);

        if(isRoomOwner)
        {
            state = (int)eState.Racing;
        }

        // 自分自身をスタートさせ
        localPlayer.SetToStart();

        // 自分がスタートしたらゴールするまで待機

        yield return new WaitWhile(() => { return localPlayer.syncGoalTimeMsec == 0; });

        // ゴールと表記
        captionRoot.SetActive(true);
        captionText.text = "GOAL"; 

        //　終了
        yield break;
    }

    /// <summary>
    /// コース選択ボタンが押された
    /// </summary>
    public void OnClickCourseSelectButton()
    {
        CourseSelector.instance.StartSelect((courseIndex/* 何番目のボタンか*/) =>
        {
            // 実際にボタンが押されたとき
            if (courseIndex >= 0)
            {
                // コールの番号を設定
                raceCourse = courseIndex;

                
            }
        });
    }
}
