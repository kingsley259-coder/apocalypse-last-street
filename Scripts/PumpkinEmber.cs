using UnityEngine;
/// <summary>Forward rifle tracer that ignores the shooter and trigger volumes.</summary>
public class PumpkinEmber : MonoBehaviour
{
    public Transform owner;
    public Vector3 direction=Vector3.forward;
    public float speed=85f, damage=50f, lifetime=3f;
    void Update()
    {
        if(Time.deltaTime<=0f)return;
        Vector3 forward=direction.normalized;
        float step=speed*Time.deltaTime;
        var hits=Physics.SphereCastAll(transform.position,.09f,forward,step,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
        foreach(var hit in hits)
        {
            if(owner&&hit.collider.transform.IsChildOf(owner))continue;
            var zombie=hit.collider.GetComponentInParent<ZombieEnemy>();
            if(zombie&&zombie.dead)continue;
            if(zombie)
            {
                zombie.TakeHit(damage,false);
                if(HorrorAudio.Instance)HorrorAudio.Instance.Hit(hit.point);
            }
            var boss=hit.collider.GetComponentInParent<AxePursuer>();
            if(boss)boss.TakeHit(damage);
            RunnerVfx.Sparks(hit.point,new Color(1,.6f,.1f),.18f);
            Destroy(gameObject);
            return;
        }
        transform.position+=forward*step;
        lifetime-=Time.deltaTime;
        if(lifetime<=0f)Destroy(gameObject);
    }
}
