using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractWeapon : MonoBehaviour
{
    public string weaponName;
    public float shootDelay;
    public string audioShootName;

    public ParticleSystem shootParticlesPrefab;
    public GameObject shootParticlesPoint;

    public bool isPlayer = false;
    public bool holdToFire = false;

    private float lastShootTime = 0f;
    private WeaponIndicator indicator;
    private Coroutine cooldownIndicatorRoutine;

    protected abstract bool _Shoot();

    public void Awake()
    {
        // Offset the shoot time so you can immediately shoot.
        lastShootTime = -shootDelay;

        indicator = GameObject.FindWithTag("WeaponIndicator").GetComponent<WeaponIndicator>();
    }

    public void Shoot()
    {
        if (isPlayer)
            print("Shoot!");
        if (lastShootTime + shootDelay > Time.time) return;

        var success = _Shoot();
        if (success)
        {
            lastShootTime = Time.time;
            var particles = Instantiate(shootParticlesPrefab, shootParticlesPoint.transform.position, transform.rotation);

            if (audioShootName != null)
            {
                AudioController.Instance.Play(audioShootName);
            }
            if (isPlayer)
            {
                GameManager.Instance.DeferTimeScale();
                if (cooldownIndicatorRoutine != null)
                {
                    StopCoroutine(cooldownIndicatorRoutine);
                }
                cooldownIndicatorRoutine = StartCoroutine(RunCooldownIndicator());
            }
        }
    }

    private IEnumerator RunCooldownIndicator()
    {
        var time = 0f;
        var start = 0f;
        while (time < 1f)
        {
            indicator.SetProgress(Mathf.Lerp(start, 1f, time));
            time += Time.deltaTime / shootDelay;
            yield return null;
        }
    }
}