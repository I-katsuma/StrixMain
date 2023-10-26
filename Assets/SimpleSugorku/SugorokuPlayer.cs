using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// For Network
using SoftGear.Strix.Unity.Runtime;

public class SugorokuPlayer : StrixBehaviour
{
    // �R�}�̏��
    private enum eState
    {
        Idle,     // �ҋ@
        DiceRoll, // �_�C�X���[��
        Walk,     // �R�}��i�߂Ă���
        Goal,     // �R�}���S�[���܂ŗ���

        Max
    }

    // �R�}�̃I�t�Z�b�g�ʒu
    private static readonly Vector3[] KOMA_OFFSET_IN_MASU_LIST = new Vector3[]
    {
        new Vector3(-0.6f, 0.0f,  0.6f),
        new Vector3( 0.6f, 0.0f,  0.6f),
        new Vector3(-0.6f, 0.0f, -0.6f),
        new Vector3( 0.6f, 0.0f, -0.6f),
    };

    public Text _nameText = null;

    // �������
    [StrixSyncField] private int _syncTurn = 0; // �^�[����
    [StrixSyncField] private int _syncState = 0; // �R�}�̏��
    [StrixSyncField] private int _syncDiceNumber = 0; // �_�C�X�̖�
    [StrixSyncField] private int _syncMasuIndex = 0; // �ǂ��܂Ń}�X��i�񂾂�

    // �v���C���[�S�̊Ǘ�
    private static List<SugorokuPlayer> _playerList = new List<SugorokuPlayer>();
    private static SugorokuPlayer _localPlayer = null;

    // �񓯊����
    private int _masuIndex = 0;
    private Coroutine _localProcCoroutine = null;
    private Coroutine _walkCoroutine = null;

    // �O�̐l��擾
    private SugorokuPlayer prevPlayer
    {
        get
        {
            int myIndex = _playerList.IndexOf(this);
            int prevIndex = (myIndex == 0) ? (_playerList.Count - 1) : (myIndex - 1);
            return _playerList[prevIndex];
        }
    }

    // �S�[�������v���C���[��擾
    private SugorokuPlayer goalPlayer
    {
        get
        {
            foreach (var player in _playerList)
            {
                if (player._syncState == (int)eState.Goal)
                {
                    return player;
                }
            }
            return null;
        }
    }

    // �������ŏ��̃v���C���[��
    private bool isFirstPlayer
    {
        get
        {
            return _playerList.IndexOf(this) == 0;
        }
    }

    // �ق��̃v���C���[��������������Ă��邩
    private bool isSomePlayerProcessing
    {
        get
        {
            foreach (var player in _playerList)
            {
                if (player._syncState != (int)eState.Idle)
                {
                    return true;
                }
            }
            return false;
        }
    }

    // ���݂̋�̃I�t�Z�b�g
    private Vector3 komaOffsetInMasu
    {
        get
        {
            int myIndex = _playerList.IndexOf(this);
            if (0 <= myIndex && myIndex < KOMA_OFFSET_IN_MASU_LIST.Length)
            {
                return KOMA_OFFSET_IN_MASU_LIST[myIndex];
            }
            return Vector3.zero;
        }
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

    private void OnDestroy()
    {
        // �o�^���
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

        if (isLocal)
        {
            // ���[�J���̏�����J�n
            _localProcCoroutine = StartCoroutine(LocalProc());
        }
        else if (_syncMasuIndex > 0)
        {
            // �r�����o���ɑ���̋���łɐi��ł������̑Ή�
            _masuIndex = _syncMasuIndex;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_walkCoroutine == null)
        {
            // �R�}�̈ʒu���킹
            transform.position = Board.instance.masuList[_masuIndex].transform.position + komaOffsetInMasu;
        }

        if (isLocal)
        {
            // �����̃^�[��������킹�鏈��
            if (_syncTurn == 0)
            {
                _syncTurn = prevPlayer._syncTurn;
            }
            if (_syncTurn == 0 && isFirstPlayer)
            {
                _syncTurn = 1;
            }

            // �N�������������ꍇ�͏I��
            if (goalPlayer != null && _localProcCoroutine != null)
            {
                // �R���[�`�����~
                StopCoroutine(_localProcCoroutine);
                _localProcCoroutine = null;

                // ���������҂łȂ��ꍇ�͑ҋ@��Ԃ�
                if (_syncState != (int)eState.Goal)
                {
                    _syncState = (int)eState.Idle;
                }

                // UI
                bool mustBeTrue = goalPlayer == this;
                SugorokuUI.instance.SetWinnerName(goalPlayer.strixReplicator.roomMember.GetName());
            }
        }

        // DiceRoll
        if (_syncState == (int)eState.DiceRoll)
        {
            SugorokuUI.instance.SetDiceRollText(strixReplicator.roomMember.GetName(), _syncDiceNumber);
        }

        // Walk
        if (_masuIndex < _syncMasuIndex && _walkCoroutine == null)
        {
            _walkCoroutine = StartCoroutine(WalkProc());
        }

        // Camera
        if (_syncState != (int)eState.Idle)
        {
            Vector3 destPos = new Vector3(transform.localPosition.x, 6.54f, transform.localPosition.z - 10.0f);
            float ratio = Mathf.Min(5.0f * Time.deltaTime, 1.0f);
            Camera.main.transform.localPosition = Camera.main.transform.localPosition * (1.0f - ratio) + destPos * ratio;
        }
    }

    private IEnumerator WalkProc()
    {
        while (_masuIndex < _syncMasuIndex)
        {
            var curPos = Board.instance.masuList[_masuIndex].transform.position + komaOffsetInMasu;
            var NextPos = Board.instance.masuList[_masuIndex + 1].transform.position + komaOffsetInMasu;

            for (float ratio = 0.0f; ratio < 1.0f; ratio += 3.0f * Time.deltaTime)
            {
                transform.localPosition = curPos * (1.0f - ratio) + NextPos * ratio + 1.5f * Vector3.up * Mathf.Sin(ratio * Mathf.PI);

                yield return null;
            }

            _masuIndex++;
        }

        // �I��
        _walkCoroutine = null;
        yield break;
    }

    // ���[�J���̏���
    private IEnumerator LocalProc()
    {
        // �������S�[������܂ŌJ��Ԃ�
        while (_syncMasuIndex < Board.instance.masuList.Count - 1)
        {
            // �����̃^�[��������܂őҋ@
            yield return new WaitWhile(() => { return (isSomePlayerProcessing || _syncTurn == 0 || _syncTurn >= prevPlayer._syncTurn + (isFirstPlayer ? 1 : 0)); });

            // �_�C�X���[����J�n
            _syncState = (int)eState.DiceRoll;
            _syncDiceNumber = 0;

            // ����L�[���������܂őҋ@
            yield return new WaitWhile(() => { return !Input.GetKeyDown(KeyCode.Return); });

            // �_�C�X�̖ڂ�����_���Ō���
            _syncDiceNumber = 1 + Random.Range(0, 6);

            // �E�F�C�g
            yield return new WaitForSeconds(0.5f);

            // ��s
            _syncState = (int)eState.Walk;
            _syncMasuIndex = Mathf.Min(_syncMasuIndex + _syncDiceNumber, Board.instance.masuList.Count - 1);

            // ��s�����܂ŃE�F�C�g
            yield return new WaitWhile(() => { return _masuIndex < _syncMasuIndex; });

            // �E�F�C�g
            yield return new WaitForSeconds(0.5f);

            // �����̃^�[�����I��
            _syncTurn = prevPlayer._syncTurn + (isFirstPlayer ? 1 : 0);
            _syncState = (int)eState.Idle;
            _syncDiceNumber = 0;
        }

        // �S�[��
        _syncState = (int)eState.Goal;

        // �I��
        //_localProcCoroutine = null;
        yield break;
    }
}
