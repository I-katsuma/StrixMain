using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class KartCourse : MonoBehaviour
{
    private int _courseIndex = -1; // 現在のコース番号

    public CourseSelector.CourseInfo courseInfo => CourseSelector.instance.courseInfoList[_courseIndex]; // 現在のコース情報
    public string courseName => courseInfo.name; // コース名
    public string prefabResourcePath => $"Kart/Course/{courseName}_"; // プレハブ
    public string albedoTextureResourcePath => $"Kart/Course/{courseName}_albe"; // 色テクスチャ

    public string attributeTextureresourcePath => $"Kart/Course/{courseName}_attr"; // 属性テクスチャ

    private GameObject _courceObject = null;
    private Texture2D _albedoTexture = null;

    /// <summary>
    /// コースを開放
    /// </summary>
    private void ReleseCource()
    {
        if(_courceObject)
        {
            Destroy( _courceObject );
            _courceObject = null;
        }
        // チェックポイント開放
        _checkPointList.Clear();
        _checkPointIntervalList.Clear();
        _checkPointTotalLengthlList.Clear();
        _totalLength = 0.0f;

        _courseIndex = -1;
    }

    /// <summary>
    /// コースをロードする
    /// </summary>
    /// <param name="index"></param>
    public void LoadCourse(int index)
    {
        Debug.Log("[LoadCourse] " + index);
        ReleseCource();
        _courseIndex = index;
    
        // オブジェクトとチェックポイント
        var coursePrefab = Resources.Load<GameObject>(prefabResourcePath);
        _courceObject = Instantiate<GameObject>(coursePrefab, prefabObjectRoot, false);
        checkPointRoot = _courceObject.transform.Find("CheckPointRoot");

        // テクスチャ
        _albedoTexture = Resources.Load<Texture2D>(albedoTextureResourcePath);
        attributeTexture= Resources.Load<Texture2D>(attributeTextureresourcePath);

        // メッシュのテクスチャ変更 (地面変わる)
        meshRenderer.material.mainTexture = _albedoTexture;

        GenerateCheckPoints();
    }
}
