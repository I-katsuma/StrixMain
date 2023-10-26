using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DaifugoCard : MonoBehaviour
{
    // UV
    const float UV_SIZE_X = 58 / 1024.0f;
    const float UV_SIZE_Y = 89 / 1024.0f;

    // Mesh
    public MeshFilter faceMeshFilter = null;
    public MeshFilter backMeshFilter = null;

    // 情報
    private int _cardIdentifier = 0;
    public int cardIdentifier => _cardIdentifier; // カード識別子
    public int number => DaifugoDefine.CardIdentifierToNumber(_cardIdentifier); // 番号
    public int suite => DaifugoDefine.CardIdentifierToSuit(_cardIdentifier); // スート

    // 位置制御
    private bool _isAutoMove = false; // これがtrueのときにカードの位置が以下の変数の位置になる
    private Vector3 _destPosition = Vector3.zero;
    private Quaternion _destRotation = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        // TEST
        // UpdateMesh(2, 5);
    }

    // Update is called once per frame
    void Update()
    {
        // カードの自動移動処理
        if(_isAutoMove)
        {
            float t = 8.0f * Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, _destPosition, t);
            transform.rotation = Quaternion.Lerp(transform.rotation, _destRotation, t);
        }

    }

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="cardIdentifier"></param>
    /// <param name="isAutoMove"></param>
    public void Initialize(int cardIdentifier, bool isAutoMove)
    {
        _cardIdentifier = cardIdentifier;
        _isAutoMove = isAutoMove;

        UpdateMesh(suite, number);
    }

    public void UpdateMesh(int suit, int number)
    {
        UpdateFaceMesh(suit, number); // オモテ面更新
        UpdateBackMesh(); // 裏面更新
    }

    void UpdateFaceMesh(int suit, int number)
    {
        var mesh = faceMeshFilter.mesh;

        Vector2[] uvList = new Vector2[4]; // カードの頂点の数
        uvList[0] = new Vector2(UV_SIZE_X * (number - 1),   1.0f - UV_SIZE_Y * (suit + 1));
        uvList[1] = new Vector2(UV_SIZE_X * number,           1.0f - UV_SIZE_Y * (suit + 1));
        uvList[2] = new Vector2(UV_SIZE_X * (number - 1),   1.0f - UV_SIZE_Y * (suit));
        uvList[3] = new Vector2(UV_SIZE_X * number,        1.0f - UV_SIZE_Y * (suit));

        mesh.uv = uvList;
    }

    void UpdateBackMesh()
    {
        var mesh = backMeshFilter.mesh;

        Vector2[] uvList = new Vector2[4]; // カードの頂点の数
        uvList[0] = new Vector2(0.0f, 0.0f);
        uvList[1] = new Vector2(UV_SIZE_X, 0.0f);
        uvList[2] = new Vector2(0.0f, UV_SIZE_Y);
        uvList[3] = new Vector2(UV_SIZE_X, UV_SIZE_Y);

        mesh.uv = uvList;
    }

    // 位置と回転を反映
    public void SetDestPositionAndRotation(Vector3 position, Quaternion rotation, bool isImmediate)
    {
        _destPosition = position;
        _destRotation = rotation;

        if(isImmediate)
        {
            UpdatePositonAndRptationImmediately();
        }
    }

    /// <summary>
    /// 位置と回転を即座に反映
    /// </summary>
    private void UpdatePositonAndRptationImmediately()
    {
        transform.position = _destPosition;
        transform.rotation = _destRotation;
    }
}
