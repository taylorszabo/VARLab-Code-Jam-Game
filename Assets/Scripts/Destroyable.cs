using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyable : MonoBehaviour
{
    private Material material;

    [SerializeField] private float maxHealth = 100f;

    private float health;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;

        health = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        // Don't let the health value go below 0 or above maxHealth.
        health = Mathf.Clamp(health, 0f, maxHealth);

        // Set the material color to white 
        material.color = Color.Lerp(Color.red, Color.white, health/maxHealth);
    }
}
