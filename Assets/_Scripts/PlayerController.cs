using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eState
{ lDodge, rDodge, lBlow, lJab, rBlow, rJab, uCut, Block, Duck, Idle, Damage, KO, Exhausted, Taunt, Stun, cutScene };

public class PlayerController : MonoBehaviour
{
    [Header("Inscribed")]
    

    public int maxHealth = 1;
    public int attackEnergy = 20;
    public float introLength = 2f;
    public float runUpLength = 1f;
    public int jabDamage = 2;
    public int blowDamage = 1;
    public int upperCutDamage = 3;
    public float jabWindup = 0.1f;
    public float blowWindup = 0.1f;
    public float uCutWindup = 0.1f;
    [Tooltip("How long the player's jabs last while opponent is stunned")]
    public float speedUpJabDur = 0.4f;
    [Tooltip("How long the player's blows last while opponent is stunned")]
    public float speedUpBlowDur = 0.4f;
    public AudioSource pAudio;
    public AudioClip upperSound;
    public AudioClip gettingHitSound;
    public HealthBars slider;



    public float jabDur = 0.6f;
    public float blowDur = 0.6f;
    public float uDur = 2f;
    public float DuckInputTime = 0.6f;
    public float duckDur = 1f;
    public float dodgeDur = 2f;
    public float dodgeCancelDur = 0.2f;
    public float upperCutMax = 5f;
    [Tooltip("How many stars should be subtracted when the player is hit")]
    public float upperCutChargeReduct = 5f;
    public int energyRefill = 15;
    public int energyLostHit = 3;
    [Tooltip("How long should the player be stunned for after getting hit")]
    public float stunTime = 1f;

    [Tooltip("How far down the player goes when knocked down")]
    public float knockDownDist = 2.5f;
    [Tooltip("How far up the player gets when pressing left or right")]
    public float getUpDist = 0.2f;

    [Tooltip("How far should the player get set back during the get up phase")]
    public float getUpDistRev = 0.1f;
    [Tooltip("How frequently the player should get lower during get up phase")]
    public float getUpRevRate = 0.5f;
    [Tooltip("How long the knockdown animation takes")]
    public float knockDownAnimTime = 0.45f;
    public int knockDownHealthRestore = 3;
    public int knockDownEnergyRestore = 20;
    public float getUpTime = 1f;

    [Header("Dynamic")]
    public float currAttack;
    public eState state = eState.Idle;
    public float attackDur;
    public float currentDuckTime = 0f;
    public float lastDuck;
    public float duckWindow;
    public float currentDodge = 0f;
    public float lastDodge = -1f;
    public float lastShortDodge = -1f;
    public float lastAttack;
    public float currentUpCutCharge = 0f;
    public int currentEnergy;
    public int currentHealth;
    public float stunTimer = -1f;
    public int knockdownCount = 0;
    public float nextRevGetUp;
    public bool getUpState = false;
    public Vector3 origin;
    private GameManager manager;
    public Animator anim;

    void Awake()
    {
        slider.setMaxHP(maxHealth);
        origin = transform.position;
        currentHealth = maxHealth;
        currentEnergy = attackEnergy;
        manager = FindObjectOfType<GameManager>();
        anim = GetComponentInChildren<Animator>();

        if (manager == null)
        { Debug.LogError("Add a GameManager"); }
        if (anim == null)
        { Debug.LogError("Add an animator"); }

    }

    void Update()
    {
        //clamp energy and upcut charge so that they are never negative
        currentUpCutCharge = Mathf.Clamp(currentUpCutCharge, 0, upperCutMax);
        currentEnergy = Mathf.Clamp(currentEnergy, 0, attackEnergy);
        currentUpCutCharge = Mathf.Clamp(currentUpCutCharge, 0, upperCutMax);

        if (currentHealth <= 0 && getUpState == false)
        {
            manager.gameRun = false;
            state = eState.KO;
            Invoke(nameof(getUp), knockDownAnimTime);
        }
        if (getUpState)
        {//If the player is down, it can get back to stage by hitting left or right
            anim.Play("Getting Up");
            if (manager.KOTime < 10)
            {
                bool getupCheck = true;
                if (Xnput.GetButtonDown(Xnput.eButton.a) || Xnput.GetButtonDown(Xnput.eButton.b))
                {

                    transform.position = new Vector3(0, transform.position.y + getUpDist, 0);
                    if (transform.position.y >= origin.y)
                    {
                        transform.position = origin;
                        anim.Play("RunUp");

                        manager.anim.Play("WalkOff");
                        manager.anim.speed = 1f;
                        getupCheck = false;
                        //reset the KOTime for the next one
                        getUpState = false;
                        manager.KORun = false;
                        manager.KOTime = 0;
                        currentHealth = knockDownHealthRestore;
                        currentEnergy = knockDownEnergyRestore;
                        manager.queGameStart(getUpTime);
                        return;
                    }
                }
                if (getupCheck)
                    getUp();
            }
            else
            {
                manager.anim.Play("KO");
                manager.oWin();
            }
        }

        if (manager.gameRun == true)
        {
            if (currentHealth > 0)
            {
                if (checkIdle())
                    state = eState.Idle;

                if (currentEnergy <= 0 && state != eState.lDodge && state != eState.rDodge && state != eState.Stun)
                    //Makes sure the player is exhausted when out of energy
                    state = eState.Exhausted;

                currentDodge = Time.time;
                currAttack = Time.time;
                currentDuckTime = Time.time;

                if (Xnput.GetButtonDown(Xnput.eButton.down) && Time.time < duckWindow)
                {//Handles ducking
                    Debug.Log("Ducked");
                    state = eState.Duck;
                    lastDuck = Time.time + duckDur;
                }

                if (Xnput.GetButtonUp(Xnput.eButton.down) && state == eState.Block)
                {
                    anim.Play("BlockExit");
                }

                if (state == eState.Idle || state == eState.Exhausted)
                {//The bulk of player controls
                    if (Xnput.GetButtonDown(Xnput.eButton.down))
                    {//Block
                        Debug.Log("Block");
                        duckWindow = Time.time + DuckInputTime;
                        state = eState.Block;
                        anim.Play("BlockIntro");
                    }
                    else if (Xnput.GetButtonDown(Xnput.eButton.left))
                    {//Left dodge
                        Debug.Log("lDodge");
                        state = eState.lDodge;
                        lastDodge = Time.time + dodgeDur;
                        lastShortDodge = Time.time + dodgeCancelDur;
                    }
                    else if (Xnput.GetButtonDown(Xnput.eButton.right))
                    {//Right dodge
                        Debug.Log("rDodge");
                        state = eState.rDodge;
                        lastDodge = Time.time + dodgeDur;
                        lastShortDodge = Time.time + dodgeCancelDur;
                    }
                    else if (Xnput.GetButtonDown(Xnput.eButton.b) && state != eState.Exhausted)
                    {//Left attacks
                        float lAttackWind;
                        if (Xnput.GetButton(Xnput.eButton.up))
                        {//left Jab
                            state = eState.lJab;
                            Debug.Log("Left Jab");
                            if (manager.Opponent.state == eState.Stun)
                                attackDur = speedUpJabDur;
                            else
                                attackDur = jabDur;
                            lastAttack = Time.time + attackDur;
                            lAttackWind = jabWindup;
                        }
                        else
                        {//left blow
                            state = eState.lBlow;
                            Debug.Log("Left Blow");
                            if (manager.Opponent.state == eState.Stun)
                                attackDur = speedUpBlowDur;
                            else
                                attackDur = blowDur;
                            lastAttack = Time.time + attackDur;
                            lAttackWind = blowWindup;
                        }
                        Invoke(nameof(GUp), lAttackWind);
                    }
                    else if (Xnput.GetButtonDown(Xnput.eButton.a) && state != eState.Exhausted)
                    {//Right attacks
                        float rAttackWindup;
                        if (Xnput.GetButton(Xnput.eButton.up))
                        {// right jab
                            state = eState.rJab;
                            Debug.Log("Right Jab");
                            if (manager.Opponent.state == eState.Stun)
                                attackDur = speedUpJabDur;
                            else
                                attackDur = jabDur;
                            lastAttack = Time.time + attackDur;
                            rAttackWindup = jabWindup;
                        }
                        else
                        {// right blow
                            state = eState.rBlow;
                            Debug.Log("Right Blow");
                            if (manager.Opponent.state == eState.Stun)
                                attackDur = speedUpBlowDur;
                            else
                                attackDur = blowDur;
                            lastAttack = Time.time + attackDur;
                            rAttackWindup = blowWindup;
                        }
                        Invoke(nameof(GUp), rAttackWindup);
                    }//UpperCut
                    else if (Xnput.GetButtonDown(Xnput.eButton.start) && currentUpCutCharge > 0 && state != eState.Exhausted)
                    {
                        state = eState.uCut;
                        Debug.Log("UpperCut");
                        anim.Play("StarPunch");
                        pAudio.clip = upperSound;
                        pAudio.Play(0);
                        attackDur = uDur;
                        lastAttack = Time.time + attackDur;
                        currentUpCutCharge -= 1;
                        Invoke(nameof(GUp), uCutWindup);
                    }
                }
                else if (state == eState.lDodge)
                {//handles the cancelling of left dodge
                    if (Xnput.GetButtonUp(Xnput.eButton.right))
                    {
                        if (checkShortDodge())
                        {
                            Debug.Log("dodgeCancel");
                            state = eState.Idle;
                        }
                        else
                            Debug.Log("cannot cancel dodge yet");
                    }
                }
                else if (state == eState.rDodge)
                {//handles the cancelling of right dodge
                    if (Xnput.GetButtonUp(Xnput.eButton.left))
                    {
                        if (checkShortDodge())
                        {
                            Debug.Log("dodgeCancel");
                            state = eState.Idle;
                        }
                        else
                            Debug.Log("cannot cancel dodge yet");
                    }
                }
            }
        }
        switch (state)
        {
            case eState.Idle:
                anim.Play("Idle");
                break;
            case eState.lDodge:
                anim.Play("DodgeLeft");
                break;
            case eState.rDodge:
                anim.Play("DodgeRight");
                break;
            case eState.Exhausted:
                anim.Play("Exhausted");
                break;
            case eState.Block:
                anim.Play("BlockHold");
                break;
            case eState.Duck:
                anim.Play("Duck");
                break;

            case eState.lJab:
                anim.Play("LeftJab");
                break;
            case eState.rJab:
                anim.Play("RightJab");
                break;
            case eState.lBlow:
                anim.Play("LeftBlow");
                break;
            case eState.rBlow:
                anim.Play("RightBlow");
                break;
        }
    }

    public bool checkIdle()
    {
        if (Xnput.GetButton(Xnput.eButton.down))
            //Return false if the player is blocking
            return false;
        //Checks if the player is in a dodge, attack, or if the player is stunned
        if ((currentDodge > lastDodge) && (lastDuck < currentDuckTime))
        {
            if (currAttack > lastAttack)
                if (Time.time > stunTimer)
                    return true;
        }
        return false;
    }
    public bool checkShortDodge()
    {//A premature check for dodge to allow dodges to be cancelled
        if (currentDodge > lastShortDodge)
        {
            //**Note** speed up the playback on the dodge
            lastShortDodge = currentDodge;
            lastDodge = currentDodge;
            return true;
        }
        return false;
    }

    public void blocked()
    {// A function to handle when the player is blocked or blocks
        currentEnergy -= 1;
        if (currentEnergy <= 0)
        {
            state = eState.Exhausted;
        }
    }

    public void dodged()
    {
        if (currentEnergy == 0)
            currentEnergy = energyRefill;
    }
    public void hit()
    {//Stuns the player and subtracts energy
        slider.updateHP(currentHealth);
        currentEnergy -= energyLostHit;
        currentUpCutCharge -= upperCutChargeReduct;
        state = eState.Stun;
        stunTimer = Time.time + stunTime;
        pAudio.clip = gettingHitSound;
        pAudio.Play(0);

    }

    public void GUp()
    {//Calls the game manager to update
        //a function to allow gameupdate to call after a specified time
        manager.gameUpdate();
    }

    public void getUp()
    {//A function to handle when the player gets knocked down
        if (getUpState == false && knockdownCount > 2)
        {
            knockdownCount++;
            manager.oWin();
        }
        if (getUpState == false && knockdownCount < 2)
        {
            manager.anim.Play("CountDown");
            getUpState = true;
            manager.gameRun = false;
            transform.position = new Vector3(0, -knockDownDist, 0);
            knockdownCount++;
            manager.koTimeCounter = (Mathf.Floor(Time.time * 1.5f));
            manager.KORun = true;
        }
        

        if (Time.time >= nextRevGetUp)
        {//Checks the if it is time for the player to get moved down
            nextRevGetUp = Time.time + getUpRevRate;

            //moves the player down while getting up
            transform.position = new Vector3(0, transform.position.y - getUpDistRev, 0);
        }
        else
            return;
    }
}
