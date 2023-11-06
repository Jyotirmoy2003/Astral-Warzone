
using UnityEngine;
using Photon.Pun;


[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviourPunCallbacks
{
    #region  Veriables
   [SerializeField] float walkSpeed=5f,sprintSpeed=10f,crouchSpeed=3f,JumpForce=0.5f,airMultiplier=0.4f,gravity=-9.8f;
   [SerializeField] Transform groundCheck;
   [SerializeField] float groundDistance=0.3f;
   [SerializeField] LayerMask ground;
   [SerializeField] Animator anim;
    [SerializeField] AudioSource footstepSlow, footstepFast;


   [SerializeField] KeyCode jumpKey=KeyCode.Space;
   [SerializeField] KeyCode crouchKey=KeyCode.LeftControl;
   [SerializeField] KeyCode sprintKey=KeyCode.LeftShift;
   [SerializeField] KeyCode lockCursoeKey=KeyCode.Escape;

  
    private Vector3 moveDire;
    private CharacterController controller;
    private bool IsGrounded=false;
    private bool IsCrouching=false;
    private bool IsSprinting=false;
    private float moveSpeed;
    private Vector3 velocity;
   private Transform myTransform;
   private MovementState state;

    private enum MovementState{
        sprint,
        walk,
        crouch,
        air,
        SpecialAction,

    }

    #endregion


    #region  MonoBehavior
    void Start()
    {
        myTransform=GetComponent<Transform>();
        controller=GetComponent<CharacterController>();

        if (!photonView.IsMine)
        {
            controller.enabled = false;
        }

       
        
    }

    
    void Update()
    {
        if (!photonView.IsMine) return;
        PlayerInput();
        Jump();
        Statehandeler();
    }

    void FixedUpdate()
    {
        Move();
    }
    #endregion

    void PlayerInput()
    {
        moveDire=Input.GetAxisRaw("Horizontal")*myTransform.right+Input.GetAxisRaw("Vertical")*myTransform.forward;
        //cursor lock and unlock
        if(Input.GetKeyDown(lockCursoeKey)) Cursor.lockState=CursorLockMode.None;
        else if(Cursor.lockState==CursorLockMode.None)
            if(Input.GetMouseButtonDown(0) && !UIController.instance.optionsScreen.activeInHierarchy) {Cursor.lockState=CursorLockMode.Locked;}
    }

    void Move()
    {
        //check if player is on ground
        IsGrounded=Physics.CheckSphere(groundCheck.position,groundDistance,ground);

        //If  on ground and velecoty of Y is less then "0" we simpley add velocity Y
        if(IsGrounded && velocity.y<0)
        {
            velocity.y=-1f;
        }  
        controller.Move(moveDire.normalized*moveSpeed*10f);
           
        
         controller.Move(velocity*Time.deltaTime);//Gravity

        if (moveDire.magnitude>0.2 &&IsGrounded)
        {
            if (IsSprinting && !footstepFast.isPlaying) {
                footstepFast.Play();
                footstepSlow.Stop();
            }
            else if (!IsSprinting && !footstepSlow.isPlaying)
            {
                footstepSlow.Play();
                footstepFast.Stop();
            }
        }
        if(moveDire.magnitude<0.2 || !IsGrounded)
        {
            footstepSlow.Stop();
            footstepFast.Stop();
        }

    }

    void Jump()
	{
		if(Input.GetKey(jumpKey) && IsGrounded)
		{
            velocity.y=Mathf.Sqrt(JumpForce*-2f*gravity);
            
		}
	}
    public void Jump(float force)
    {
        velocity.y = Mathf.Sqrt(force * -2f * gravity);
        footstepSlow.Stop();
        footstepFast.Stop();
    }

    void Statehandeler()
    {
        //Mode:-Sprint
        if(Input.GetKey(sprintKey))
        {
            state=MovementState.sprint;
            moveSpeed=sprintSpeed;
            IsSprinting=true;
        }
       
        //Mode:-Crouch
        if(Input.GetKeyDown(crouchKey))
        {
            state=MovementState.crouch;
            moveSpeed=crouchSpeed;
            IsCrouching=true;
        }
        //Mode:-walking 
        else if(IsGrounded && !IsSprinting)
        {
            state=MovementState.walk;
            moveSpeed=walkSpeed;
        }

        //Mode:- Air
        else{
            state=MovementState.air;
            velocity.y+=gravity*Time.deltaTime;
            moveSpeed*=airMultiplier;
        }
        

         //Mode:-resetSpeed
        if(Input.GetKeyUp(crouchKey))
        {
            IsCrouching=false;
        }
        if(Input.GetKeyUp(sprintKey))
        {
            IsSprinting=false;
        }

        anim.SetBool("grounded", IsGrounded);
        anim.SetFloat("speed", moveDire.magnitude);
    }

   
}
