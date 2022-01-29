using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    float OriginalSpeed;
    float OriginalDetectionRange;
    NavMeshAgent NMA;
    GameObject Player;
    float dist;
    public float DetectionRange;
    float alert;
    bool chasingPlayer;
    public float RoamCooldown = 10;
    float roamCooldown;
    public float RoamRadius = 10;
    public float Bravery = 50;

    public float RandomSoundMaxCooldown = 5;
    float SoundCoolDown;

    EnemyAttackSystem eas;
    EnemyHealth hp;
    Animator anim;
    SkinnedMeshRenderer MR;
    AudioManager Audio;
    ParticleSystem Particle;
    ParticleSystemRenderer PR;

   // public Image HpBar;
   // public Image StaminaBar;

    Transform TheEnemy;

    // Start is called before the first frame update
    void Awake()
    {
        TheEnemy = transform.GetChild(0);
        NMA = TheEnemy.GetComponent<NavMeshAgent>();
        eas = TheEnemy.GetComponent<EnemyAttackSystem>();
        hp = TheEnemy.GetComponent<EnemyHealth>();
        Audio = TheEnemy.GetComponent<AudioManager>();
        Particle = TheEnemy.GetComponent<ParticleSystem>();
        PR = TheEnemy.GetComponent<ParticleSystemRenderer>();
        anim = TheEnemy.GetChild(0).GetComponent<Animator>();
        MR = anim.transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>();

    }

    void Start()
    {
        ChangeColor();
        Player = GameManager.Player.transform.Find("ThePlayer").gameObject;
        OriginalSpeed = NMA.speed;
        OriginalDetectionRange = DetectionRange;
    }

    // Update is called once per frame
    void Update()
    {
        //  HpBar.fillAmount = 0;
        //   StaminaBar.fillAmount = 0;
        EnemyAI();
        PlayRandomSound();
    }

    protected void EnemyAI()
    {

        if (canSeePlayer())
        {
            alert += Time.deltaTime;
        }
        else if (alert > 0)
        {
            alert -= Time.deltaTime;
            if (alert > 1) chasingPlayer = true;
            else chasingPlayer = false;
        }

        if (chasingPlayer)
        {
            DetectionRange = OriginalDetectionRange * 1.5f;
            NMA.speed = OriginalSpeed * 1.5f;
            if (CheckBravery())
            {
                ChasePlayer();
            }
            else
            {
                RunningAway();
            }


        }
        else
        {
            DetectionRange = OriginalDetectionRange;
            NMA.speed = OriginalSpeed;
            Wonder();
        }

        if (eas.Stamina < eas.Tired) { NMA.speed = OriginalSpeed * 0.5f; }
    }

    void PlayRandomSound()
    {
        if (SoundCoolDown <= 0)
        {
            SoundCoolDown = Random.Range(3, 3+SoundCoolDown);
            Audio.PlaySound(Sound.Activation.Custom, "Boo");
        }
        else { SoundCoolDown -= Time.deltaTime; }
    }

    bool CheckBravery()
    {
        float maxBrave = hp.MaxHp + eas.MaxStamina;
        float currentBravery = hp.Hp + eas.Stamina;
        float BravePercent = currentBravery / maxBrave * 100;
        if (BravePercent >= 100 - Bravery)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void RunningAway()
    {
        NMA.SetDestination(TheEnemy.position + ((TheEnemy.position - Player.transform.position).normalized * 5));
    }

    void ChasePlayer()
    {
        NMA.SetDestination(Player.transform.position);
        if (dist <= NMA.stoppingDistance)
        {
            RotateTowards(Player.transform);
            eas.Attack();
        }
    }

    

    void Wonder()
    {
        if (roamCooldown > 0) { roamCooldown -= Time.deltaTime; }
        else
        {
            roamCooldown = Random.Range(0, RoamCooldown);
            float x = Random.Range(-RoamRadius, RoamRadius);
            float z = Random.Range(-RoamRadius + 0.1f, RoamRadius);
            Vector3 MoveTo = new Vector3(TheEnemy.position.x + x, TheEnemy.position.y, TheEnemy.position.z + z);
            NMA.SetDestination(MoveTo);
        }
    }

    bool eyesightcheck()
    {
        RaycastHit hit;
        if (Physics.Raycast(TheEnemy.position, (Player.transform.position - TheEnemy.position).normalized, out hit, DetectionRange))
        {
            if (hit.transform.gameObject == Player)
            {
                return true;
            }
        }
        return false;
    }

    bool canSeePlayer()
    {
        dist = Vector3.Distance(TheEnemy.position, Player.transform.position);
        if (dist <= DetectionRange && alert < 3)
        {
            if (eyesightcheck())
                return true;
            else return false;
        }
        else return false;
    }

    public void GotHit()
    {
        Particle.Emit(5);
        Audio.PlaySound(Sound.Activation.Custom, "Ah");
        anim.SetTrigger("Ouch");
        alert += 2;
    }

    private void RotateTowards(Transform target)
    {
        Vector3 targetlocation = new Vector3(target.transform.position.x, TheEnemy.position.y, target.transform.position.z);
        Vector3 direction = (targetlocation - TheEnemy.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        TheEnemy.rotation = Quaternion.Slerp(TheEnemy.rotation, lookRotation, Time.deltaTime * NMA.angularSpeed);
    }
    
    void ChangeColor()
    {
        int num = Random.Range(0, GameManager.EnemyColors.Capacity);
        MR.materials[1].SetColor("Color_6dd71f295d59438e88021f9aa009ed4b", GameManager.EnemyColors[num].GetColor("Color_6dd71f295d59438e88021f9aa009ed4b"));
        MR.materials[1].SetColor("Color_be99ade80ae949f583a1f004406e601b",GameManager.EnemyColors[num].GetColor("Color_be99ade80ae949f583a1f004406e601b"));
        PR.material = MR.materials[1];
    }

}