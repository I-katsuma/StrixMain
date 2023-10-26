using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// For Network
using SoftGear.Strix.Unity.Runtime;

public class DaifugoPlayer : StrixBehaviour
{
    private const int MAX_SIT_INDEX = 4;
    private static readonly Vector3[] SIT_POSITION_LIST = new Vector3[MAX_SIT_INDEX]
    {
        new Vector3(0, 0, -5),
        new Vector3(-5, 0, 0),
        new Vector3(0, 0, 5),
        new Vector3(5, 0, 0),
    };
    private static readonly float[] SIT_ANGLE_LIST = new float[MAX_SIT_INDEX]
    {
        0, 90, 180, 270,
    };

    private const float CARD_OFFSET_Y = 0.6f; // カードの位置オフセット
    private const float CARD_OFFSET_Y_SELECTED = 0.9f; // 選択中のカードのオフセット
    // プレイヤー各テキスト
    public Text _nameText = null;
    public Text _debugText = null;

    // Player全体管理
    private static List<DaifugoPlayer> _playerList = new List<DaifugoPlayer>();
    public static List<DaifugoPlayer> playerList => _playerList;
    private static DaifugoPlayer _localPlayer = null;
    public static DaifugoPlayer localPlayer => _localPlayer;

    // 同期情報
    [StrixSyncField] private int _syncRank = 0; // 順位の同期
    public int syncRank => _syncRank;

    // 決定情報
    private Queue<DaifugoDecideInfo> _decideInfoQueue = new Queue<DaifugoDecideInfo>();
    public Queue<DaifugoDecideInfo> decideInfoQueue => _decideInfoQueue;

    // 非同期情報
    private int _sitIndex = -1;
    private List<DaifugoCard> _cardList = new List<DaifugoCard>();
    public List<DaifugoCard> cardList => _cardList;
    public int cardCount => _cardList.Count;

    // オブジェクトからインデックスを特定するpub
    public int indexOfCard(int cardIdentifier)
    {
        int n = _cardList.Count;
        for (int i = 0; i < n; i++)
        {
            if (_cardList[i].cardIdentifier == cardIdentifier)
            {
                return i;
            }
        }
        return -1;
    }



    public int IndexOfCard(GameObject gameObject)
    {
        int n = _cardList.Count;
        for (int i = 0; i < n; i++)
        {
            if (_cardList[i].gameObject == gameObject)
            {
                return i;
            }
        }
        return -1;
    }

        public DaifugoCard RemoveCard(int cardIdentifer)
        {
            DaifugoCard card = null;

            int index = indexOfCard(cardIdentifer);
        if (index >= 0)
            {
            card = _cardList[index];

            Transform cardTransform = card.transform;
            cardTransform.SetParent(null, true);

            _cardList.RemoveAt(index);
            }
        return card;
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

    private void OnDestroy() // 入室中プレイヤーリストから抜ける（ログアウトした）
    {
        // 登録解除
        if (_localPlayer = this)
        {
            _localPlayer = null;
        }
        _playerList.Remove(this);
        SortPlayerList();
    }

    // Start is called before the first frame update
    void Start()
    {
        // �o�^
        if (isLocal)
        {
            _localPlayer = this;
        }
        _playerList.Add(this);
        SortPlayerList();

        // UI
        _nameText.text = strixReplicator.roomMember.GetName();
    }

    // Update is called once per frame
    void Update()
    {
        if (_localPlayer != null)
        {
            // �v���C���[�̈ʒu�ݒ�
            int localSitIndex = _playerList.IndexOf(_localPlayer); // �������g
            int thisSitIndex = _playerList.IndexOf(this); // ���ꂼ��̐l
            int sitIndex = (thisSitIndex - localSitIndex + MAX_SIT_INDEX) % MAX_SIT_INDEX; // ���΍���

            if (_sitIndex != sitIndex)
            {
                _sitIndex = sitIndex;

                transform.localPosition = SIT_POSITION_LIST[sitIndex];
                transform.localEulerAngles = new Vector3(0.0f, SIT_ANGLE_LIST[sitIndex], 0.0f);
            }
        }
    }

    // �z�X�g����z��ꂽ�J�[�h��󂯎��
    public void DealCardFromHost(byte[] cards)
    {
        RpcToAll("DealCardFromHostRpc", cards);
    }

    [StrixRpc]
    void DealCardFromHostRpc(byte[] cards)
    {
        if (isLocal)
        {
            _debugText.text = string.Empty;
        }

        int n = cards.Length;
        for (int i = 0; i < n; i++)
        {
            int cardIdentifier = cards[i];

            if (isLocal)
            {
                // 一枚ずつのカードの処理
                _debugText.text += $"{cardIdentifier}, ";
            }

            // ここでカードを初期化
            var card = DaifugoTask.instance.InstantiateCard();
            card.Initialize(cardIdentifier, true);

            Transform cardTransform = card.transform;
            cardTransform.SetParent(transform, true);
            cardTransform.localPosition = new Vector3(0.0f, CARD_OFFSET_Y, 0.0f);
            cardTransform.localRotation = Quaternion.identity;

            _cardList.Add(card);

        }
        // カードを整列
        SetCardPositionAndRotation();

        if (isLocal)
        {
            Debug.Log("[DealCardFromHostRpc]" + cards.ToString());
        }
    }


    public void ShotCard(byte[] cardList)
    {
        RpcToOtherMembers("ShotCardRpc", cardList);
    }

    [StrixRpc]
    void ShotCardRpc(byte[] cardList)
    {
        Debug.Log("[ShotCardRpc] " + cardList.Length);

        var decideInfo = new DaifugoDecideInfo();

        int n = decideInfo.infoVelues.Length;
        int m = cardList.Length;

        for (int i = 0; i < n; i++)
        {
            decideInfo.infoVelues[i] = (i < m)? cardList[i] : 0;
        }

        _decideInfoQueue.Enqueue(decideInfo);
    }


    public void SetCardPositionAndRotation(bool isImmediate = false, List<int> selectedCardIndexList = null)
    {
        int n = _cardList.Count;
        if(n == 0)
        {
            // カードがない
            return;
        }

        float cardInterval = Mathf.Min(9.0f / n, 0.6f);
        for (int i = 0;i < n;i++)
        {
            bool isSelected = selectedCardIndexList != null && selectedCardIndexList.Contains(i); // i番目のカードが選択されているか
            float x = cardInterval * (i - (n -1) * 0.5f); // カードのX座標
            Vector3 position = transform.position + // Playerの位置
                transform.right * x - // 左側に寄せる
                transform.forward * i * 0.001f + // 手前に持ってくる
                Vector3.up * (isSelected ? CARD_OFFSET_Y_SELECTED : CARD_OFFSET_Y); // 上にあげる

            _cardList[i].SetDestPositionAndRotation(position, transform.rotation, isImmediate);
        }
    }

    /// <summary>
    /// 順位を設定
    /// </summary>
    /// <param name="rank"></param>
    public void SetRank(int rank)
    {
        _syncRank = rank;
    }

    /// <summary>
    /// 次の順位を求める
    /// </summary>
    /// <returns></returns>
    public static int GetNextRank()
    {
        // 今確定している順位の最大値をとってきて、それに１を加える
        int rank = 0;
        foreach(var player in _playerList)
        {
            rank = Mathf.Max(rank, player.syncRank); // 比較して大きい数字をrankに入れる
        }
        return rank + 1; 
    }

    public static DaifugoPlayer GetPlaeyrByRank(int rank)
    {
        foreach (var player in _playerList)
        {
            if(player.syncRank == rank)
            {
                return player;
            }
            
        }
        return null;
    }
}
