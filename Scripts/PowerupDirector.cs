using UnityEngine;
using UnityEngine.AI;
public class PowerupDirector : MonoBehaviour
{
    public GameObject healthPrefab,laserPrefab,airstrikePrefab;
    float healthAt,laserAt,airstrikeAt;
    bool initialized;
    void Start()
    {
        ResetForWave();
    }
    public void ResetForWave()
    {
        healthAt=Time.time+8;
        laserAt=Time.time+17;
        if(!initialized)
        {
            airstrikeAt=Time.time+24;
            initialized=true;
        }
        foreach(var p in FindObjectsByType<PathPowerup>(FindObjectsSortMode.None))Destroy(p.gameObject);
    }
    void Update()
    {
        var g=PumpkinGame.Instance;
        if(!g||!g.IsPlaying||(g.Campaign&&g.Campaign.BossPhase))return;
        if(Time.time>=healthAt)
        {
            Spawn(0);
            healthAt=Time.time+17;
        }
        if(Time.time>=laserAt)
        {
            Spawn(1);
            laserAt=Time.time+32;
        }
        if(Time.time>=airstrikeAt)
        {
            if(g.player.GetComponent<PlayerArsenal>().airstrikes<3)Spawn(2);
            airstrikeAt=Time.time+55;
        }
    }
    public PathPowerup Spawn(int kind)
    {
        var g=PumpkinGame.Instance;
        Vector3 p=g.player.transform.position+new Vector3(Random.Range(-3,3),0,Random.Range(15,25));
        if(!NavMesh.SamplePosition(p,out var hit,3,NavMesh.AllAreas)||hit.position.z<g.player.transform.position.z+10)return null;
        var prefab=kind==0?healthPrefab:kind==1?laserPrefab:airstrikePrefab;
        var go=Instantiate(prefab,hit.position+Vector3.up*.8f,Quaternion.identity);
        go.SetActive(true);
        var pickup=go.GetComponent<PathPowerup>();
        pickup.kind=kind;
        return pickup;
    }
}
