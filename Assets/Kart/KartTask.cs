using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KartTask : MonoBehaviour // ゲームの流れを制御
{
    // UI
    public Text speedText = null;

    public KartPlayer localPlayer = null;

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
        // m / s => km / h
        speedText.text = $"{(int)(localPlayer.Speed * 3600.0f / 1000.0f)}";
    }
}
