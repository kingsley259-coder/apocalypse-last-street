using UnityEngine;
using UnityEngine.AI;
[DefaultExecutionOrder(-100)] public class ApocalypseCampaign : MonoBehaviour
{
    public enum Difficulty
    {
        Easy, Medium, Hard
    }
    public Difficulty difficulty=Difficulty.Medium;
    public int level=1,points;
    public bool started;
    public bool BossPhase=>level==4;
    public bool HasCheckpoint=>PlayerPrefs.GetInt("AP.Save",0)==1;
    public float SpeedMultiplier=>difficulty==Difficulty.Easy?.7f:difficulty==Difficulty.Hard?1.15f:1f;
    public int Population=>difficulty==Difficulty.Easy?6+level*2:difficulty==Difficulty.Hard?12+level*3:8+level*2;
    public float CheckpointZ=>level==1?175:level==2?353:480;
    PumpkinGame game;
    void Awake()
    {
        game=GetComponent<PumpkinGame>();
        difficulty=(Difficulty)PlayerPrefs.GetInt("AP.Mode",1);
        level=HasCheckpoint?Mathf.Clamp(PlayerPrefs.GetInt("AP.Level",1),1,4):1;
        points=HasCheckpoint?PlayerPrefs.GetInt("AP.Points"):0;
        game.wave=level;
    }
    public void NewRun(Difficulty mode)
    {
        difficulty=mode;
        level=1;
        points=0;
        started=false;
        game.wave=1;
        game.kills=0;
        game.ended=game.won=false;
        game.player.health=100;
        var a=game.player.GetComponent<PlayerArsenal>();
        a.airstrikes=a.laserCells=0;
        a.laserUntil=0;
        PlayerPrefs.SetInt("AP.Save",0);
        PlayerPrefs.SetInt("AP.Mode",(int)mode);
        Prepare(true);
        Save();
    }
    public void EnsureStarted()
    {
        if(started)return;
        Prepare(true);
    }
    void ClearEnemies()
    {
        foreach(var z in Object.FindObjectsByType<ZombieEnemy>(FindObjectsSortMode.None))
        {
            z.gameObject.SetActive(false);
            Destroy(z.gameObject);
        }
        game.enemies.Clear();
    }
    void Prepare(bool restore)
    {
        ClearEnemies();
        game.wave=level;
        if(restore)
        {
            Vector3 pos=HasCheckpoint?new Vector3(PlayerPrefs.GetFloat("AP.X"),.06f,PlayerPrefs.GetFloat("AP.Z",15)):new Vector3(0,.06f,15);
            if(NavMesh.SamplePosition(pos,out var hit,3,NavMesh.AllAreas))
            {
                var c=game.player.GetComponent<CharacterController>();
                c.enabled=false;
                game.player.transform.position=hit.position;
                c.enabled=true;
            }
            game.player.health=HasCheckpoint?PlayerPrefs.GetFloat("AP.Health",100):100;
            var a=game.player.GetComponent<PlayerArsenal>();
            if(HasCheckpoint)
            {
                a.airstrikes=PlayerPrefs.GetInt("AP.Air");
                a.laserCells=PlayerPrefs.GetInt("AP.Cells");
                a.useLaser=false;
            }
            Object.FindFirstObjectByType<PumpkinCamera>().Snap();
        }
        var manager=game.spawnManager;
        if(manager)manager.enabled=!BossPhase;
        var missiles=GetComponent<MissileDirector>();
        if(missiles)
        {
            missiles.ResetForWave();
            missiles.enabled=!BossPhase;
        }
        var pumpkins=GetComponent<PumpkinHazardManager>();
        if(pumpkins)
        {
            pumpkins.ResetForWave();
            pumpkins.enabled=!BossPhase;
        }
        var pickups=GetComponent<PowerupDirector>();
        if(pickups)
        {
            pickups.ResetForWave();
            pickups.enabled=!BossPhase;
        }
        var boss=Object.FindFirstObjectByType<AxePursuer>();
        if(boss)
        {
            if(BossPhase)boss.BeginBossFight();
            else boss.ResetForWave();
        }
        started=true;
        if(!BossPhase)game.SpawnWave(Population);
    }
    public void Advance()
    {
        if(level>=4)return;
        points+=250*level;
        level++;
        game.wave=level;
        Prepare(false);
        Save();
    }
    public void Save()
    {
        PlayerPrefs.SetInt("AP.Save",1);
        PlayerPrefs.SetInt("AP.Mode",(int)difficulty);
        PlayerPrefs.SetInt("AP.Level",level);
        PlayerPrefs.SetInt("AP.Points",points);
        PlayerPrefs.SetFloat("AP.X",game.player.transform.position.x);
        PlayerPrefs.SetFloat("AP.Z",game.player.transform.position.z);
        PlayerPrefs.SetFloat("AP.Health",Mathf.Max(1,game.player.health));
        var a=game.player.GetComponent<PlayerArsenal>();
        PlayerPrefs.SetInt("AP.Air",a.airstrikes);
        PlayerPrefs.SetInt("AP.Cells",a.laserCells);
        PlayerPrefs.Save();
    }
    public void Victory()
    {
        points+=2000;
        PlayerPrefs.SetInt("AP.Save",0);
        game.WinGame();
    }
}
