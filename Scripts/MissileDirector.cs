using UnityEngine;
using UnityEngine.AI;
public class MissileDirector : MonoBehaviour
{
    public static MissileDirector Instance;
    public GameObject explosionPrefab;
    public AudioClip explosionSound,launchSound;
    public float nextStrike;
    public int strikesCreated;
    public float minAhead=14,maxAhead=28;
    void Awake()
    {
        Instance=this;
    }
    void Start()
    {
        ResetForWave();
    }
    public void ResetForWave()
    {
        nextStrike=Time.time+9;
        foreach(var strike in FindObjectsByType<MarkedStrike>(FindObjectsSortMode.None))Destroy(strike.gameObject);
    }
    void Update()
    {
        var g=PumpkinGame.Instance;
        if(!g||!g.IsPlaying||(g.Campaign&&g.Campaign.BossPhase))return;
        if(Time.time>=nextStrike)
        {
            var candidate=g.player.transform.position+new Vector3(Random.Range(-3.5f,3.5f),0,Random.Range(minAhead,maxAhead));
            if(NavMesh.SamplePosition(candidate,out var hit,2,NavMesh.AllAreas)&&hit.position.z>g.player.transform.position.z+10)Schedule(hit.position,false);
            nextStrike=Time.time+Random.Range(6f,10f);
        }
    }
    public MarkedStrike Schedule(Vector3 point,bool friendly,float delay=0)
    {
        var go=new GameObject(friendly?"Player airstrike missile":"Hostile marked missile");
        go.transform.position=point;
        var strike=go.AddComponent<MarkedStrike>();
        strike.friendly=friendly;
        strike.delay=delay;
        strikesCreated++;
        return strike;
    }
    public void Airstrike(Vector3 point)
    {
        for(int i=0;i<3;i++)Schedule(point+Vector3.right*(i-1)*3,true,i*.25f);
    }
    public void Boom(Vector3 point)
    {
        if(explosionSound)RunnerVfx.PlaySound(explosionSound,point,.65f);
    }
    void OnDestroy()
    {
        if(Instance==this)Instance=null;
    }
}
