using UnityEngine;
using UnityEngine.AI;
public class AxePursuer : MonoBehaviour
{
    public Animator animator;
    public Transform axe;
    public float followSpeed=2.6f,startingDistance=22;
    public AudioClip roar,axeHit;
    public float health=2400,maxHealth=2400;
    public bool bossFight,dead;
    public float SwingProgress=>Time.time<attackUntil?1-(attackUntil-Time.time)/1.6f:0;
    NavMeshAgent agent;
    float repath,attackUntil,damageAt;
    bool struck;
    void Awake()
    {
        agent=GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        if(!bossFight)ResetForWave();
    }
    public void ResetForWave()
    {
        var g=PumpkinGame.Instance;
        if(!g)return;
        bossFight=dead=false;
        var p=g.player.transform.position-Vector3.forward*startingDistance;
        if(NavMesh.SamplePosition(p,out var hit,16,NavMesh.AllAreas))agent.Warp(hit.position);
        attackUntil=0;
        struck=false;
        if(animator)
        {
            animator.SetBool("Attack",false);
            animator.SetBool("Dead",false);
        }
    }
    public void BeginBossFight()
    {
        var g=PumpkinGame.Instance;
        bossFight=true;
        dead=false;
        int mode=(int)g.Campaign.difficulty;
        maxHealth=mode==0?1800:mode==2?3200:2400;
        health=maxHealth;
        attackUntil=0;
        if(NavMesh.SamplePosition(g.player.transform.position+Vector3.forward*20,out var hit,8,NavMesh.AllAreas))agent.Warp(hit.position);
        if(animator)animator.SetBool("Attack",false);
    }
    public void TakeHit(float damage)
    {
        if(!bossFight||dead||!PumpkinGame.Instance.IsPlaying)return;
        health=Mathf.Max(0,health-damage*.55f);
        RunnerVfx.Sparks(transform.position+Vector3.up*1.5f,new Color(1,.35f,.1f),.3f);
        if(health<=0)
        {
            dead=true;
            agent.isStopped=true;
            if(animator)animator.SetBool("Dead",true);
            PumpkinGame.Instance.Campaign.Victory();
        }
    }
    void Update()
    {
        var g=PumpkinGame.Instance;
        if(!g||!agent||!agent.isOnNavMesh)return;
        if(!g.IsPlaying||dead)
        {
            agent.isStopped=true;
            return;
        }
        float gap=Vector3.Distance(transform.position,g.player.transform.position);
        int mode=g.Campaign?(int)g.Campaign.difficulty:1;
        if(Time.time<attackUntil)
        {
            agent.isStopped=true;
            if(!struck&&Time.time>=damageAt)
            {
                struck=true;
                if(gap<2.8f)
                {
                    if(axeHit)RunnerVfx.PlaySound(axeHit,transform.position,.45f);
                    g.DamagePlayer(bossFight?(mode==0?30:mode==2?60:45):1000);
                }
            }
            return;
        }
        if(gap<2.3f)
        {
            float windup=mode==0?1.1f:mode==2?.75f:.9f;
            attackUntil=Time.time+1.6f;
            damageAt=Time.time+windup;
            struck=false;
            if(animator)animator.SetBool("Attack",true);
            return;
        }
        if(animator)
        {
            animator.SetBool("Attack",false);
            animator.SetFloat("Speed",agent.velocity.magnitude);
        }
        if(!bossFight&&gap>50)
        {
            if(NavMesh.SamplePosition(g.player.transform.position-Vector3.forward*32,out var hit,8,NavMesh.AllAreas))agent.Warp(hit.position);
        }
        agent.isStopped=false;
        agent.speed=bossFight?(mode==0?2.2f:mode==2?3.15f:2.7f):(mode==0?1.9f:mode==2?3f:2.6f);
        if(Time.time>repath)
        {
            repath=Time.time+.2f;
            agent.SetDestination(g.player.transform.position);
        }
    }
    void OnGUI()
    {
        GUI.matrix=Matrix4x4.identity;
        var g=PumpkinGame.Instance;
        if(!g||!g.IsPlaying||dead)return;
        if(bossFight)
        {
            var p=Camera.main.WorldToScreenPoint(transform.position+Vector3.up*3.3f);
            if(p.z>0)
            {
                float x=p.x-90,y=Screen.height-p.y;
                GUI.color=new Color(.03f,.03f,.03f,.9f);
                GUI.DrawTexture(new Rect(x,y,180,14),Texture2D.whiteTexture);
                GUI.color=new Color(.9f,.15f,.1f);
                GUI.DrawTexture(new Rect(x+2,y+2,176*health/maxHealth,10),Texture2D.whiteTexture);
                GUI.color=Color.white;
                GUI.Label(new Rect(x,y-23,200,24),"BOSS MONSTER  "+Mathf.CeilToInt(health),new GUIStyle(GUI.skin.label)
                {
                    fontStyle=FontStyle.Bold
                }
                );
            }
        }
        else if(Vector3.Distance(transform.position,g.player.transform.position)<15)
        {
            GUI.color=new Color(1,.3f,.1f);
            GUI.Label(new Rect(Screen.width/2-180,55,360,28),"BOSS MONSTER IS CLOSING IN",new GUIStyle(GUI.skin.label)
            {
                alignment=TextAnchor.MiddleCenter,fontSize=17,fontStyle=FontStyle.Bold
            }
            );
            GUI.color=Color.white;
        }
    }
}
