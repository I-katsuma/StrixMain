using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ForNetwork
using SoftGear.Strix.Unity.Runtime;

public class DaifugoTask : MonoBehaviour
{
    private static readonly string[] RANK_PREFIX_LIST = new string[]
    {
    "",
    "1st",
    "2nd",
    "3rd",
    "4th",
    };

    public DaifugoCard cardBase = null; // カードの元ネタ
    public Transform centerRoot = null; // 中央に置くカード

    public GameObject passUI = null;
    public GameObject resultUI = null;
    public Text resultText = null;

    public static DaifugoTask instance = null;


    private bool isRoomOwner => StrixNetwork.instance.isRoomOwner;

    private DaifugoPlayer localPlayer => DaifugoPlayer.localPlayer;
    private int playerCount => DaifugoPlayer.playerList.Count; // 現在何人プレイしているか取得
    private int currenTurnIdentifier
    {
        get
        {
            return StrixNetwork.instance.room.GetState();
        }
        set
        {
            // 送信処理
            var rp = new Dictionary<string, object>(); //StrixNetwork.instance.room.GetProperties(); // ルーム内の情報のかたまり
            rp.Add("state", value);
            StrixNetwork.instance.SetRoom(StrixNetwork.instance.room.GetPrimaryKey(), rp, null, null);
        }
    }
   
    private int currentTurn => currenTurnIdentifier / 10;
    private int currentPlayerIndex => currenTurnIdentifier % 10;
    private DaifugoPlayer currentPlayer
    {
        get
        {
            int index = currentPlayerIndex;
            return (index < playerCount) ? DaifugoPlayer.playerList[index] : null;
        }
    }

    private bool isMyTurn => currentPlayer == localPlayer; // 自分のターンか

    // カード情報
    private List<DaifugoCard> _centerCardList = new List<DaifugoCard>(); // 中央にあるカード
    private List<int> _topcardList = new List<int>(); // 一番上にあるカードのID
    private List<int> _selectedCardIndexList =  new List<int>(); // 選択カードインデックス
    private List<int> _selectedCardList = new List<int>(); // 選択カード(番号、スート)

    // パス情報
    private int _continuousPassCount = 0; // 連続パス回数

    // コルーチン
    private Coroutine _turnCoroutine = null; // ターン用のコルーチン
    private bool _isTurnSuspended = false; // ターンが中断されたか


    void Awake()
    {
        if(StrixNetwork.instance.room == null) 
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
            return;
        }

        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // 元ネタ非表示
        cardBase.gameObject.SetActive(false);

        // ���C���̏�����J�n
        StartCoroutine(MainProc());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ���C���̏���
    IEnumerator MainProc()
    {
        // �ҋ@
        while (true)
        {
            if (isRoomOwner)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    break;
                }
            }
            else
            {
                break;
            }

            yield return null;
        }

        // ホストの場合はカードを配る
        if (isRoomOwner)
        {
            DealCard();
        }

        // 船員のカードが配られるまで待機
        yield return new WaitWhile(() =>
        {
            for (int i = 0; i < playerCount; i++)
            {
                if (DaifugoPlayer.playerList[i].cardCount == 0) //カード０　持ってない
                {
                    return true; // まだ配られてない
                }
            }
            return false; // 全員が配られた
        });

        // 待機
        yield return new WaitForSeconds(1.0f);

        // 最後の一人を残してカード出し切ったら終了
        for(int localTurn = 0; ;)
        {
            // ターン数、プレイヤー番号の同期待機
            yield return new WaitWhile(() =>{ return currentTurn < localTurn || currentPlayer == null; });

            // ターン
            _turnCoroutine = StartCoroutine(TurnProc());
            yield return new WaitWhile(() => {  return _turnCoroutine != null; });

            // ターン中にプレイヤーが抜けたことで、ターンが中断された場合は continue
            if(_isTurnSuspended)
            {
                _isTurnSuspended = false;
                continue; // 同期待機のところまで戻る
            }

            yield return new WaitForSeconds(1.0f);  

            // ぱすのUI をオフ
            passUI.SetActive(false);


            
            // プレー中のプレイヤー数を求める
            int leftPlayingPlayerCard = 0;
            DaifugoPlayer lastLeftPlayer = null;
            for (int i = 0; i < playerCount; i++)
            {
                if (DaifugoPlayer.playerList[i].cardCount > 0)
                {
                    leftPlayingPlayerCard++;
                    lastLeftPlayer = DaifugoPlayer.playerList[i];
                }
            }
            
            // 最後の一人になっている場合は終了
            if(leftPlayingPlayerCard <= 1)
            {
                // 最後の一人（大貧民）の順位を確定させる
                if (lastLeftPlayer != null)
                { 
                    lastLeftPlayer.SetRank(DaifugoPlayer.GetNextRank()); 
                }
                break;
            }

            // 連続パス薄によってトップカードをリセット
            if(_continuousPassCount >= leftPlayingPlayerCard -1)
            {
                int n = _centerCardList.Count;
                for (int i = 0; i < n; i++)
                {
                    GameObject.Destroy(_centerCardList[i].gameObject);
                }
                _centerCardList.Clear();
                _topcardList.Clear();
            }

            // ホスト側はターンを進める
            if(isRoomOwner)
            {
                int nextTurn = currentTurn + 1;

                int nextPlayerIndex = -1;
                for (int i = 1; i < playerCount; i++)
                {
                    int tmpIndex  = (currentPlayerIndex + i) % playerCount;
                    if (DaifugoPlayer.playerList[tmpIndex].cardCount > 0) // 手札が残っている？
                    {
                        nextPlayerIndex = tmpIndex;
                        break;
                    }
                }

                currenTurnIdentifier = nextTurn * 10 + nextPlayerIndex;
            }
            // ターン終了
            localTurn++;
            yield return null;
        }
        // リザルト画面をだす
        resultUI.SetActive(true);
        resultText.text = "GAME SET!!";
        for (int i = 1; i <= playerCount; i++)
        {
            var player = DaifugoPlayer.GetPlaeyrByRank(i);
            string name = (player == null) ? string.Empty : player.strixReplicator.roomMember.GetName();
            resultText.text = $"\n{RANK_PREFIX_LIST[i]} {name}";
        }

        yield break;
    }

    // ターン処理
    IEnumerator TurnProc()
    {
        // カード選択処理
        yield return SelectCardProc();

        // パスまたはカードを出すかの分岐
        if(_selectedCardIndexList.Count == 0)
        {
            /////////// パス処理 //////////////

            // 連続パス回数を加算
            _continuousPassCount++;

            passUI.SetActive(true);
        }
        else
        {
            ////////// カードを出す処理 //////////

            // 連続パス回数をリセット
            _continuousPassCount = 0;

            // カードを実際に出す処理
            ShotCard();
        }

        // カードがなくなったら勝利
        if (isMyTurn && currentPlayer.cardCount == 0)
        {
            currentPlayer.SetRank(DaifugoPlayer.GetNextRank()); //順位セット
        }

        // 終了
        _turnCoroutine = null;
        yield break;
    }

    // カード選択処理
    IEnumerator SelectCardProc()
    {
        // 全部のカードの選択を解除
        _selectedCardIndexList.Clear();
        ReflectSelectedCardList();

        if(isMyTurn)
        {
            // 自分のターン //
            if(localPlayer.cardCount == 0)
            {
                // カード持ってなければパス

            }
            else 
            {

                while (true)
                {
                    // クリック判定
                    if (Input.GetMouseButtonDown(0))
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        int hitCardIndex = -1;

                        if(Physics.Raycast(ray, out hit))
                        {
                            Debug.Log(hit.collider.gameObject.name);
                            hitCardIndex = localPlayer.IndexOfCard(hit.collider.gameObject);
                        }

                        Debug.Log(hitCardIndex);

                        if(hitCardIndex < 0)
                        {
                            // 全部カードを選択、解除
                            _selectedCardIndexList.Clear();
                        }
                        else
                        {
                            // カードの選択、解除
                            if(_selectedCardIndexList.Contains(hitCardIndex))
                            {
                                _selectedCardIndexList.Remove(hitCardIndex);

                            }
                            else
                            {
                                _selectedCardIndexList.Add(hitCardIndex);
                            }
                        }
                        ReflectSelectedCardList();
                    }
                    // 決定ボタン判定
                    if(Input.GetKeyDown(KeyCode.Return))
                    {
                        // カードを出せるか判定
                        if(CheckSelectedCardPlayerble())
                        {
                            break; //必ず出せる状態
                        }
                    }
                    // 「パス」ボタン決定
                    if(Input.GetKeyDown(KeyCode.P))
                    {
                        // 
                        _selectedCardIndexList.Clear();
                        ReflectSelectedCardList();

                        break;
                    }

                    yield return null;
                }

                // RPC
                int n = _selectedCardList.Count;
                byte[] selectedCardListByte = new byte[n];
                for(int i = 0; i < n; i++)
                {
                    selectedCardListByte[i] = (byte)_selectedCardList[i];
                }
                currentPlayer.ShotCard(selectedCardListByte);
             }
        }
        else
        {
            // 他人のターン //
            yield return new WaitWhile(()=> { return currentPlayer.decideInfoQueue.Count == 0; });

            var val = currentPlayer.decideInfoQueue.Dequeue();

            _selectedCardList.Clear();
            int n = val.infoVelues.Length;
            for(int i = 0; i < n; i++)
            {
                int card = val.infoVelues[i];
                if(card == 0)
                {
                    break;
                }
                _selectedCardList.Add(card);
            }

        }
        yield break;
    }

    // カードを出す
     void ShotCard()
    {
        List<DaifugoCard> shotCardList = new List<DaifugoCard>();

        int centerCardCount = _centerCardList.Count; // 今中央に積まれているカードの枚数

        // 一番お上のカード更新
        _topcardList.Clear();
        int n = _selectedCardList.Count;
        for(int i = 0; i < n;  i++)
        {
            int card = _selectedCardList[i];

            _topcardList.Add(card);

            var removeCard = currentPlayer.RemoveCard(card);
            if (removeCard != null)
             {
                removeCard.transform.SetParent(centerRoot, true);
                _centerCardList.Add(removeCard);
                shotCardList.Add(removeCard);
             }
        }

        _selectedCardIndexList.Clear();
        ReflectSelectedCardList();

        //
        float randomRotY = Random.Range(-10.0f, 10.0f);
        n =shotCardList.Count;
        for (int i = 0;i < n;i++)
        {
            float x = 0.5f * (i - (n - 1) * 0.5f) + Random.Range(-1.0f, 0.1f);
            float y = (1 + centerCardCount + i) * 0.01f;
            float z = Random.Range(-0.1f, 0.1f);
            shotCardList[i].SetDestPositionAndRotation(new Vector3(x, y, z), Quaternion.Euler(90.0f, randomRotY, 0.0f), false);
        }

        currentPlayer.SetCardPositionAndRotation();
    }

    void ReflectSelectedCardList()
    {
        // かー度の選択上体を反映
        _selectedCardList.Clear();

        foreach (var cardIndex in _selectedCardIndexList)
        {
            int card = localPlayer.cardList[cardIndex].cardIdentifier;
            _selectedCardList.Add(card);
        }

        _selectedCardList.Sort((a, b) => { return DaifugoDefine.CompareCardIdentifier(a, b); }) ; // 昇順

        currentPlayer.SetCardPositionAndRotation(false, _selectedCardIndexList);
    }

    // カード配る
    void DealCard()
    {
        // 山札を生成
        // Suit : 0 ~ 3 �i�X�y�[�h�A�n�[�g�A�N���u�A�_�C���j
        // Number : 1 ~ 13
        List<int> allCardList = new List<int>();
        for (int i = 1; i <= 13; i++)
        {
            for (int j = 0; j <= 3; j++)
            {
                allCardList.Add(10 * i + j);
            }
        }

        // �v���C���[���Ƃ̃J�[�h���X�g�𐶐�
        int playerCount = DaifugoPlayer.playerList.Count;
        List<int>[] eachCardsList = new List<int>[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            eachCardsList[i] = new List<int>();
        }

        // �J�[�h������_���ɔz�z
        while (allCardList.Count > 0)
        {
            for (int i = 0; i < playerCount && allCardList.Count > 0; i++)
            {
                int index = Random.Range(0, allCardList.Count);
                eachCardsList[i].Add(allCardList[index]);
                allCardList.RemoveAt(index);
            }
        }

        // 配ったカードを送信
        for (int i = 0; i < playerCount; i++)
        {
            var player = DaifugoPlayer.playerList[i];
            var cardList = eachCardsList[i];
            Debug.Log($"[Cards] {i} : {cardList.Count}");
            cardList.Sort((a, b) => { return a - b; });
            int n = cardList.Count;
            byte[] cards = new byte[n];
            for (int j = 0; j < n; j++)
            {
                cards[j] = (byte)cardList[j];
            }
            player.DealCardFromHost(cards); // RPC
        }
    }

    /// <summary>
    /// カードを作る
    /// </summary>
    /// <returns></returns>
    public DaifugoCard InstantiateCard()
    {
        cardBase.gameObject.SetActive(true);
        DaifugoCard newCard = GameObject.Instantiate<DaifugoCard>(cardBase, cardBase.transform.parent, true);
        cardBase.gameObject.SetActive(false);

        return newCard;
    }


    /// <summary>
    /// 選択中のカードを出せるか
    /// </summary>
    /// <returns></returns>
    bool CheckSelectedCardPlayerble()
    {
        // 選択中のカードと役の強さを判定
        int selectedCardStrength = 0;
        var selectedCardSetType = DaifugoDefine.CheckCardSetType(_selectedCardList, out  selectedCardStrength);

        // カードの役が不正な場合は出せない
        if(selectedCardSetType == DaifugoDefine.eCardSetType.None || selectedCardSetType == DaifugoDefine.eCardSetType.Illegal)
        {
            return false;
        }

        // カードが出されていない場合は出せる
        if(_topcardList.Count == 0)
        {
            return true;
        }

        // カードの枚数が異なる
        if(_selectedCardList.Count != _topcardList.Count)
        {
            return false;
        }

        // 出されているカードの役と強さを判定
        int topCardStrength;
        var topCardSetType = DaifugoDefine.CheckCardSetType(_selectedCardList, out topCardStrength);

        // カードの形式が異なる場合は出せない
        if(selectedCardSetType != topCardSetType)
        {
            return false;
        }

        // カードの強さがうわ待っていたら出せる
        if(selectedCardStrength > topCardStrength)
        {
            return true;
        }

        return false;

    }
}
