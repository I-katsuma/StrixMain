using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartPlayer : MonoBehaviour
{
    const float MAX_SPEED = 40.0f;
    public Rigidbody myRigidBody = null;

    public Transform KartRoot = null;

    float _accel = 12.0f;
    float _speed = 0.0f;
    public float Speed => _speed;
    Vector3 _moveVelocity = Vector3.zero;

    float _rotationAccel = 360.0f;
    float _rotationSpeed = 0.0f;
    float _rotation = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // スペースキーが押されたら加速
        bool isAccel = Input.GetKey(KeyCode.Space);
        if (isAccel)
        {
            _speed = Mathf.Min(_speed + _accel * Time.fixedDeltaTime, MAX_SPEED);
        }

        // あたりぶ
        KartCource.eAttribute attr = KartCource.instance.GetAttribute(transform.position);
        bool isDart = (attr == KartCource.eAttribute.Dart);
        // 抵抗処理（減速）
        _speed *= isDart ? 0.98f : 0.99f;

        // 反映
        _moveVelocity = _speed * transform.forward;
        myRigidBody.velocity = _moveVelocity;
        myRigidBody.angularVelocity = Vector3.zero;

        // 左右の矢印キーでステアリング
        float handle = 0.0f;
        if (Input.GetKey(KeyCode.LeftArrow))
        { 
        handle -= 1.0f;
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            handle += 1.0f;
        }
        if(handle == 0.0f)
        {
            _rotationSpeed *= 0.9f;
        }

        _rotationSpeed += _rotationAccel * handle * Time.fixedDeltaTime;
        _rotationSpeed = Mathf.Clamp(_rotationSpeed, -120.0f, 120.0f);

        _rotation += _rotationSpeed * Time.fixedDeltaTime;
        myRigidBody.MoveRotation(Quaternion.Euler(0.0f, _rotation, 0.0f));

        // 見た目の更新
        float swing = isDart ? 0.03f : 0.01f;
        float speedClip = isDart ? 10.0f : 20.0f;
        float ratio = Mathf.Min(_speed, speedClip) / speedClip;
        // 上下にスウィング
        KartRoot.transform.localPosition = new Vector3(0.0f, ((_speed >= 1.0f) ? (Random.Range(-swing, swing) * ratio) : 0.0f), 0.0f);

        float angle = _rotationSpeed * 0.1f;
        KartRoot.transform.localEulerAngles = new Vector3(0.0f, angle, 0.0f);
    }
}
