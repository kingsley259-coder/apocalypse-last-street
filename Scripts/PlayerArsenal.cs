using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
public class PlayerArsenal : MonoBehaviour
{
    public int airstrikes;
    public float laserUntil;
    public int laserCells;
    public bool useLaser;
    public GameObject laserModel;
    public Renderer[] rifleRenderers=new Renderer[0];
    public AudioClip laserSound,pickupSound;
    public bool targeting;
    GroundWarning target;
    Vector3 targetPoint;
    public bool LaserActive=>useLaser&&laserCells>0;
    public bool Shooting=>Mouse.current!=null&&Mouse.current.leftButton.isPressed&&!targeting;
    public void GiveHealth()
    {
        var g=PumpkinGame.Instance;
        g.player.health=Mathf.Min(100,g.player.health+35);
        PickupSound();
    }
    public void GiveLaser()
    {
        laserCells=Mathf.Min(160,laserCells+40);
        useLaser=true;
        PickupSound();
    }
    public void GiveAirstrike()
    {
        airstrikes=Mathf.Min(3,airstrikes+1);
        PickupSound();
    }
    void PickupSound()
    {
        if(pickupSound)RunnerVfx.PlaySound(pickupSound,transform.position,.6f);
    }
    void Update()
    {
        var g=PumpkinGame.Instance;
        if(!g||!g.IsPlaying)
        {
            CancelTarget();
            return;
        }
        if(Keyboard.current!=null&&Keyboard.current[PlayerControls.Get(7)].wasPressedThisFrame)useLaser=!useLaser;
        if(laserModel)laserModel.SetActive(LaserActive);
        foreach(var r in rifleRenderers)if(r)r.enabled=!LaserActive;
        if(Keyboard.current!=null&&Keyboard.current[PlayerControls.Get(5)].wasPressedThisFrame&&airstrikes>0)
        {
            if(targeting)CancelTarget();
            else
            {
                targetPoint=transform.position+Vector3.forward*14;
                targeting=true;
                target=GroundWarning.Circle(transform.position+Vector3.forward*14,4,Color.cyan,1);
                target.permanent=true;
            }
        }
        if(!targeting)return;
        var mouse=Mouse.current;
        var ray=Camera.main.ScreenPointToRay(mouse!=null?mouse.position.ReadValue():new Vector2(Screen.width/2,Screen.height/2));
        var plane=new Plane(Vector3.up,Vector3.zero);
        if(plane.Raycast(ray,out float distance))
        {
            var p=ray.GetPoint(distance);
            p.x=Mathf.Clamp(p.x,-5,5);
            p.z=Mathf.Clamp(p.z,transform.position.z-4,transform.position.z+42);
            if(NavMesh.SamplePosition(p,out var hit,3,NavMesh.AllAreas))targetPoint=hit.position;
        }
        target.transform.position=targetPoint+Vector3.up*.08f;
        if(mouse!=null&&mouse.rightButton.wasPressedThisFrame)
        {
            CancelTarget();
            return;
        }
        if(mouse!=null&&mouse.leftButton.wasPressedThisFrame)Deploy(targetPoint);
    }
    public bool Deploy(Vector3 point)
    {
        if(airstrikes<=0||!MissileDirector.Instance)return false;
        airstrikes--;
        MissileDirector.Instance.Airstrike(point);
        CancelTarget();
        return true;
    }
    public void ResetTargeting()
    {
        CancelTarget();
    }
    void CancelTarget()
    {
        targeting=false;
        if(target)Destroy(target.gameObject);
    }
    public void FireLaser(Vector3 origin,Vector3 direction)
    {
        if(laserCells<=0)return;
        laserCells--;
        var g=PumpkinGame.Instance;
        Vector3 end=origin+direction*65;
        var hits=Physics.RaycastAll(origin,direction,65,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
        foreach(var hit in hits)
        {
            if(hit.collider.transform.IsChildOf(transform))continue;
            end=hit.point;
            var zombie=hit.collider.GetComponentInParent<ZombieEnemy>();
            if(zombie&&!zombie.dead)zombie.TakeHit(95,false);
            var boss=hit.collider.GetComponentInParent<AxePursuer>();
            if(boss)boss.TakeHit(95);
            RunnerVfx.Sparks(hit.point,Color.cyan,.25f);
            break;
        }
        RunnerVfx.Beam(origin,end,new Color(.1f,3,4),.12f);
        if(laserSound)RunnerVfx.PlaySound(laserSound,origin,.3f);
    }
    void OnDestroy()
    {
        CancelTarget();
    }
    void OnGUI()
    {
        GUI.matrix=Matrix4x4.identity;
        var g=PumpkinGame.Instance;
        if(!g||!g.IsPlaying)return;
        string status=LaserActive?"LASER "+laserCells+" CELLS":"RIFLE   |   LASER "+laserCells+" CELLS  ("+PlayerControls.Label(7)+")";
        status+="   |   AIRSTRIKE "+airstrikes+"/3"+(targeting?"  LEFT CLICK TARGET / RIGHT CLICK CANCEL":airstrikes>0?"  "+PlayerControls.Label(5)+" TO AIM":"");
        GUI.color=Color.cyan;
        GUI.Label(new Rect(20,Screen.height-126,Screen.width-40,26),status,new GUIStyle(GUI.skin.label)
        {
            font=g.uiFont,fontSize=18,fontStyle=FontStyle.Bold
        }
        );
        GUI.color=Color.white;
    }
}
