using UnityEngine;
public class PumpkinHazardManager : MonoBehaviour
{
    public GameObject pumpkinPrefab;
    public float interval=17f,warningSeconds=3f;
    float nextAt,spawnAt=-1,crossZ;
    int side=1;
    bool warning;
    GroundWarning lane;
    RollingPumpkin activePumpkin;
    public int spawnedCount;
    void Start()
    {
        ResetForWave();
    }
    public void ResetForWave()
    {
        nextAt=Time.time+10;
        spawnAt=-1;
        warning=false;
        if(lane)Destroy(lane.gameObject);
        foreach(var b in FindObjectsByType<RollingPumpkin>(FindObjectsSortMode.None))Destroy(b.gameObject);
    }
    public void ScheduleCrossing()
    {
        var g=PumpkinGame.Instance;
        if(!g)return;
        crossZ=g.player.transform.position.z+18;
        if(lane)Destroy(lane.gameObject);
        lane=GroundWarning.Lane(crossZ);
        warning=true;
        spawnAt=Time.time+warningSeconds;
        if(HorrorAudio.Instance)HorrorAudio.Instance.Warn();
    }
    void Update()
    {
        var g=PumpkinGame.Instance;
        if(!g||!g.IsPlaying||(g.Campaign&&g.Campaign.BossPhase))return;
        if(!warning&&!activePumpkin&&lane)Destroy(lane.gameObject);
        if(!warning&&Time.time>=nextAt)ScheduleCrossing();
        if(warning&&Time.time>=spawnAt)
        {
            var go=Instantiate(pumpkinPrefab,new Vector3(-side*17,1.6f,crossZ),Quaternion.identity);
            go.SetActive(true);
            var b=go.AddComponent<RollingPumpkin>();
            b.direction=Vector3.right*side;
            activePumpkin=b;
            spawnedCount++;
            side=-side;
            warning=false;
            nextAt=Time.time+interval;
        }
    }
}
