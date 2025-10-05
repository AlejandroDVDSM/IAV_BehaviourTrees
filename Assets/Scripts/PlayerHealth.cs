using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private float maxHealth = 100.0f;

    private Camera _camera;
    private float _currentHealth = 100.0f;

    
    private void Start()
    {
        _camera = Camera.main;
        _currentHealth = maxHealth;
        // InvokeRepeating(nameof(UpdateHealth), 5, 0.5f);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("bullet"))
            _currentHealth -= 10;
    }
    
    void Update()
    {
        Vector3 healthBarPos = _camera.WorldToScreenPoint(this.transform.position);
        healthBar.value = (int)_currentHealth;
        healthBar.transform.position = healthBarPos + new Vector3(0, 60, 0);
    }
    
    void UpdateHealth()
    {
        if (_currentHealth < 100)
            _currentHealth++;
    }


    public float GetCurrentHealth()
    {
        return _currentHealth;
    }
}
