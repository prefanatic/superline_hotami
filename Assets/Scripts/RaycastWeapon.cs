using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class RaycastWeapon : AbstractWeapon
{
    public int damage = 1;
    public int bulletsPerShot = 1;
    public float spreadChance = 0f;
    public float travelTime = 0.3f;

    public GameObject firePoint;
    public TrailRenderer tracerPrefab;
    public LineRenderer laserSight;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward);
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(firePoint.transform.position, firePoint.transform.forward, out hit))
        {
            laserSight.enabled = true;
            laserSight.positionCount = 2;
            laserSight.SetPosition(0, firePoint.transform.position);
            laserSight.SetPosition(1, hit.point);
        }
        else
        {
            laserSight.enabled = false;
        }
    }

    protected override bool _Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(firePoint.transform.position, firePoint.transform.forward, out hit))
        {
            for (int i = 0; i < bulletsPerShot; i++)
            {
                var spread = new Vector3(Random.Range(-spreadChance, spreadChance), Random.Range(-spreadChance, spreadChance), Random.Range(-spreadChance, spreadChance));
                var trail = Instantiate(tracerPrefab, transform.position, Quaternion.identity);
                StartCoroutine(RunTracer(trail, hit, spread));
            }

            return true;
        }
        return false;
    }

    private IEnumerator RunTracer(TrailRenderer trail, RaycastHit hit, Vector3 spread)
    {
        var time = 0f;
        var startPosition = trail.transform.position;
        while (time < 1f)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hit.point + spread, time);
            time += Time.deltaTime / travelTime;
            yield return null;
        }
        trail.transform.position = hit.point + spread;

        // Do damages, if we still can
        if (!Physics.CheckSphere(hit.point + spread, 0.1f)) yield break;

        var hitObject = hit.transform.gameObject;
        var health = hitObject.GetComponent<Health>();
        if (health)
        {
            health.RemoveHealth(damage);
        }
    }
}