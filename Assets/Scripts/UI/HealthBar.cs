using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _healthbarSprite;

    public void UpateHealthbar(float maxHealth, float currHealth)
    {
        _healthbarSprite.fillAmount = currHealth / maxHealth;
    }
}
