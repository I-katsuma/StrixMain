using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float moveSpeed = 10.0f;

    public void Fire(Vector3 position, float angle)
    {
        transform.position = position;
        GetComponent<Rigidbody>().velocity = Quaternion.Euler(0.0f, angle, 0.0f) * Vector3.forward * moveSpeed;
        Destroy(gameObject, 4.0f);
    }
}
