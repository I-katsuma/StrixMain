using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SoftGear.Strix.Unity.Runtime;

public class Player : StrixBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotateSpeed = 1.0f;
    public Bullet bulletPrefab = null;

    Rigidbody _rigidbody = null;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        if (!isLocal)
        {
            _rigidbody.isKinematic = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocal)
        {
            _rigidbody.velocity = Input.GetAxis("Vertical") * moveSpeed * transform.forward;
            _rigidbody.angularVelocity = new Vector3(0.0f, Input.GetAxis("Horizontal") * rotateSpeed, 0.0f);

            if (Input.GetKeyDown(KeyCode.Z))
            {
                RpcToAll("FireBulletRpc", transform.position + transform.forward * 2.0f, transform.eulerAngles.y);
            }
        }
    }

    [StrixRpc]
    void FireBulletRpc(Vector3 position, float angle)
    {
        Instantiate(bulletPrefab).Fire(position, angle);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isLocal)
        {
            Destroy(gameObject);
        }
    }
}
