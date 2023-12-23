using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    // ビルボード　2D画面の正面を常にカメラに向ける処理


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        // ビルボードの処理
        transform.rotation = Camera.main.transform.rotation;
    }
}
