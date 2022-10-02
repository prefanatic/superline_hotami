using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // How quickly the enemy will react
    public float reactionSpeed = 0.2f;

    // Random factor for reaction speed.
    public float reactionFactor = 0.1f;

    // Error to apply when aiming
    public float aimErrorFactor = 0.3f;

    public float rotationSpeed = 0.2f;

    private GameObject player;
    private AbstractWeapon weapon;
    private bool tryingToShoot;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        weapon = GetComponentInChildren<AbstractWeapon>();
    }

    // Update is called once per frame
    void Update()
    {

        if (player == null) return;

        // Is the player in view?
        var direction = (player.transform.position - transform.position).normalized;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit))
        {
            if (hit.rigidbody.tag != "Player") return;
            if (tryingToShoot) return;

            StartCoroutine(TryShoot());
        }
    }

    private IEnumerator RotateToTarget(Transform target)
    {
        var time = 0f;
        var startRotation = transform.rotation;
        Vector3 direction;
        while (time < 1f)
        {
            direction = target.transform.position - transform.position;
            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, time);
            time += Time.deltaTime / rotationSpeed;
            yield return null;
        }

        direction = target.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up); ;
    }

    private IEnumerator TryShoot()
    {
        tryingToShoot = true;
        // Lock in the target location
        var target = player.transform.position;
        var difficultyFactor = GameManager.Instance.difficultyFactor;

        yield return new WaitForSeconds(reactionSpeed / difficultyFactor);
        var randomness = Random.value * reactionFactor;
        yield return new WaitForSeconds(randomness / difficultyFactor);

        var error = new Vector3(Random.Range(-aimErrorFactor, aimErrorFactor), Random.Range(-aimErrorFactor, aimErrorFactor), Random.Range(-aimErrorFactor, aimErrorFactor));
        error /= difficultyFactor;

        yield return RotateToTarget(player.transform);

        // Don't actually shoot in a tutorial.
        if (GameManager.Instance.tutorial)
        {
            tryingToShoot = false;
            yield break;
        }

        // Start shootin' cowboy
        transform.LookAt(player.transform.position + error, Vector3.up);
        weapon.transform.LookAt(player.transform.position + error, Vector3.up);
        weapon.Shoot();
        tryingToShoot = false;
    }
}
