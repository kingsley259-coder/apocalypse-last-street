using UnityEngine;
using UnityEngine.AI;
/// <summary>Spawns reachable, separated zombies in the active road section.</summary>
[DisallowMultipleComponent,RequireComponent(typeof(PumpkinGame))] public class SpawnManager : MonoBehaviour
{
    [Min(1)] public int maximumAlive=28;
    [Min(1f)] public float minimumPlayerDistance=13f;
    [Min(.1f)] public float minimumZombieSpacing=1.2f;
    [Min(1)] public int attemptsPerZombie=80;
    PumpkinGame game;
    float refillAt;
    public float lastSpawnMinimumAhead=999;
    void Update()
    {
        if(!game||!game.IsPlaying||(game.Campaign&&game.Campaign.BossPhase))return;
        foreach(var z in game.enemies.ToArray())if(z&&!z.dead&&z.transform.position.z<game.player.transform.position.z-30)
        {
            game.enemies.Remove(z);
            Destroy(z.gameObject);
        }
        if(Time.time<refillAt)return;
        refillAt=Time.time+(game.Campaign?(game.Campaign.difficulty==ApocalypseCampaign.Difficulty.Easy?5:game.Campaign.difficulty==ApocalypseCampaign.Difficulty.Hard?2.8f:3.5f):3.5f);
        int desired=Mathf.Min(maximumAlive,game.Campaign?game.Campaign.Population:8+game.wave*2);
        SpawnWave(Mathf.Min(2+game.wave/3,Mathf.Max(0,desired-game.enemies.Count)));
    }
    public void ClearForCheckpoint()
    {
        foreach(var z in game.enemies.ToArray())if(z)Destroy(z.gameObject);
        game.enemies.Clear();
        refillAt=Time.time+4;
    }
    void Awake()
    {
        game=GetComponent<PumpkinGame>();
    }
    public int SpawnWave(int requested)
    {
        if(!game)game=GetComponent<PumpkinGame>();
        if(!game||!game.IsPlaying||(game.Campaign&&game.Campaign.BossPhase)||!game.player||!game.malePrefab||!game.femalePrefab)return 0;
        game.enemies.RemoveAll(z=>!z||z.dead);
        int count=Mathf.Min(Mathf.Max(0,requested),maximumAlive-game.enemies.Count),spawned=0;
        var path=new NavMeshPath();
        for(int i=0;i<count;i++)for(int attempt=0;attempt<attemptsPerZombie;attempt++)
        {
            Vector3 c=game.player.transform.position+new Vector3(Random.Range(-4.5f,4.5f),0,Random.Range(16f,36f));
            if(!NavMesh.SamplePosition(c,out var hit,3,NavMesh.AllAreas)||hit.position.z<game.player.transform.position.z+13||Vector3.Distance(hit.position,game.player.transform.position)<minimumPlayerDistance)continue;
            if(!NavMesh.CalculatePath(hit.position,game.player.transform.position,NavMesh.AllAreas,path)||path.status!=NavMeshPathStatus.PathComplete)continue;
            if(game.enemies.Exists(z=>z&&!z.dead&&Vector3.Distance(z.transform.position,hit.position)<minimumZombieSpacing))continue;
            var zombie=Instantiate(Random.value<.2f?game.femalePrefab:game.malePrefab,hit.position,Quaternion.LookRotation((game.player.transform.position-hit.position).normalized));
            zombie.gameObject.SetActive(true);
            zombie.health*=(game.Campaign?(game.Campaign.difficulty==ApocalypseCampaign.Difficulty.Easy?.8f:game.Campaign.difficulty==ApocalypseCampaign.Difficulty.Hard?1.25f:1):1)*(1+(game.wave-1)*.06f);
            zombie.GetComponent<NavMeshAgent>().speed=Mathf.Min(zombie.female?3.8f:2.8f,(zombie.female?3.2f:2.2f)*(game.Campaign?game.Campaign.SpeedMultiplier:1)*(1+(game.wave-1)*.02f));
            lastSpawnMinimumAhead=hit.position.z-game.player.transform.position.z;
            game.enemies.Add(zombie);
            spawned++;
            break;
        }
        return spawned;
    }
}
