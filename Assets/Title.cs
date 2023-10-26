using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [SerializeField] private InputField _PlayerNameInputField = null;
    [SerializeField] private InputField _roomNoInputField = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// ���[���쐬�{�^���������ꂽ�Ƃ�
    /// </summary>
    public void OnClickCreateRoom()
    {
        NetworkManager.playerName = _PlayerNameInputField.text;
        NetworkManager.roomNo = _roomNoInputField.text;
        NetworkManager.isHost = true;

        NetworkManager.instance.StartConnect();
    }

    /// <summary>
    /// ���[�������{�^���������ꂽ�Ƃ�
    /// </summary>
    public void OnClickJoinRoom()
    {
        NetworkManager.playerName = _PlayerNameInputField.text;
        NetworkManager.roomNo = _roomNoInputField.text;
        NetworkManager.isHost = false;

        NetworkManager.instance.StartConnect();
    }
}
