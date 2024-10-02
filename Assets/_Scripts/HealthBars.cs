using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBars : MonoBehaviour
{
    public Slider slider;
    public int maxHp;
    public void setMaxHP(int maxHP)
    {
        slider.maxValue = maxHP;
        maxHp = maxHP;
        slider.value = 100;
    }

    public void updateHP(int HP)
    {
        slider.value = Mathf.Floor(HP / maxHp);

    }
}
