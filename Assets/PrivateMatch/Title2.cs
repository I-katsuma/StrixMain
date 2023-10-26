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
        // �ڑ������܂őҋ@
        yield return new WaitWhile(() => { return connectGUI.gameObject.activeSelf; });

        // �}�X�^�[�T�[�o�[�ւ̐ڑ�������

        // ���[���ꗗ��\��
        roomListGUI.gameObject.SetActive(true);

        // ���[���ɐڑ��ł���܂őҋ@
        yield return new WaitWhile(() => { return StrixNetwork.instance.room == null; });

        // ���[���ւ̐ڑ�������

        // �V�[���J��
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");

        yield break;
    }
}
