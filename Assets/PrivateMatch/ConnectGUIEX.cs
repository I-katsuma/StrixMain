using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For Network
using SoftGear.Strix.Unity.Runtime;

public class ConnectGUIEX : MonoBehaviour
{
    /// <summary>
    /// Createƒ{ƒ^ƒ“‚ª‰Ÿ‚³‚ê‚½‚Ìˆ—
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

        // “ü—Í‚µ‚½î•ñ
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
