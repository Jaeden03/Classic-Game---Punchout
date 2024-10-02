using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarText : MonoBehaviour
{
    public TextMesh starText;
    public PlayerController player;
    private void Awake()
    {
        starText = GetComponent<TextMesh>();
        player = FindObjectOfType<PlayerController>();
    }
    void FixedUpdate()
    {
        starText.text = player.currentUpCutCharge.ToString("#,0");
    }
}
