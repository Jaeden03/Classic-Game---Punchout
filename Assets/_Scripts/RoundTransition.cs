using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundTransition : MonoBehaviour
{
    public GameManager manager;
    public Animator anim;
    public bool nextRound = false;
    void Awake()
    {
        anim  = GetComponent<Animator>();
        manager = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        if (nextRound) 
        {
            if (manager.RoundTime == 0)
            {
                anim.Play("RoundTransition 0");
                nextRound = false;
            }
        }
        if (manager.RoundTime != 0)
        {
            nextRound = true;
        }
    }

}
