using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackSystem : AttackSystem
{
    Animator Anim;
    CharacterController CC;
    public LayerMask OnlyFloor;
    AudioManager Audio;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        CC = GetComponent<CharacterController>();
        Audio = GetComponent<AudioManager>();
        Anim = transform.GetChild(0).GetComponent<Animator>();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        
    }
    public void Attack(InputAction.CallbackContext context)
    {
        if (CanAttack())
        {
            Acooldown = AttackCooldown;
            Audio.PlaySound(Sound.Activation.Custom, "Attack");
            Anim.SetTrigger("Attack1");
        }
    }
}
