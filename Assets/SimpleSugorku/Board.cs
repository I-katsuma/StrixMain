using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    // シングルトン
    public static Board instance = null;

    // マスの管理用リスト
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
                var prevMasu = (_masuList.Count == 0) ? null : _masuList[_masuList.Count - 1]; // 一個前のマス
                if (prevMasu)
                {
                    masu.prev = prevMasu;
                    prevMasu.next = masu;
                }

                // リストに追加
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
