using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeText : MonoBehaviour
{
    public TextMesh timeText;
    public GameManager manager;

    private int min;
    private int sec;
    private int tenSec;
    private int timeCompare;

    private void Awake()
    {
        timeText = GetComponent<TextMesh>();
        manager = FindObjectOfType<GameManager>();
        timeCompare = manager.RoundTime;
    }
    void FixedUpdate()
    {
        if (manager.RoundTime == 0)
            resetTime();
        if (timeCompare != manager.RoundTime)
        {
            sec += 1;
            timeCompare = manager.RoundTime;
        }
        if (sec >= 10)
        {
            sec = 0;
            tenSec += 1;
        }
        if (tenSec >= 6) 
        {
            tenSec = 0;
            min += 1;
        }
        if (min >= 3)
        {
            resetTime();
        }

        timeText.text = (min.ToString("#,0") + ":" + tenSec.ToString("#,0") + sec.ToString("#,0"));
    }

    public void resetTime()
    {
        sec = 0;
        min = 0;
        tenSec = 0;
    }
}

