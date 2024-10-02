using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
//using UnityEditor.PackageManager;
using UnityEngine;

public class GlassJoe : Enemy
{//A class to handle Glass Joe's specific AI
    [Header("Inscribed")]
    public float speedUpAttackTime = 5f;
    [Tooltip("How likely is it to get a star from stunning Joe")]
    public float starChance = 5f;
    public float tauntDur = 3f;
    public int tauntVulnFrames = 5;
    public AudioClip tauntSound;
 
    

    [Header("Dynamic")]
    public float attackProbabilities = 0f;
    public PlayerController Player;
    GlassJoeStars JStar;
    public float normalDecisionTime;
    public float joeJabProb;
    public float joeUCutProb;
    public float joeBlowProb;
    public bool beginningStar = true;
    public bool thisHit = true;
    public int hitCount = 0;
    public bool thisHitRound = true;
    public int currentTauntVulnFrames;
    public bool firstAttack = true;
    public bool tauntAttack = true;
    [Tooltip("If the player is out of energy before the taunt attack, Joe will only use rjab")]
    public bool rJabOnly = true;

    protected override void Awake()
    {
        normalDecisionTime = decisionTime;
        Player = FindObjectOfType<PlayerController>();
        JStar = FindObjectOfType<GlassJoeStars>();
        //does not need the rest of the attacks because Joe only does three attacks
        attackProbabilities = (rightJabChance + leftBlowChance + upperCutChance);
        joeJabProb = rightJabChance;
        joeBlowProb = leftBlowChance;
        base.Awake();
    }

    protected override void Update()
    {
        //base has its own check for manager.gameRun
        base.Update();
        if (manager.gameRun)
        {
            

            if (currentTauntVulnFrames > 0)
            {//Joe can get 1 hit ko in the first frames of his taunt punch
                oneShot = true;
                currentTauntVulnFrames--;
            }
            else
                oneShot = false;
            if ((manager.RoundTime >= 40 && firstAttack) || (tauntAttack && state == eState.uCut))
            {
               //taunt attack is a check to see if joe should do his taunt attack
               //based on if he went into a different state after his initial ucut
                tauntAttack = false;
                firstAttack = false;
                
                //taunt punch
                 //A multi staged, telegraphed punch
                 //This is the first move Joe uses
                    float attackWindup;
                    state = eState.cutScene;
                    anim.Play("Taunt ");
                    Invoke(nameof(TauntPunch), tauntDur);
                    attackWindup = upperWindup;
                    manager.timeRun = false;
                    lastDecision = (Time.time + decisionTime + attackWindup + currentAttDur);
                

            }
            if (state == eState.Idle || state == eState.Stun)
            //Checks to see if joe left his damage state
            {
                if (thisHitRound == true && state == eState.Stun)
                {//Stars are awarded for specific attacks for different opponents
                    thisHitRound = false;
                    float chance = Random.Range(0f, 100f);
                    if (chance < starChance)
                        JStar.AddStar();
                }
                thisHit = true;
                if (state == eState.Idle)
                {
                    tauntAttack = true;
                    thisHitRound = true;
                }

            }
            if (beginningStar)
            {//joe gives a star if you hit him 20 times in the first 40 sec
                if ((state == eState.Damage || state == eState.Stun) && thisHit == true)
                {
                    thisHit = false;
                    hitCount++;

                    if (hitCount >= 20)
                    {//Give a star
                        beginningStar = false;
                        JStar.AddStar();
                    }
                }
            }

            if (Player.currentEnergy <= 0)
            {
                //Glass Joe repeatedly attacks if the player is out of energy
                decisionTime = speedUpAttackTime;
                decide = false;
                if (lastDecision < Time.time && state == eState.Idle)
                {
                    JoeDecideAttack();
                }
            }
            else
            {
                decide = true;
                decisionTime = normalDecisionTime;
            }
        }
        
    }


    public void JoeDecideAttack()
    {// A more specific function to allow Joe to always attack at certain times
        if (Player.currentHealth > 0) {
            float attackWindup;
            if (state != eState.Idle)
            //should not do anything if not idle
            {
                lastDecision = (Time.time + tauntDur + 20);
                return;
            }
            if (manager.round == 1 && (manager.RoundTime > (40 - (jabDur * 3)) && manager.RoundTime < (40 + tauntDur * 3)))
            {//Just makes sure that joe isn't about to use his first taunt punch
                lastDecision = (Time.time + tauntDur + 15);
                return;
            }

            if (rJabOnly)
            {//Joe only right hooks if attacking in the first 40 seconds
                anim.Play("RightHook");
                state = eState.rJab;
                attackWindup = jabWindup;
                block = currentBlock.low;
                currentAttDur = jabDur;
                Invoke(nameof(GUp), attackWindup);
                return;
            }

            float attackDetermine = Random.Range(0f, attackProbabilities);
            if (joeJabProb < attackDetermine)
            {//right hook/right jab
                anim.Play("RightHook");
                state = eState.rJab;
                attackWindup = jabWindup;
                block = currentBlock.low;
                currentAttDur = jabDur;
            }
            else if (joeBlowProb < attackDetermine)
            {//left blow
                anim.Play("LeftJab ");
                state = eState.lBlow;
                attackWindup = blowWindup;
                block = currentBlock.high;
                currentAttDur = blowDur;
            }
            else
            {//taunt punch
             //A multi staged, telegraphed punch
                state = eState.cutScene;
                anim.Play("Taunt ");
                Invoke(nameof(TauntPunch), tauntDur);
                attackWindup = upperWindup;
                manager.timeRun = false;
            }
            lastDecision = (Time.time + decisionTime + attackWindup + currentAttDur);
            //Invoke a game update after the attack Windup
            if (state != eState.cutScene)
                //Only update if a taunt punch isn't occuring
                Invoke(nameof(GUp), attackWindup);
        } 
    }
    public void TauntPunch()
    {
        //set by the first taunt punch    
        attack = true;
        rJabOnly = false;
        
        manager.timeRun = true;
        currentTauntVulnFrames = tauntVulnFrames;
        anim.Play("RightHook");
        state = eState.uCut;
        block = currentBlock.none;
        currentAttDur = uDur;
        Invoke(nameof(GUp), upperWindup);
    }
}
    