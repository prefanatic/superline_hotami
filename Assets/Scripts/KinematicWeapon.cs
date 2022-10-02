using UnityEngine;

class KinematicWeapon : AbstractWeapon
{
    public float spreadChance = 0f;

    public GameObject kinematic;
    public GameObject firePoint;
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
            Instantiate(kinematic, firePoint.transform.position, Quaternion.LookRotation(firePoint.transform.forward, Vector3.up));
            return true;
        }
        return false;
    }

}