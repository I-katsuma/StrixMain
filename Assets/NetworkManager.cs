using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Network
using SoftGear.Strix.Unity.Runtime;
using SoftGear.Strix.Client.Core.Model.Manager.Filter;
using SoftGear.Strix.Client.Core.Model.Manager.Filter.Builder;

public class NetworkManager : MonoBehaviour
{
    public string applicationId = "";
    public string host = "";
    public int port = 9122;

    [SerializeField] private Text _errorMessage = null;

    // �^�C�g����ʂ���ݒ肳�ꂽ���
    public static string playerName = string.Empty;
    public static string roomNo = string.Empty;
    public static bool isHost = false;

    public static NetworkManager instance = null;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        GameObject.DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    public void StartConnect()
    {
        StrixNetwork.instance.applicationId = applicationId;
        StrixNetwork.instance.playerName = playerName;
        StrixNetwork.instance.ConnectMasterServer(
           host,
           port,
           connectEventHandler: args =>
           {
               if (isHost)
               {
                   CreateRoom();
               }
               else
               {
                   EnterRoom();
               }
           },
           errorEventHandler: args =>
           {
               OnFailed();
           }
        );
    }

    // �Q�X�g��
    public void EnterRoom()
    {
        StrixNetwork.instance.SearchJoinableRoom(
            condition: ConditionBuilder.Builder().Field("name").EqualTo(roomNo).Build(),
            order: null,
            limit: 1,
            offset: 0,
            handler: args =>
            {
                // ����
                foreach (var roomInfo in args.roomInfoCollection)
                {
                    // ���[���ɓ���
                    StrixNetwork.instance.JoinRoom(
                        args: new RoomJoinArgs
                        {
                            host = roomInfo.host,
                            port = roomInfo.port,
                            protocol = roomInfo.protocol,
                            roomId = roomInfo.roomId,
                            password = null,
                            memberProperties = new RoomMemberProperties { name = StrixNetwork.instance.playerName }
                        },
                        handler: args =>
                        {
                            // ���[���ɓ��ꂽ
                            OnConnected();
                        },
                        failureHandler: args =>
                        {
                            OnFailed();
                        }
                    );
                    return;
                }

                // ���[�������݂��Ȃ����� -> �G���[
                Debug.LogError($"{roomNo} ���[�������݂��܂���I");
                OnFailed();
            },
            failureHandler: args =>
            {
                // ���s
                OnFailed();
            }
        );
    }

    // �z�X�g��
    private void CreateRoom()
    {
        StrixNetwork.instance.SearchJoinableRoom(
            condition: ConditionBuilder.Builder().Field("name").EqualTo(roomNo).Build(),
            order: null,
            limit: 1,
            offset: 0,
            handler: args =>
            {
                // ����
                foreach (var roomInfo in args.roomInfoCollection)
                {
                    // ���ɂ���
                    Debug.LogError($"���� {roomNo} ���[�������݂��܂��I");
                    OnFailed();
                    return;
                }

                // ���[�����܂����� -> ���[����쐬
                StrixNetwork.instance.CreateRoom(
                    new RoomProperties
                    {
                        capacity = 4,
                        name = roomNo
                    },
                    new RoomMemberProperties
                    {
                        name = StrixNetwork.instance.playerName
                    },
                    handler: args =>
                    {
                        OnConnected();
                    },
                    failureHandler: args =>
                    {
                        OnFailed();
                    }
                );
            },
            failureHandler: args =>
            {
                // ���s
                OnFailed();
            }
        );
    }

    private void OnConnected()
    {
        Debug.Log("OnConnected");

        UnityEngine.SceneManagement.SceneManager.LoadScene("Kart");
    }

    private void OnFailed()
    {
        Debug.Log("OnFailed");

        _errorMessage.gameObject.SetActive(true);
    }
}
