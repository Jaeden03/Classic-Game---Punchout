using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreText : MonoBehaviour
{ 
    public TextMesh scoreText;
    public GameManager manager;
    private void Awake()
    {
        scoreText = GetComponent<TextMesh>();
        manager = FindObjectOfType<GameManager>();
    }
    void FixedUpdate()
    {
        scoreText.text = manager.score.ToString("#,0");
    }
}
