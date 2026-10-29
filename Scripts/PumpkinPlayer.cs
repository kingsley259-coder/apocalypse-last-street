using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
public class PumpkinPlayer : MonoBehaviour
{
    public float health=100,stamina=100,hurtUntil;
    public Transform visual,muzzle;
    public Material emberMaterial;
    public GunFeedback gun;
    public HumanoidMotion motion;
    public AudioClip jumpSound;
    CharacterController controller;
    PlayerArsenal arsenal;
    StreetBounds bounds;
    float fireAt,vertical;
    const float Gravity=20;
    public bool IsSprinting
    {
        get;
        private set;
    }
    public bool IsShooting
    {
        get;
        private set;
    }
    public bool IsGrounded
    {
        get;
        private set;
    }
    public float MoveSpeed
    {
        get;
        private set;
    }
    Vector3 aim=Vector3.forward;
    void Awake()
    {
        controller=GetComponent<CharacterController>();
        arsenal=GetComponent<PlayerArsenal>();
        bounds=GetComponent<StreetBounds>();
    }
    void Update()
    {
        var g=PumpkinGame.Instance;
        if(!g||!g.IsPlaying)
        {
            IsSprinting=IsShooting=false;
            MoveSpeed=0;
            return;
        }
        var k=Keyboard.current;
        var m=Mouse.current;
        Vector3 move=PlayerControls.Move(k);
        aim=Vector3.forward;
        if(g.Campaign&&g.Campaign.BossPhase&&m!=null)
        {
            var ray=Camera.main.ScreenPointToRay(m.position.ReadValue());
            var plane=new Plane(Vector3.up,Vector3.zero);
            if(plane.Raycast(ray,out float d))
            {
                var direction=ray.GetPoint(d)-transform.position;
                direction.y=0;
                if(direction.sqrMagnitude>.1f)aim=direction.normalized;
            }
        }
        visual.rotation=Quaternion.LookRotation(aim);
        IsShooting=arsenal?arsenal.Shooting:m!=null&&m.leftButton.isPressed;
        if(IsShooting&&move.sqrMagnitude<.01f)move=aim;
        IsGrounded=(controller.isGrounded||transform.position.y<.12f)&&vertical<=0;
        if(IsGrounded)vertical=-1;
        if(k!=null&&k[PlayerControls.Get(6)].wasPressedThisFrame&&IsGrounded)
        {
            vertical=7.7f;
            IsGrounded=false;
            if(motion&&motion.animator)motion.animator.SetTrigger("Jump");
            if(jumpSound)RunnerVfx.PlaySound(jumpSound,transform.position,.35f);
        }
        bool sprint=!IsShooting&&PlayerControls.Held(k,4)&&move.sqrMagnitude>0&&stamina>0;
        IsSprinting=sprint;
        float speed=IsShooting?3.2f:sprint?7:4.3f;
        stamina=Mathf.Clamp(stamina+(sprint?-24:16)*Time.deltaTime,0,100);
        Vector3 desired=transform.position+move*speed*Time.deltaTime;
        if(bounds)desired=bounds.Clamp(desired);
        Vector3 delta=Vector3.zero;
        if(NavMesh.SamplePosition(new Vector3(desired.x,.06f,desired.z),out var hit,1,NavMesh.AllAreas)&&Mathf.Abs(hit.position.x-desired.x)<.7f&&Mathf.Abs(hit.position.z-desired.z)<.7f)
        {
            delta=hit.position-transform.position;
            delta.y=0;
        }
        vertical-=Gravity*Time.deltaTime;
        delta.y=vertical*Time.deltaTime;
        Vector3 before=transform.position;
        controller.Move(delta);
        var horizontal=transform.position-before;
        horizontal.y=0;
        MoveSpeed=horizontal.magnitude/Mathf.Max(.0001f,Time.deltaTime);
        visual.localPosition=Vector3.zero;
        if(motion&&motion.animator)motion.animator.SetBool("Grounded",IsGrounded);
        if(IsShooting&&IsGrounded&&MoveSpeed>.1f&&Time.time>fireAt)
        {
            fireAt=Time.time+.10f;
            Fire();
        }
    }
    void Fire()
    {
        if(arsenal&&arsenal.LaserActive)
        {
            if(gun)gun.Fire(false);
            if(motion)motion.Fire();
            arsenal.FireLaser(muzzle.position,aim);
            return;
        }
        var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name="Rifle tracer";
        go.transform.position=muzzle?muzzle.position:transform.position+Vector3.up*1.35f+aim*.9f;
        go.transform.localScale=new Vector3(.15f,.15f,1.05f);
        go.transform.rotation=Quaternion.LookRotation(aim);
        if(gun)gun.Fire();
        if(motion)motion.Fire();
        go.GetComponent<Collider>().enabled=false;
        Destroy(go.GetComponent<Collider>());
        go.GetComponent<Renderer>().sharedMaterial=emberMaterial;
        var shot=go.AddComponent<PumpkinEmber>();
        shot.direction=aim;
        shot.owner=transform;
        RunnerVfx.Sparks(go.transform.position,new Color(1,.6f,.1f),.12f);
    }
}
