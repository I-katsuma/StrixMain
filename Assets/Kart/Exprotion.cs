using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exprotion : MonoBehaviour
{
    private float _timer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        
        // 爆風波1秒で消える
        if(_timer >= 1.0f)
        {
            Destroy(gameObject);
            return;
        }

        // スケールをアニメーション(拡大して縮小　放物線の動き)
        transform.localScale = 6.0f * Mathf.Sin(Mathf.PI * _timer) * Vector3.one;

    }

    private void OnTriggerEnter(Collider other)
    {
        KartPlayer otherPlayer = KartPlayer.GetPlayerByGameObject(other.gameObject);

           if(otherPlayer == KartPlayer.localPlayer)
            {
                // クラッシュ
                otherPlayer.StartCrash();
            }
      }

}
