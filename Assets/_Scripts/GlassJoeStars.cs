using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassJoeStars : MonoBehaviour
{
    public GlassJoe Joe;
    PlayerController Play;
    public Animator anim;
    public float starTime = 1f;
    public AudioSource starGet;

    private void Awake()
    {
        Joe = FindObjectOfType<GlassJoe>();
        anim = GetComponent<Animator>();
        Play = FindObjectOfType<PlayerController>();
    }

    public void AddStar()
    {
        anim.Play("StarGet");
        Play.currentUpCutCharge +=1;
        starGet.Play(0);
        Invoke(nameof(starGone), starTime);
        
    }
    public void starGone()
    {
        anim.Play("NoStarSprite");
    }
}
