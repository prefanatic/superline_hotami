using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehavior : MonoBehaviour
{
    public float lifetime;
    public float radius;
    public Light light;

    Vector3 targetScale;

    void Start()
    {
        targetScale = new Vector3(radius, radius, radius);

        StartCoroutine(RunExplosion());
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player") || collider.gameObject.CompareTag("Enemy"))
        {
            var health = collider.gameObject.GetComponent<Health>();
            if (health)
            {
                health.RemoveHealth(1000);
            }
        }
    }

    void Update()
    {
        light.intensity = transform.localScale.x;
    }


    private IEnumerator RunExplosion()
    {
        var time = 0f;
        var startScale = transform.localScale;
        while (time < 1f)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, time);
            time += Time.deltaTime / lifetime;
            yield return null;
        }

        while (time > 0f)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, time);
            time -= Time.deltaTime / lifetime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
