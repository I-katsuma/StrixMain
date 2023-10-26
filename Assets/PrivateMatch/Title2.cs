using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// For Network
using SoftGear.Strix.Unity.Runtime;

public class Title2 : MonoBehaviour
{
    public StrixConnectGUI connectGUI = null;
    public StrixRoomListGUI roomListGUI = null;

    void Awake()
    {
        connectGUI.gameObject.SetActive(true);
        roomListGUI.gameObject.SetActive(false);
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

    private IEnumerator MainProc()
    {
        // 接続されるまで待機
        yield return new WaitWhile(() => { return connectGUI.gameObject.activeSelf; });

        // マスターサーバーへの接続が成功

        // ルーム一覧を表示
        roomListGUI.gameObject.SetActive(true);

        // ルームに接続できるまで待機
        yield return new WaitWhile(() => { return StrixNetwork.instance.room == null; });

        // ルームへの接続が成功

        // シーン遷移
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");

        yield break;
    }
}
