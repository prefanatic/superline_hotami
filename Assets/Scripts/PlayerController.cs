using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody rigidbody;
    public AbstractWeapon weapon;
    public GameObject weaponPoint;
    public float speed;

    [SerializeField] private Vector3 movement;
    private Camera camera;

    private float lockedY = 0f;

    void Start()
    {
        camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        lockedY = transform.position.y;
    }

    void Update()
    {
        // Look at the mouse
        var mousePos = Input.mousePosition;
        var zOffset = camera.transform.position.y - transform.position.y;
        var worldMousePos = camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, camera.transform.position.y));

        transform.LookAt(worldMousePos + (Vector3.up * transform.position.y));

        // Point the weapon this direction too
        var weaponDirection = (worldMousePos - weapon.transform.position).normalized;
        var weaponAngle = Vector2.SignedAngle(Vector2.up, weaponDirection);

        weapon.transform.LookAt(worldMousePos + (Vector3.up * weapon.transform.position.y));

        // Movement
        var horizontal = Input.GetAxis("Horizontal");
        var vertical = Input.GetAxis("Vertical");
        movement = new Vector3(horizontal, 0f, vertical) * speed * Time.fixedDeltaTime;

        // Shooting
        if (weapon.holdToFire)
        {
            if (Input.GetButton("Fire1"))
            {
                weapon.Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                weapon.Shoot();
            }
        }
    }

    void FixedUpdate()
    {
        rigidbody.MovePosition(rigidbody.position + movement);

        // Prevent building up velocity while running into walls.
        rigidbody.velocity = Vector3.zero;

        // Lock the y axis to prevent cheesing up the side of walls.
        var position = transform.position;
        position.y = lockedY;
        //transform.position = position;
    }
}
