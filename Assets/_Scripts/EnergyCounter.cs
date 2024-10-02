using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCounter : MonoBehaviour
{
    public TextMesh energyText;
    public PlayerController player;
    private void Awake()
    {
        energyText = GetComponent<TextMesh>();
        player = FindObjectOfType<PlayerController>();
    }
    void FixedUpdate()
    {
        energyText.text = player.currentEnergy.ToString("#,0");
    }
}
