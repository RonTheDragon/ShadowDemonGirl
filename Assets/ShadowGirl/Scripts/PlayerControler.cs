using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerControler : MonoBehaviour
{ 
    public float MovementSpeed = 10;
    float OriginalSpeed;
    public float LookSpeed = 10;
    public float CameraDist = 10;
    public LayerMask CLM;

    public float gravityMultiplier = 1f;
    public float JumpTime = 1f;
    public float jumpHeight = 2.4f;
    float jumpTimeLeft;
    float fallingTime;


    Transform PlayerBody;
    CharacterController CC;
    PlayerAttackSystem pas;
    PlayerHealth hp;
    Animator anim;
    Camera cam;
    Transform Tripod;
    Transform CameraHolder;

    float pitch;
    float yaw;

    public Image HpBar;
    public Image StaminaBar;
    public TextMeshProUGUI Kills;
    public TextMeshProUGUI Highscore;
    [HideInInspector]
    public int killCount;


    PlayerInputActions playerInputActions;

    
    public List<Material> EnemyColors;


    private void Awake()
    {
        OriginalSpeed = MovementSpeed;
        GameManager.Player = gameObject;
        GameManager.EnemyColors = EnemyColors;
        PlayerBody = transform.GetChild(0);
        CC = PlayerBody.transform.GetComponent<CharacterController>();
        anim = PlayerBody.GetChild(0).GetComponent<Animator>();
        pas = PlayerBody.GetComponent<PlayerAttackSystem>();
        hp = PlayerBody.GetComponent<PlayerHealth>();
        cam = Camera.main;
        Tripod = cam.transform.parent;
        CameraHolder = Tripod.parent;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateKillCount();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerInputActions.Player.Fire.performed += pas.Attack;
    }

    // Update is called once per frame
    void Update()
    {
        Walk();
        Look();
        Jump();
        ShowBars();
        MenuStuff();
    }

    void Walk()
    {
        Sprinting();
        Vector2 v2 = playerInputActions.Player.Move.ReadValue<Vector2>();
        Vector3 V3 = PlayerBody.forward * v2.y + PlayerBody.right * v2.x;
        CC.Move(V3.normalized * MovementSpeed * Time.deltaTime);
        if (V3 == Vector3.zero)
        {
            anim.SetInteger("Walk", 0);
        }
        else
        {
            anim.SetInteger("Walk", 1);
        }

    }
    void Look()
    {
        Vector2 v2 = playerInputActions.Player.Look.ReadValue<Vector2>();
        pitch -= v2.y * LookSpeed * 0.7f;
        pitch = Mathf.Clamp(pitch, -90, 90);
        yaw += v2.x * LookSpeed;
        PlayerBody.localRotation = Quaternion.Euler(0, yaw, 0);
        CameraHolder.localRotation = Quaternion.Euler(pitch, 0, 0);

        RaycastHit hit;
        if (Physics.Raycast(CameraHolder.transform.position, -CameraHolder.transform.forward, out hit, CameraDist, CLM))
        {
            CameraHolder.transform.GetChild(0).transform.position = hit.point;
        }
        else
        {
            CameraHolder.transform.GetChild(0).transform.position = CameraHolder.transform.position - CameraHolder.transform.forward * CameraDist;
        }

    }

    void ShowBars()
    {
        HpBar.fillAmount = hp.Hp / hp.MaxHp;
        StaminaBar.fillAmount = pas.Stamina / pas.MaxStamina;
    }

    void UpdateKillCount()
    {
        Kills.text = $"Ghosts Kills: {killCount}";
        if (killCount > PlayerPrefs.GetInt("Kills"))
            PlayerPrefs.SetInt("Kills", killCount);
        if (PlayerPrefs.GetInt("Kills") > 0) { Highscore.text = $"HighScore: {PlayerPrefs.GetInt("Kills")}"; }
       else { Highscore.text = string.Empty; }
    }

    public void KillAdded()
    {
        killCount++;
        UpdateKillCount();
    }

    public void Sprinting()
    {
        InputActionPhase phase = playerInputActions.Player.Sprint.phase;
        if (pas.Stamina > pas.Tired)
        {
            if (phase == InputActionPhase.Started)
            {
                MovementSpeed = OriginalSpeed * 1.5f;
                pas.Stamina -= (pas.StaminaRegan + pas.StaminaCost) * Time.deltaTime;
            }
            else
            {
                MovementSpeed = OriginalSpeed;

            }
        }
        else
        {
            MovementSpeed = OriginalSpeed * 0.5f;
            if (phase == InputActionPhase.Started)
            { pas.Stamina -= (pas.StaminaRegan + pas.StaminaCost) * Time.deltaTime; }
        }

    }

    void Jump()
    {
        
        float Direction = 0;
        
        if (jumpTimeLeft > 0)
        {
            jumpTimeLeft -= Time.deltaTime;
            Direction = jumpHeight * jumpTimeLeft;
        }
        else
        {
            fallingTime += Time.deltaTime;
            Direction = -gravityMultiplier * fallingTime;

            if (CC.isGrounded) fallingTime = 0;
        }
        
        
        CC.Move(PlayerBody.up * Direction * Time.deltaTime);

        if (CC.isGrounded && playerInputActions.Player.Jump.phase==InputActionPhase.Started)
        {
            jumpTimeLeft = JumpTime;
            fallingTime = 0;
        }

      

    }

    void MenuStuff()
    {
        if (Keyboard.current.escapeKey.IsPressed())
        {
            Application.Quit();
        }
        if (Keyboard.current.kKey.IsPressed())
        {
            PlayerPrefs.SetInt("Kills", 0);
            UpdateKillCount();
        }
    }
    
}

public static class GameManager
{
    public static GameObject Player;
    public static List<Material> EnemyColors;
}
