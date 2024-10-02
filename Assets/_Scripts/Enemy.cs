using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KOSide {Left, Right, None, lFail, rFail, lGetup, rGetup}
public enum currentBlock { none, low, high }
public class Enemy : MonoBehaviour
{

    [Header("Inscribed")]
    public int maxHealth = 5;
    public int damage = 1;
    public float introLength = 10f;
    public float runUpLength = 1f;
    public bool upperCut = false;
    public AudioClip blockSound;
    public AudioClip oppPunch;
    public AudioSource oppAudio;
    public HealthBars slider;


    [Header("Attack Timings")]
    public float decisionTime = 10f;
    public float blowWindup = 1f;
    public float jabWindup = 0.5f;
    public float upperWindup = 1.5f;
    public float blowDur = 1f;
    public float jabDur = 1.5f;
    public float uDur = 2f;
    public float blockDur = 0.5f;
    public float shortStunDur = 0.5f;
    public float stunDur = 2f;
    public float damageDur = 0.5f;
    public bool attack = false;
    public int jabDam = 2;
    public int blowDam = 1;
    public int upperDam = 3;

    public int healthRestore = 15;

    [Header("Knock down animation timings")]
    [Tooltip("How long the fake get up animations run for in seconds")]
    public float fakeGetUpTime = 2f;
    [Tooltip("How long the get up animations run for in seconds")]
    public float getUpTime = 3f;
    [Tooltip("How long the knock down animations run for in seconds")]
    public float KnockDownTime = 4f;
    [Tooltip("How long the walk in animation takes")]
    public float walkInTime = 1f;

    [Header("Knock down rolls")]
    [Tooltip("The chance the opponent will get up during KO")]
    public float getUpChance = 80f;
    [Tooltip("Reduces the chance the opponent will get up each time he's knocked down")]
    public float getUpReductionRate = 2f;
    [Tooltip("How frequently should the get up chance happen (seconds)")]
    public float getUpRollFrequency = 2f;
    [Tooltip("How likely is the opponent to do a fake getup")]
    public float fakeGetUpChance = 10f;



    

    [Header("Attack frequencies")]

    public float blockChance = 35f;
    public float leftJabChance = 15f;
    public float rightJabChance = 15f;
    public float leftBlowChance = 15f;
    public float rightBlowChance = 15f;
    public float upperCutChance = 5f;
    public float lowBlockChance = 50f;

    [Header("Dynamic")]
    public int currentHealth;
    public GameManager manager;
    public float lastDecision = 0f;
    public float blockTimer = -1f;
    public float stunTimer = -1f;
    public float currentAttDur;
    public int knockdownCount = 0;
    public Animator anim;
    public bool decide = true;
    public float nextGetUpRoll = 0;
    public float lastDamage = 0f;
    public eState state = eState.Idle;
    public currentBlock block = currentBlock.low;
    public int knockDownCount;
    public float currentKnockDownMult;
    public KOSide koSide = KOSide.None;
    //A timer for all knocked down animations 
    public float knockOutTimer;
    public bool oneShot = false;
    public int currentRoundKO = 0;
    public bool knockDownThisRound = true;
    protected virtual void Awake()
    {
        manager = FindObjectOfType<GameManager>();
        currentHealth = maxHealth;
        slider.setMaxHP(maxHealth);
        //Convert the chances into values to make the enemy randomly decide what to do
        leftJabChance = blockChance + leftJabChance;
        rightJabChance = leftJabChance + rightJabChance;
        leftBlowChance = rightJabChance + leftBlowChance;
        rightBlowChance = rightBlowChance + leftBlowChance;
        upperCutChance = rightBlowChance + upperCutChance;

        if (manager == null)
        { Debug.LogError("Add a GameManager"); }

        anim = GetComponentInChildren<Animator>();
        if (anim == null)
        { Debug.LogError("Add an animator"); }
    }

    protected virtual void Update()
    {
        slider.updateHP(currentHealth);
        if (knockDownThisRound && currentHealth <= 0)
        {
            state = eState.KO;
            manager.gameRun = false;
            knockDownThisRound = false;
            if (currentRoundKO < 3)
            {
                
                manager.koTimeCounter = (Mathf.Floor(Time.time * 1.5f));
                knockDownCount += 1;
                currentRoundKO += 1;
                

                knockOutTimer = KnockDownTime + Time.time;
                Invoke(nameof(countBuffer), KnockDownTime);
                Invoke(nameof(getUp), KnockDownTime + 4.5f);
            }
            else
            {
                manager.pWin();
                manager.gameRun = false;
            }
        }
        if (state == eState.KO)
        {
            if (koSide == KOSide.lFail && Time.time > knockOutTimer)
            {
                manager.KORun = true;
                manager.anim.speed = 1f;
                koSide = KOSide.Left;
            }
            else if (koSide == KOSide.rFail && Time.time > knockOutTimer)
            {
                manager.KORun = true;
                manager.anim.speed = 1f;
                koSide = KOSide.Right;
            }
            if (manager.KOTime < 10)
            {
                if (Time.time > nextGetUpRoll && Time.time > knockOutTimer)
                {
                    if (koSide == KOSide.Left || koSide == KOSide.Right)
                    {
                        nextGetUpRoll = Time.time + getUpRollFrequency;
                        getUp();
                    }
                }
            }
            else
            {
                manager.pWin();
            }
        }
        if (koSide == KOSide.lGetup || koSide == KOSide.rGetup)
        {
            manager.KORun = false;
            if (knockOutTimer < Time.time)
                anim.Play("Walk In");
        }
        if (manager.gameRun == true)
            // Should not do anything at certain times in the game
            if (currentHealth > 0)
            {//Reset state to idle
                if (state == eState.Block && blockTimer < Time.time)
                {
                    state = eState.Idle;
                }
                if (state == eState.Stun && stunTimer < Time.time)
                {
                    state = eState.Idle;
                }
                else if (state == eState.Damage && lastDamage < Time.time && state != eState.Stun && stunTimer > Time.time)
                {//If the opponent should show stun once damage is done
                    state = eState.Stun;
                }
                else if (state == eState.Damage && lastDamage < Time.time)
                {//If stun does not need to be shown after damage, opponent goes back to idle
                    state = eState.Idle;
                }

                else if (lastDecision < Time.time && decide == true && state == eState.Idle)
                {
                    decision();
                }

                switch (state)
                {//This will handle his stun and high/low block
                    case eState.Idle:
                        switch (block)
                        {
                            case currentBlock.high:
                                anim.Play("IdleGuardHigh");
                                break;
                            case currentBlock.low:
                                anim.Play("IdleGuardLow");
                                break;

                        }
                        break;
                }
                if (state == eState.Stun && lastDamage < Time.time)
                    //Plays stun animation while the enemy should be stunned
                    anim.Play("Stunned ");
            }
            else
            {//Plays knockdown idle while the opponent is on the ground
                if (state == eState.KO)
                {
                    if (knockOutTimer < Time.time)
                    {
                        if (koSide == KOSide.Left)
                        {
                            anim.Play("LeftKnockdownIdle");
                        }
                        else if (koSide == KOSide.Right)
                        {
                            anim.Play("RightKnockDown Idle");
                        }
                    }

                }
            }

    }

    public void decision()
    {
        float attackWindup = 0f;
        float chance = Random.Range(0f, 100f);
        if (chance < blockChance)
        {
            chance = Random.Range(0f, 100f);
            if (lowBlockChance < chance)
            {
                block = currentBlock.low;
            }
            else
                block = currentBlock.high;

            //Idle is a blocking state here. Block will be used as a state to handle when the opponent blocks
            state = eState.Idle;
        }
        else if (attack == true)
        {//Nested because the rest are attacks with windup
            float attackDetermine = Random.Range(0f, 100f);
            bool attackBuffer = true;
            if (leftJabChance < attackDetermine && leftJabChance != blockChance)
            {
                state = eState.lJab;
                attackWindup = jabWindup;
                block = currentBlock.low;
                currentAttDur = jabDur;
                anim.Play("Jab");
            }
            else if (rightJabChance > attackDetermine && rightJabChance != leftJabChance)
            {
                state = eState.rJab;
                attackWindup = jabWindup;
                block = currentBlock.low;
                currentAttDur = jabDur;
                anim.Play("RightHook");
            }
            else if (leftBlowChance > attackDetermine && leftBlowChance != rightJabChance)
            {
                state = eState.lBlow;
                attackWindup = blowWindup;
                block = currentBlock.high;
                currentAttDur = blowDur;
                anim.Play("LeftJab ");
            }
            else if (rightBlowChance > attackDetermine && rightBlowChance != leftBlowChance)
            {
                state = eState.rBlow;
                attackWindup = blowWindup;
                block = currentBlock.high;
                currentAttDur = blowDur;
                anim.Play("RightBlow");
            }
            else if (upperCutChance > attackDetermine && upperCutChance != rightBlowChance)
            {
                state = eState.uCut;
                attackBuffer = false;
                if (upperCut)
                {// if the opponent does not have a special uppercut
                    attackBuffer = true;
                    attackWindup = upperWindup;
                    block = currentBlock.none;
                    currentAttDur = uDur;

                    anim.Play("UpperCut ");
                }
            }
            else
            {
                state = eState.lJab;
                attackWindup = jabWindup;
                block = currentBlock.low;
                currentAttDur = jabDur;
                anim.Play("RightHook");
            }
            //Invoke a game update after the attack Windup
            if(attackBuffer)
                Invoke(nameof(GUp), attackWindup);
        }
        lastDecision = (Time.time + decisionTime + attackWindup + currentAttDur);
    }

    public void GUp()
    {//Calls an update in the game manager
        manager.oppGameUpdate();

        //Sets the block to none so the player can stun the opponent
        block = currentBlock.none;
        oppAudio.clip = oppPunch;
        oppAudio.Play(0);
        Invoke(nameof(attackReset), currentAttDur);
    }

    public void attackReset()
    {//A function to get the opponent back to idle after their attack
        Debug.Log("Attack reset");
        if (currentHealth > 0)
        {
            //Idle is a blocking state here. Block will be used as a state to handle when the opponent blocks
            if (state != eState.Stun && state != eState.Damage && state != eState.KO)
                state = eState.Idle;

            float chance;
            //Decides which block the opponent does after the attack
            chance = Random.Range(0f, 100f);
            if (lowBlockChance > chance)
            {
                block = currentBlock.low;
                anim.Play("IdleGuardHigh");
            }
            else
            {
                block = currentBlock.high;
                anim.Play("IdleGuardHigh");
            }
        }
    }



    public void blockTime()
    {
        oppAudio.clip = blockSound;
        oppAudio.Play(0);
        state = eState.Block;
        blockTimer = Time.time + blockDur;

        if (block == currentBlock.high)
        {
            anim.Play("FaceBlock");
        }
        else if (block == currentBlock.low)
        {
            anim.Play("GutBlock");
        }
        else
        {
            Debug.Log("Check why block is called on none");
        }
    }

    public void stunTime(bool shortLongStun)
    {//Handles player attacks that stun the opponent
        if (state != eState.KO)
        {
            Debug.Log("Stunned");
            state = eState.Stun;
            if (shortLongStun)
            {
                stunTimer = Time.time + stunDur;
            }
            else
            {
                stunTimer = Time.time + shortStunDur;
            }
        }
    }


    public void damageTime()
    {//Allows the enemy to stay in damage so that the hit animation plays
        state = eState.Damage;
        lastDamage = Time.time + damageDur;
        //because the opponent audio source is already going to be used, the player can get this one
    }

    public void countBuffer()
    {//a buffer so that the countdown matches with the ko timer
        manager.anim.Play("CountDown");
    }
    public void getUp()
    {
        if (koSide != KOSide.lGetup && koSide != KOSide.rGetup)
        {
            manager.KORun = true;
            nextGetUpRoll = Time.time + getUpRollFrequency;
            if (manager.KOTime < 10)
            {//Another check for safety
                print("GetUpRoll");
                print(manager.KOTime);
                print((fakeGetUpChance + getUpChance) / currentKnockDownMult);
                if (knockDownCount > 0)
                    currentKnockDownMult = knockDownCount * getUpReductionRate;
                else
                    currentKnockDownMult = 1;

                
                if (currentKnockDownMult == 0)
                {//Adjusts the opponent to have less of a chance to get up with more KO
                    currentKnockDownMult = 1;
                }
                float roll;
                roll = Random.Range(0f, 100f);
                nextGetUpRoll = Time.time + getUpRollFrequency + fakeGetUpTime;
                if (roll < fakeGetUpChance)
                {
                    knockOutTimer = Time.time + fakeGetUpTime;

                    Debug.Log("Failed to get up");
                    manager.KORun = false;
                    manager.anim.speed = 0f;
                    if (koSide == KOSide.Left)
                    {
                        anim.Play("LeftGetupFail");
                        koSide = KOSide.lFail;
                    }
                    else if (koSide == KOSide.Right)
                    {
                        anim.Play("RightGetUp Fail");
                        koSide = KOSide.lFail;
                    }
                }
                else if (roll < ((fakeGetUpChance + getUpChance) / currentKnockDownMult))
                {//Restore health, put state back to idle after the getup time
                 //Play getup animation
                    Debug.Log("Got Up");
                    currentHealth = healthRestore;
                    manager.KORun = false;
                    manager.anim.speed = 0f;
                    knockDownThisRound = true;

                    //Change to the length of the animation
                    manager.queGameStart(getUpTime + walkInTime);
                    state = eState.Idle;
                    knockOutTimer = Time.time + getUpTime;
                    if (koSide == KOSide.Left)
                    {
                        anim.Play("LeftGetUp ");

                        koSide = KOSide.lGetup;
                    }
                    else if (koSide == KOSide.Right)
                    {
                        anim.Play("RightGetUp");
                        koSide = KOSide.rGetup;
                    }
                    manager.anim.Play("WalkOff");
                    manager.anim.speed = 1f;
                }

            }
        }

    }
}
