using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For Network
using SoftGear.Strix.Unity.Runtime;

public class ConnectGUIEX : MonoBehaviour
{
    /// <summary>
    /// Create�{�^���������ꂽ���̏���
    /// </summary>
    public void OnCreateRoomButtonClick()
    {
        RoomPasswordInputUI roomPasswordInputUI = RoomPasswordInputUI.instance;

        roomPasswordInputUI.Open(true, () =>
        {
            CreateRoom();
        });
    }

    private void CreateRoom()
    {
        RoomPasswordInputUI roomPasswordInputUI = RoomPasswordInputUI.instance;

        // ���͂������
        var roomName = roomPasswordInputUI.roomNameInputField.text;
        var password = roomPasswordInputUI.passwordInputField.text;

        RoomProperties roomProperties = new RoomProperties
        {
            name = roomName,
            capacity = 4,
            password = string.IsNullOrEmpty(password) ? null : password,
        };

        RoomMemberProperties memberProperties = new RoomMemberProperties
        {
            name = StrixNetwork.instance.playerName,
        };

        StrixNetwork.instance.CreateRoom(
            roomProperties,
            memberProperties,
            null,
            null
        );
    }
}
