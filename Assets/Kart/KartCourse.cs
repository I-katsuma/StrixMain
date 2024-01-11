using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class KartCourse : MonoBehaviour
{

    // コースの属性
    public enum eAttribute
    {
        None,

        Road, // 道
        Dart, // ダート
        Item, // アイテム

        Max
    };

    private const float SECTION_WIDTH = 24.0f; // 幅

    public MeshRenderer meshRenderer = null;

    public Transform prefabObjectRoot = null;

    public Texture2D attributeTexture = null;
    
    public static KartCourse instance = null;

    // チェックPoint
    public Transform checkPointRoot = null;
    private List<Transform> _checkPointList = new List<Transform>();
    private List<float> _checkPointIntervalList = new List<float>();
    private List<float> _checkPointTotalLengthlList = new List<float>();
    private float _totalLength = 0.0f;

    public int sectionCount => _checkPointList.Count;

    private void Awake()
    {
        instance = this;

        // GenerateCheckPoints();
    }

    // Start is called before the first frame update
    void Start()
    {
        // 最初に走るコースをロード
        LoadCourse(0);
    }

    // Update is called once per frame
    void Update()
    {
        // コース選択が選択されていればリロード
        int raceCource = (int)KartTask.instance.raceCourse;
        if(_courseIndex != raceCource)
        {
            LoadCourse(raceCource);
        }
    }

    public eAttribute GetAttribute(Vector3 position)
    {
        int pixelX = Mathf.Clamp((int)(512 + position.x * 10.0f), 0, attributeTexture.width);
        int pixelY = Mathf.Clamp((int)(512 + position.z * 10.0f), 0, attributeTexture.height);

        Color color = attributeTexture.GetPixel(pixelX, pixelY);

        if(color.r < 128/ 255.0f)
        {
            // ダートゾーン
            return eAttribute.Dart;
        }
        else if(color.g < 205 / 255.0f)
        {
            // みち
            return eAttribute.Road;
        }

        // アイテム
        return eAttribute.Item;
    }

    private void GenerateCheckPoints()
    {
        _totalLength = 0.0f;

        int n = checkPointRoot.childCount;
        for(int i = 0; i < n; i++)
        {
            Transform checkPoint = checkPointRoot.GetChild(i);
            _checkPointList.Add(checkPoint);
            _checkPointIntervalList.Add(0.0f);
            _checkPointTotalLengthlList.Add(0.0f);
        }

        for(int i = 0; i < n; ++i)
        {
            Transform checkPointPrew = _checkPointList[(i + n - 1) % n];
            Transform checkPoint = _checkPointList[i];
            Transform checkPointNext = _checkPointList[(i + 1) % n];

            // インターバル求める
            float intervalCurToNext = (checkPointNext.position - checkPoint.position).magnitude;

            _checkPointIntervalList[i] = intervalCurToNext; //現在のチェックポイントから次のチェックポイントまでの距離
            _checkPointTotalLengthlList[i] = _totalLength; // スタートから現在のチェックポイントまでの総距離

            _totalLength += intervalCurToNext;

            checkPoint.localScale = new Vector3(SECTION_WIDTH, 1.0f, 1.0f);

            // 進行方向ベクトル（前のポイントから次のポイントまでの方向）
            Vector3 normalPrevToCur = (checkPoint.position - checkPointPrew.position).normalized;
            // 現在から次のポイントまでの方向
            Vector3 normalCurToNext = (checkPointNext.position - checkPoint.position).normalized;

            // 角度を求める 値を入れるを角度が出るのがArcコサイン
            float radPrevToCur = Mathf.Acos(normalPrevToCur.z); //真上にしたときの角度
            if(normalPrevToCur.x > 0.0f)
            {
                // 角度を反転
                radPrevToCur = -radPrevToCur;
            }
            float radCurToNext = Mathf.Acos(normalCurToNext.z);
            if (normalPrevToCur.x > 0.0f)
            {
                // 
                radCurToNext = -radCurToNext;
            }
            while(radCurToNext - radPrevToCur > Mathf.PI)
            {
                radCurToNext-= Mathf.PI * 2.0f;
            }
            while (radCurToNext - radPrevToCur < -Mathf.PI)
            {
                radCurToNext += Mathf.PI * 2.0f;
            }

            // 平均 角度の傾き具合
            float radAverage = (radPrevToCur + radCurToNext) * 0.5f;

            // Rad -> Deg
            checkPoint.localEulerAngles = new Vector3(0.0f, -radAverage * Mathf.Rad2Deg, 0.0f);
        }
    }

    public bool GetPercentOnCource(Vector3 position,out int sectionIndex,  out float ratioInSection, out float rationOfAll)
    {
        // 高さを考慮しない
        position.y  = 0.0f;

        // 結果を初期化
        sectionIndex = -1;
        ratioInSection = 0.0f;
        rationOfAll = 0.0f;

        int n = _checkPointList.Count;
        for(int i = 0; i < n; i++) {
            Transform checkPointA = _checkPointList[i];
            Transform checkPointB = _checkPointList[(i + 1) % n];

            Vector3 vecAP = position - checkPointA.position;
            Vector3 vecBP = position - checkPointB.position;

            Vector3 normAB = (checkPointB.position - checkPointA.position).normalized;
            Vector3 rightAB = new Vector3(normAB.z, 0.0f, -normAB.x);

            float dA = Vector3.Dot(vecAP, checkPointA.forward);
            float dB = Vector3.Dot(vecBP, -checkPointB.forward);
            float dL = Vector3.Dot(vecAP, rightAB);

            if(dA >= 0.0f && dB >= 0.0f && Mathf.Abs(dL) <= SECTION_WIDTH * 0.5f)
            {
                // セクションの範囲内
                sectionIndex = i;
                ratioInSection = dA / (dA + dB);
                rationOfAll = (_checkPointTotalLengthlList[i] + ratioInSection * _checkPointIntervalList[i]) / _totalLength;               
            }

        }
        return (sectionIndex >= 0);
    }

    // スタート位置を設定
    public Vector3 GetStartingPoint(int index = -1)
    {
        Transform firstCheckPoint = _checkPointList[0];

        if(index < 0)
        {
            return firstCheckPoint.position - 1.2f * firstCheckPoint.forward;
        }
        else
        {
            //                                                                                                                  ↓ 1.0fから増やして隣のクルマとの距離を広げた
            return firstCheckPoint.position - 1.2f * firstCheckPoint.forward + 2.0f * (index -(KartPlayer.playerList.Count -1) * 0.5f) * firstCheckPoint.right;
        }
    }
}
