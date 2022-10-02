using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketKinematic : MonoBehaviour
{
    public float speed;
    public float acceleration;
    public GameObject explosionPrefab;
    public Rigidbody rigidbody;
    private Vector3 movement;

    void Update()
    {
        speed += acceleration * Time.deltaTime;
        movement = transform.forward * speed * Time.deltaTime;
    }

    void FixedUpdate()
    {
        rigidbody.MovePosition(rigidbody.position + movement);
    }

    void OnTriggerEnter(Collider collider)
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
