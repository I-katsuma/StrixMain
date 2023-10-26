using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // �V���O���g��
    public static Board instance = null;

    // �}�X�̊Ǘ��p���X�g
    private List<Masu> _masuList = new List<Masu>();
    public List<Masu> masuList => _masuList;

    void Awake()
    {
        instance = this;

        int n = transform.childCount;
        for (int i = 0; i < n; i++)
        {
            Transform child = transform.GetChild(i);
            Masu masu = child.GetComponent<Masu>();
            if (masu != null)
            {
                var prevMasu = (_masuList.Count == 0) ? null : _masuList[_masuList.Count - 1]; // ��O�̃}�X
                if (prevMasu)
                {
                    masu.prev = prevMasu;
                    prevMasu.next = masu;
                }

                // ���X�g�ɒǉ�
                _masuList.Add(masu);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
