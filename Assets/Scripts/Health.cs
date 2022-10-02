using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public Light deathLight;
    public ParticleSystem deathParticles;
    public Material particleMaterial;
    public int maxHealth;
    public string causeOfDeath;

    [SerializeField] private int health;
    [SerializeField] private bool dead = false;

    void Start()
    {
        health = maxHealth;
    }

    public void SetCause(string cause)
    {
        causeOfDeath = cause;
    }

    public void AddHealth(int value)
    {
        health += value;
    }

    public void RemoveHealth(int value)
    {
        if (dead) return;
        health = Mathf.Max(health - value, 0);

        // TODO: not the right place for this
        if (health == 0)
        {
            dead = true;
            var particles = (ParticleSystem)Instantiate(deathParticles, transform.position, Quaternion.identity);
            particles.transform.localScale = transform.localScale;
            particles.GetComponent<ParticleSystemRenderer>().material = particleMaterial;

            // For the player, give some death light thing
            if (gameObject.tag == "Player")
            {
                Instantiate(deathLight, transform.position + Vector3.up * 3.5f, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
