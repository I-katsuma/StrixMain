using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartCource : MonoBehaviour
{

    // コースの属性
    public enum eAttribute
    {
        None,
        Road, // 道
        Dart, // ダート

        Max
    };

    public Texture2D attributeTexture = null;
    
    public static KartCource instance = null;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public eAttribute GetAttribute(Vector3 position)
    {
        int pixelX = Mathf.Clamp((int)(512 + position.x * 10.0f), 0, attributeTexture.width);
        int pixelY = Mathf.Clamp((int )(512 + position.z *10.0f), 0, attributeTexture.height);

        Color color = attributeTexture.GetPixel(pixelX, pixelY);

        if(color.r < 0.5f)
        {
            // ダートゾーン
            return eAttribute.Dart;
        }

        // 道
        return eAttribute.Road;
    }
}
