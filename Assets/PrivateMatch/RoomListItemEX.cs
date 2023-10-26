using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For Network
using SoftGear.Strix.Unity.Runtime;

public class RoomListItemEX : MonoBehaviour
{
    public void OnClick()
    {
        StrixRoomListItem strixRoomListItem = GetComponent<StrixRoomListItem>();
        RoomPasswordInputUI roomPasswordInputUI = RoomPasswordInputUI.instance;

        var roomInfo = strixRoomListItem.roomInfo;

        if (roomInfo.isPasswordProtected) // パスワードが必要な部屋か？
        {
            // Private
            roomPasswordInputUI.Open(false, () =>
            {
                RoomJoinArgs args = new RoomJoinArgs()
                {
                    roomId = roomInfo.roomId,
                    host = roomInfo.host,
                    port = roomInfo.port,
                    protocol = roomInfo.protocol,
                    password = roomPasswordInputUI.passwordInputField.text,
                    memberProperties = new RoomMemberProperties { name = StrixNetwork.instance.playerName },
                };

                StrixNetwork.instance.JoinRoom(
                    args,
                    null,
                    null
                );
            });
        }
        else
        {
            // Public
            strixRoomListItem.OnClick();
        }
    }
}
