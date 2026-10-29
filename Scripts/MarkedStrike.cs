using UnityEngine;
/// <summary>Red telegraph, visible descending missile, then a compact radial blast.</summary>
public class MarkedStrike : MonoBehaviour
{
    public bool friendly;
    public float delay,warningTime=3f,radius=2.2f;
    public float age;
    public bool detonated;
    GroundWarning marker;
    GameObject missile;
    void Update()
    {
        var g=PumpkinGame.Instance;
        if(!g||!g.IsPlaying)return;
        age+=Time.deltaTime;
        if(age<delay)return;
        float t=age-delay;
        if(!marker)
        {
            marker=GroundWarning.Circle(transform.position,friendly?4:radius,friendly?Color.cyan:Color.red,warningTime);
            marker.permanent=true;
        }
        if(t>=warningTime&&!missile)
        {
            missile=GameObject.CreatePrimitive(PrimitiveType.Capsule);
            missile.name="Falling missile";
            missile.GetComponent<Collider>().enabled=false;
            missile.transform.localScale=new Vector3(.18f,.65f,.18f);
            missile.GetComponent<Renderer>().sharedMaterial=RunnerVfx.Material(new Color(.7f,.72f,.75f));
            if(MissileDirector.Instance.launchSound)RunnerVfx.PlaySound(MissileDirector.Instance.launchSound,transform.position,.3f);
        }
        if(missile)missile.transform.position=transform.position+Vector3.up*Mathf.Lerp(24,.2f,(t-warningTime)/.55f);
        if(t>=warningTime+.55f)Detonate();
    }
    public void Detonate()
    {
        if(detonated)return;
        detonated=true;
        var g=PumpkinGame.Instance;
        if(g)
        {
            if(!friendly&&Vector3.Distance(g.player.transform.position,transform.position)<radius)g.DamagePlayer(1000);
            foreach(var z in g.enemies.ToArray())if(z&&!z.dead&&Vector3.Distance(z.transform.position,transform.position)<(friendly?5:radius))z.TakeHit(friendly?300:110,false);
        }
        if(friendly)
        {
            var boss=Object.FindFirstObjectByType<AxePursuer>();
            if(boss&&Vector3.Distance(boss.transform.position,transform.position)<5)boss.TakeHit(300);
        }
        RunnerVfx.Burst(transform.position+Vector3.up*.2f,friendly?1.25f:1);
        if(MissileDirector.Instance)MissileDirector.Instance.Boom(transform.position);
        Destroy(gameObject);
    }
    void OnDestroy()
    {
        if(marker)Destroy(marker.gameObject);
        if(missile)
        {
            Destroy(missile.GetComponent<Renderer>().sharedMaterial);
            Destroy(missile);
        }
    }
}
