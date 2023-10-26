using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPasswordInputUI : MonoBehaviour
{
    public GameObject roomNameInputRoot = null;

    public InputField roomNameInputField = null;
    public InputField passwordInputField = null;

    public static RoomPasswordInputUI instance = null;

    System.Action _closeCallback = null;

    void Awake()
    {
        instance = this;

        Close();
    }

    public void Open(bool isShowRoomName, System.Action closeCallback)
    {
        _closeCallback = closeCallback;

        gameObject.SetActive(true);
        roomNameInputRoot.SetActive(isShowRoomName);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnClickOK()
    {
        Close();

        _closeCallback?.Invoke();
    }
}
