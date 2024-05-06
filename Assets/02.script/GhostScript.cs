using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;

public class GhostScript : MonoBehaviour
{
    private Animator Anim;
    private CharacterController Ctrl;
    private Vector3 MoveDirection = Vector3.zero;

    private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
    private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
    private static readonly int SurprisedState = Animator.StringToHash("Base Layer.surprised");
    private static readonly int AttackState = Animator.StringToHash("Base Layer.attack_shift");
    private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");
    private static readonly int AttackTag = Animator.StringToHash("Attack");

    [SerializeField]
    public Color[] colors;

    [SerializeField]
    private SkinnedMeshRenderer[] MeshR;
    [SerializeField]
    private float Dissolve_value = 1;
    private bool DissolveFlg = false;

    private int level = 1;
    public int Level {  get { return level; } }
    private int xp = 0;
    public int XP { get { return xp; } }        
    private int needXp = 10;
    public int NeedXp { get { return needXp; } }    
    private bool isMoved = true;
    [SerializeField]
    private float Speed = 2;
    [SerializeField]
    private float rotateSpeed = 360;
    [SerializeField]
    private FloatingJoystick joystick;
    [SerializeField]
    private Renderer renderer;
    private Vector3 moveVector = Vector3.zero;
    private float curDisolve = 0f;

    [SerializeField]
    private AudioClip audioClip;
    private AudioSource audioSource;
    void Start()
    {
        Anim = this.GetComponent<Animator>();
        Ctrl = this.GetComponent<CharacterController>();
        audioSource = this.GetComponent<AudioSource>();
    }
    void Update()
    {
        STATUS();
        GRAVITY();
        // this character status
        if (!PlayerStatus.ContainsValue(true))
        {
            MOVE();
            PlayerAttack();
        }
        else if (PlayerStatus.ContainsValue(true))
        {
            int status_name = 0;
            foreach (var i in PlayerStatus)
            {
                if (i.Value == true)
                {
                    status_name = i.Key;
                    break;
                }
            }
            if (status_name == Dissolve)
            {
                PlayerDissolve();
            }
            else if (status_name == Attack)
            {
                PlayerAttack();
            }
            else if (status_name == Surprised)
            {
                // nothing method
            }
        }
    }

    private const int Dissolve = 1;
    private const int Attack = 2;
    private const int Surprised = 3;
    private Dictionary<int, bool> PlayerStatus = new Dictionary<int, bool>
    {
        {Dissolve, false },
        {Attack, false },
        {Surprised, false },
    };

    private void STATUS()
    {
        if (!DissolveFlg)
        {
            PlayerStatus[Dissolve] = false;
        }
        if (Anim.GetCurrentAnimatorStateInfo(0).tagHash == AttackTag)
        {
            PlayerStatus[Attack] = true;
        }
        else if (Anim.GetCurrentAnimatorStateInfo(0).tagHash != AttackTag)
        {
            PlayerStatus[Attack] = false;
        }
        if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash == SurprisedState)
        {
            PlayerStatus[Surprised] = true;
        }
        else if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash != SurprisedState)
        {
            PlayerStatus[Surprised] = false;
        }
    }
    private void PlayerDissolve()
    {
        Dissolve_value -= Time.deltaTime;
        for (int i = 0; i < MeshR.Length; i++)
        {
            MeshR[i].material.SetFloat("_Dissolve", Dissolve_value);
        }
        if (Dissolve_value <= 0)
        {
            Ctrl.enabled = false;
        }
    }
    private void PlayerAttack()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Anim.CrossFade(AttackState, 0.1f, 0, 0);
        }
    }
    private void GRAVITY()
    {
        if (Ctrl.enabled)
        {
            if (CheckGrounded())
            {
                if (MoveDirection.y < -0.1f)
                {
                    MoveDirection.y = -0.1f;
                }
            }
            MoveDirection.y -= 0.1f;
            Ctrl.Move(MoveDirection * Time.deltaTime);
        }
    }
    private bool CheckGrounded()
    {
        if (Ctrl.isGrounded && Ctrl.enabled)
        {
            return true;
        }
        Ray ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
        float range = 0.2f;
        return Physics.Raycast(ray, range);
    }
    private void MOVE()
    {
        if (joystick.Horizontal != 0 || joystick.Vertical != 0)
        {
            moveVector = Vector3.zero;
            moveVector.x = joystick.Horizontal * Speed * Time.deltaTime;
            moveVector.z = joystick.Vertical * Speed * Time.deltaTime;

            Vector3 dir = Vector3.RotateTowards(transform.forward, moveVector, rotateSpeed * Time.deltaTime, 0.0f);

            MOVE_Velocity(new Vector3(joystick.Horizontal * Speed, 0, joystick.Vertical * Speed), dir);

            if (!isMoved)
            {
                Anim.CrossFade(MoveState, 0.1f, 0, 0);
                isMoved = true;
            }
        }
        else
        {
            if (isMoved)
            {
                Anim.CrossFade(IdleState, 0.1f, 0, 0);
                isMoved = false;
            }
        }
    }

    private void MOVE_Velocity(Vector3 velocity, Vector3 rot)
    {
        MoveDirection = new Vector3(velocity.x, MoveDirection.y, velocity.z);
        if (Ctrl.enabled)
        {
            Ctrl.Move(MoveDirection * Time.deltaTime);
        }
        MoveDirection.x = 0;
        MoveDirection.z = 0;
        this.transform.rotation = Quaternion.LookRotation(rot);
    }

    public Vector3 GetCameraPos()
    {
        Vector3 cameraPos = new Vector3(transform.position.x, transform.localScale.x * 3f + 2f, transform.position.z - 1.5f);
        return cameraPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("TriggerEnter ");
        Food food = other.GetComponent<Food>();
        if (food == null)
        {
            return;
        }
        Debug.Log("Trigger");

        if (food.CheckEating(level))
        {
            audioSource.PlayOneShot(audioClip);
            xp += food.XP;
            curDisolve += 0.01f;
            renderer.material.SetFloat("_Dissolve", curDisolve);
            CheckLevelXp();
        }
    }

    private void CheckLevelXp()
    {
        if (xp >= needXp)
        {
            xp -= needXp;
            level += 1;
            needXp *= 2;
            transform.localScale = new Vector3(transform.localScale.x + 5, transform.localScale.y + 5, transform.localScale.z + 5);
            Speed += 4;
            Debug.Log(renderer.material.GetColor("_MainColor"));
            if (level > 5)
            {
                renderer.material.SetColor("_MainColor", colors[4]);
            }
            else
            {
                renderer.material.SetColor("_MainColor", colors[level -1]);
            }
        }
        GameManager.Instance.onUpXp.Invoke();
    }
}