using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerController Player;
    public Enemy Opponent;
    public int RoundTime;
    public int KOTime;
    public float marioLeaveTime = 1f;
    public AudioClip bgm;
    public AudioClip bellSound;
    public AudioClip oppHitFaceSound;
    public AudioClip introSound;
    public AudioClip getUpSound;
    public AudioClip oppHitGutSound;
    public AudioClip knockOutSound;
    public AudioClip winSound;
    public AudioClip opponentDown;
    public AudioClip loseSound;
    public AudioSource gameMusic;
    public AudioClip oppPunchSound;

    
    public eState pState;
    public eState enState;
    public bool gameRun = true;
    //A separate time variable so that we can manipulate the time
    public float timeCounter;
    public bool timeRun = false;
    public bool KORun = false;
    public int score = 0;
    public int round = 1;
    public float koTimeCounter = 0;
    [Tooltip("The animator for the ref")]
    public Animator anim;

    private void Awake()
    {
        Player = FindObjectOfType<PlayerController>();
        Opponent = FindObjectOfType<Enemy>();
        anim = GetComponentInChildren<Animator>();
        if (Player == null)
        {
            Debug.LogError("Add a player");
        }
        if (Opponent == null)
        {
            Debug.LogError("Add an enemy");
        }
        timeCounter = Mathf.Floor(Time.time * 3f);

        intro();
    }

    virtual protected void Update()
    {//The game's round times actually move about 3 times faster
        if (timeCounter != Mathf.Floor(Time.time * 3f))
        {
            timeCounter = Mathf.Floor(Time.time * 3f);

            if (gameRun && timeRun)
                RoundTime += 1;
            if (RoundTime >= 180)
            {
                RoundTime = 0;
                round++;
                intro();
                Opponent.currentRoundKO = 0;
            }
            if (round >= 4)
            {
                if (score >= 5000)
                {
                    pWin();
                }
                else
                {
                    oWin();
                }
            }
        }
        if (KORun)
        {//knockdown count is actually only about 1.5x faster

            if (koTimeCounter != Mathf.Floor(Time.time * 1.5f))
            {
                koTimeCounter = Mathf.Floor(Time.time * 1.5f);
                KOTime += 1;
            }
        }
        pState = Player.state;
        enState = Opponent.state;
        if (gameMusic.clip == opponentDown)
        {
            gameMusic.volume = 0.5f;
        }
        else
        {
            gameMusic.volume = 1f;
        }
    }

    public void intro()
    {
        gameMusic.clip = introSound;
        gameMusic.Play(0);
        gameRun = false;
        Opponent.state = eState.cutScene;
        Player.state = eState.cutScene;
        //(Assumes the opponents are longer than the players)
        float cutSceneTime = Opponent.introLength + Opponent.walkInTime;
        Opponent.anim.Play("Intro");
        //Placeholder animation for now
        Player.anim.Play("Getting Up");
        anim.Play("Intro");

        Invoke(nameof(oRunUp), (cutSceneTime - Opponent.walkInTime));
        Invoke(nameof(pRunUp), (cutSceneTime - Player.runUpLength));
        Invoke(nameof(gameStart), cutSceneTime);
    }
    public void pRunUp()
    {
        Player.anim.Play("RunUp");

    }

    public void oRunUp()
    {
        Opponent.anim.Play("Walk In");
    }

    public void pWin()
    {
        Player.state = eState.cutScene;
        Opponent.state = eState.cutScene;
        if (KOTime >= 10)
        {
            anim.Play("KO");
        }
        else if (Opponent.currentRoundKO >= 3)
        {
            anim.Play("TKO");
        }
        else if (score >= 5000 && round == 4)
        {
            anim.Play("Decision");
            anim.Play("LittleMacWins");
        }
        Player.pAudio.clip = bellSound;
        Player.pAudio.Play(0);
        gameMusic.clip = winSound;
        gameMusic.Play(0);
        Player.anim.Play("Victory");
    }

    public void oWin()
    {
        Player.state = eState.cutScene;
        Opponent.state = eState.cutScene;
        if (Player.knockdownCount >= 3)
        {
            anim.Play("TKO");
        }
        else if (KOTime >= 10)
        {
            anim.Play("KO");
        }
        else if (round == 4 && score < 5000)
        {
            anim.Play("TKO");
        }

        Opponent.oppAudio.clip = bellSound;
        Opponent.oppAudio.Play(0);
        gameMusic.loop = true;
        gameMusic.clip = loseSound;
        gameMusic.Play(0);
        
    }
 
    public void gameStart()
    {//A function to allow the game to be started after certain events
        if (gameMusic.clip != bgm)
        {
            gameMusic.clip = bgm;
            gameMusic.Play(0);
        }
        anim.Play("WalkOff");
        gameRun = true;
        timeRun = true;
        //Reset Player to Idle
        Player.state = eState.Idle;
        KOTime = 0;
        //Reset opponent
        Opponent.koSide = KOSide.None;
        //call attackReset to return to idle and choose a block
        Opponent.attackReset();
    }

    public void queGameStart(float getUpTime)
    {//A function other classes can call to allow animations to play before restarting the game
        Invoke(nameof(gameStart), getUpTime);
        KORun = false;
    }

    public void gameUpdate()
    {// a function to make the game process each action as they happen; Does not need to be in Update()
        print(Opponent.state);
        damage();
    }

    public void oppGameUpdate()
    {// a separate function to handle enemy controls so the player doesn't get hit during opponent windup if they do anything.
        print(Player.state);
        oppDamage();
    }

    public void oppDamage()
    {
        if (Player.state != eState.cutScene)
        {
            if (Player.state == eState.lDodge || Player.state == eState.rDodge || Player.state == eState.Duck)
            {//If the player is in any dodge, attacks can't hit them 
                Debug.Log("Dodged");
                Player.dodged();
                return;
            }
            else if (Player.state == eState.Block)
            {
                Player.blocked();
                return;
            }
            else
            {//If the player hasn't dodged or blocked, do damage.
                Player.hit();
                if (Opponent.state == eState.lJab || Opponent.state == eState.rJab)
                {
                    Player.currentHealth -= Opponent.jabDam;
                    if (Player.currentHealth > 0)
                    {
                        if (Opponent.state == eState.lJab)
                            Player.anim.Play("HitRight");
                        else
                            Player.anim.Play("HitLeft");
                    }
                    else
                    {
                        anim.Play("WalkIn");
                        Player.anim.Play("Knocked Down");
                        gameMusic.clip = getUpSound;
                        gameMusic.Play(0); 
                        Player.pAudio.clip = knockOutSound;
                        Player.pAudio.Play(0);
                    }
                }
                else if (Opponent.state == eState.rBlow || Opponent.state == eState.lBlow)
                {
                    Player.currentHealth -= Opponent.blowDam;

                    if (Player.currentHealth > 0)
                    {
                        if (Opponent.state == eState.rBlow)
                            Player.anim.Play("HitLeft");
                        else
                            Player.anim.Play("HitRight");
                    }
                    else
                    {
                        anim.Play("WalkIn");
                        Player.anim.Play("Knocked Down");
                        gameMusic.clip = getUpSound;
                        gameMusic.Play(0);
                    }
                }
                else if (Opponent.state == eState.uCut)
                {
                    Player.currentHealth -= Opponent.upperDam;
                    if (Player.currentHealth > 0)
                        Player.anim.Play("HitRight");
                    else
                    {
                        anim.Play("WalkIn");
                        Player.anim.Play("Knocked Down");
                    }
                }
            }
        }
    }

    public void damage()
    {//Handles damage when the player attacks
        if (Opponent.state != eState.cutScene && Opponent.oneShot == false)
        {
            if (Player.state == eState.uCut)
            {//Uppercuts can't be blocked
                score += 10;
                Opponent.currentHealth -= Player.upperCutDamage;
                if (Opponent.currentHealth <= 0)
                {
                    Opponent.anim.Play("LeftKnockDown ");
                    anim.Play("WalkIn");
                    Opponent.oppAudio.clip = knockOutSound;
                    Opponent.oppAudio.Play(0);
                    gameMusic.clip = opponentDown;
                    gameMusic.Play(0);
                    Opponent.koSide = KOSide.Left;
                    score += 1000;
                }
                else
                {
                    Opponent.anim.Play("HitFaceRight");
                    Opponent.oppAudio.clip = oppHitFaceSound;
                    Opponent.oppAudio.Play(0);
                }
                Opponent.damageTime();
            }
            else if (Opponent.block == currentBlock.none)
            {//Handles times when the opponent drops their guard after an attack
                if (Player.state == eState.rJab)
                {//right jab hit
                    Opponent.currentHealth -= Player.jabDamage;
                    score += 10;
                    Debug.Log("Hit");

                    if (Opponent.currentHealth <= 0)
                    {
                        Opponent.anim.Play("RightKnockDown");
                        anim.Play("WalkIn");
                        Opponent.oppAudio.clip = knockOutSound;
                        Opponent.oppAudio.Play(0);
                        gameMusic.clip = opponentDown;
                        gameMusic.Play(0);
                        Opponent.koSide = KOSide.Right;
                        score += 1000;
                    }
                    else
                    {
                        Opponent.anim.Play("HitFaceRight");
                        Opponent.oppAudio.clip = oppHitFaceSound;
                        Opponent.oppAudio.Play(0);
                    }
                    Opponent.stunTime(true);
                    Opponent.damageTime();
                }
                else if (Player.state == eState.lJab)
                {//left jab hit
                    Opponent.currentHealth -= Player.jabDamage;
                    score += 10;

                    Debug.Log("Hit");

                    if (Opponent.currentHealth <= 0)
                    {
                        Opponent.anim.Play("LeftKnockDown ");
                        anim.Play("WalkIn");
                        Opponent.oppAudio.clip = knockOutSound;
                        Opponent.oppAudio.Play(0);
                        gameMusic.clip = opponentDown;
                        gameMusic.Play(0);
                        Opponent.koSide = KOSide.Left;
                        score += 1000;
                    }
                    else
                    {
                        Opponent.anim.Play("HitFaceLeft");
                        Opponent.oppAudio.clip = oppHitFaceSound;
                        Opponent.oppAudio.Play(0);
                    }
                    Opponent.stunTime(true);
                    Opponent.damageTime();
                    
                }
                else
                {// blow hits
                    Opponent.currentHealth -= Player.blowDamage;
                    score += 10;
                    Debug.Log("Hit");
                    if (Opponent.currentHealth <= 0)
                    {
                        score += 1000;
                        if (Player.state == eState.lBlow)
                        {
                            Opponent.anim.Play("LeftKnockDown ");
                            anim.Play("WalkIn");
                            Opponent.oppAudio.clip = knockOutSound;
                            Opponent.oppAudio.Play(0);
                            gameMusic.clip = opponentDown;
                            gameMusic.Play(0);
                            Opponent.koSide = KOSide.Left;

                        }
                        else
                        {
                            Opponent.anim.Play("RightKnockDown");
                            anim.Play("WalkIn");
                            Opponent.oppAudio.clip = knockOutSound;
                            Opponent.oppAudio.Play(0);
                            gameMusic.clip = opponentDown;
                            gameMusic.Play(0);
                            Opponent.koSide = KOSide.Right;
                        }
                    }
                    else
                    {
                        Opponent.anim.Play("HitGut");

                        Opponent.damageTime();
                        Opponent.oppAudio.clip = oppHitGutSound;
                        Opponent.oppAudio.Play(0);
                    }

                    
                }
            }

            else if (Opponent.block == currentBlock.high)
            {//if the opponent is in a state where they block their face
                if (Player.state == eState.lJab || Player.state == eState.rJab)
                {//The attack is blocked
                    Debug.Log("Blocked");
                    Opponent.blockTime();
                    Opponent.anim.Play("FaceBlock");
                    Player.blocked();
                }
                else
                {//Blows can still hit
                    Opponent.currentHealth -= Player.blowDamage;
                    score += 10;
                    Debug.Log("Hit");
                    if (Opponent.currentHealth <= 0)
                    {
                        Opponent.oppAudio.clip = knockOutSound;
                        Opponent.oppAudio.Play(0);
                        gameMusic.clip = opponentDown;
                        gameMusic.Play(0);
                        score += 1000;
                        if (Player.state == eState.lBlow)
                        {
                            Opponent.anim.Play("LeftKnockDown ");
                            anim.Play("WalkIn");
                            
                            Opponent.koSide = KOSide.Left;

                        }
                        else
                        {
                            Opponent.anim.Play("RightKnockDown");
                            anim.Play("WalkIn");

                            Opponent.koSide = KOSide.Right;
                        }
                    }
                    else
                    {
                        Opponent.anim.Play("HitGut");
                        Opponent.oppAudio.clip = oppHitGutSound;
                        Opponent.oppAudio.Play(0);
                    }
                    if (Opponent.state != eState.Idle)
                        Opponent.stunTime(false);

                    Opponent.damageTime();

                }
            }
            else if (Opponent.block == currentBlock.low)
            {//if the opponent is in a state where they block their gut
                if (Player.state == eState.rJab)
                {
                    if (Opponent.state == eState.rJab)
                        //The attack misses because opponent's head is to their right
                        Debug.Log("Missed");
                    else
                    {
                        Opponent.currentHealth -= Player.jabDamage;
                        score += 10;
                        if (Opponent.currentHealth <= 0)
                        {
                            score += 1000;
                            Opponent.anim.Play("RightKnockDown");
                            anim.Play("WalkIn");
                            Opponent.oppAudio.clip = knockOutSound;
                            Opponent.oppAudio.Play(0);
                            gameMusic.clip = opponentDown;
                            gameMusic.Play(0);
                            Opponent.koSide = KOSide.Right;
                        }
                        else
                        {
                            Opponent.anim.Play("HitFaceRight");
                            Opponent.oppAudio.clip = oppHitFaceSound;
                            Opponent.oppAudio.Play(0);
                            if (Opponent.state != eState.Idle)
                            {//Stun if this attack interupts another attack
                                Opponent.stunTime(true);
                                Opponent.damageTime();
                            }
                            else
                            {
                                Opponent.damageTime();
                            }

                        }
                    }
                }

                else if (Player.state == eState.lBlow || Player.state == eState.rBlow)
                {//Blocks the player's attack
                    Debug.Log("Blocked");
                    Opponent.anim.Play("GutBlock");
                    Opponent.blockTime();
                    Player.blocked();
                }
                else
                {//left Jab can hit
                    Debug.Log("Hit jab");
                    Opponent.currentHealth -= Player.jabDamage;
                    score += 10;
                    if (Opponent.currentHealth <= 0)
                    {
                        score += 1000;
                        Opponent.anim.Play("LeftKnockDown ");
                        anim.Play("WalkIn");
                        Opponent.oppAudio.clip = knockOutSound;
                        Opponent.oppAudio.Play(0);
                        gameMusic.clip = opponentDown;
                        gameMusic.Play(0);
                        Opponent.koSide = KOSide.Left;
                    }
                    else
                    {
                        Opponent.anim.Play("HitFaceLeft");
                        Opponent.oppAudio.clip = oppHitFaceSound;
                        Opponent.oppAudio.Play(0);
                        if (Opponent.state != eState.Idle)
                        {//Stun if this attack interupts another attack
                            Opponent.stunTime(true);
                            Opponent.damageTime();
                        }
                        else
                        {
                            Opponent.damageTime();
                        }
                    }
                }

            }
        }
        else if (Opponent.oneShot)
        {//handles special cases of one shot ko
            if (Player.state == eState.lBlow || Player.state == eState.lJab)
            {
                Opponent.currentHealth = 0;
                score += 1010;
                Opponent.anim.Play("LeftKnockDown ");
                anim.Play("WalkIn");
                Opponent.oppAudio.clip = knockOutSound;
                Opponent.oppAudio.Play(0);
                gameMusic.clip = opponentDown;
                gameMusic.Play(0);
                Opponent.koSide = KOSide.Left;
            }
            else if (Player.state == eState.rBlow || Player.state == eState.rJab)
            {
                Opponent.currentHealth = 0;
                score += 1010;
                Opponent.anim.Play("RightKnockDown");
                anim.Play("WalkIn");
                Opponent.oppAudio.clip = knockOutSound;
                Opponent.oppAudio.Play(0);
                gameMusic.clip = opponentDown;
                gameMusic.Play(0);
                Opponent.koSide = KOSide.Right;
            }
        }
    }
}

