using UnityEngine;
using UnityEngine.AI;
public class ZombieEnemy : MonoBehaviour
{
    public bool female,dead;
    public float health=100;
    public Transform visual;
    NavMeshAgent agent;
    float attackAt,repathAt;
    float hitUntil;
    void Awake()
    {
        agent=GetComponent<NavMeshAgent>();
    }
    void OnEnable()
    {
        dead=false;
        health=female?150:65;
        if(agent)agent.speed=female?3.2f:2.2f;
    }
    void Update()
    {
        var game=PumpkinGame.Instance;
        if(!game||!agent||!agent.isOnNavMesh||dead)return;
        if(!game.IsPlaying)
        {
            agent.isStopped=true;
            return;
        }
        float distance=Vector3.Distance(transform.position,game.player.transform.position);
        if(Time.time>repathAt)
        {
            repathAt=Time.time+.2f;
            agent.SetDestination(game.player.transform.position);
        }
        agent.isStopped=distance<1.55f||Time.time<hitUntil;
        if(distance<1.8f)
        {
            Vector3 d=game.player.transform.position-transform.position;
            d.y=0;
            if(d.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(d),Time.deltaTime*7);
            if(Time.time>attackAt)
            {
                attackAt=Time.time+(female?1:1.5f);
                game.DamagePlayer(female?18:7);
            }
        }
    }
    public void TakeHit(float damage,bool push)
    {
        if(dead)return;
        health-=damage;
        hitUntil=Time.time+.15f;
        if(health<=0)
        {
            dead=true;
            PumpkinGame.Instance.kills++;
            PumpkinGame.Instance.AddPoints(female?25:10);
            if(agent&&agent.isOnNavMesh)agent.isStopped=true;
            foreach(var c in GetComponents<Collider>())c.enabled=false;
            if(!GetComponent<HumanoidMotion>())visual.rotation*=Quaternion.Euler(-75,0,15);
            Destroy(gameObject,3.2f);
        }
        else if(push&&agent&&agent.isOnNavMesh)
        {
            Vector3 dir=(transform.position-PumpkinGame.Instance.player.transform.position).normalized;
            NavMeshHit hit;
            if(NavMesh.SamplePosition(transform.position+dir*2,out hit,2,NavMesh.AllAreas))agent.Warp(hit.position);
        }
    }
}
