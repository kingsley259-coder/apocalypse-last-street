using UnityEngine;
public class PathPowerup : MonoBehaviour
{
    public int kind;
    public bool collected;
    float age;
    Vector3 rest;
    void Start()
    {
        rest=transform.position;
    }
    void Update()
    {
        var g=PumpkinGame.Instance;
        if(!g||!g.IsPlaying)return;
        age+=Time.deltaTime;
        transform.position=rest+Vector3.up*Mathf.Sin(age*3)*.18f;
        if(kind!=0)transform.Rotate(Vector3.up,60*Time.deltaTime,Space.World);
        if(Vector3.Distance(transform.position,g.player.transform.position+Vector3.up*.8f)<1.3f)Collect(g.player.GetComponent<PlayerArsenal>());
        if(age>45||transform.position.z<g.player.transform.position.z-25)Destroy(gameObject);
    }
    public void Collect(PlayerArsenal arsenal)
    {
        if(collected||!arsenal)return;
        if(kind==0&&PumpkinGame.Instance.player.health>=100)return;
        if(kind==2&&arsenal.airstrikes>=3)return;
        collected=true;
        if(kind==0)arsenal.GiveHealth();
        else if(kind==1)arsenal.GiveLaser();
        else arsenal.GiveAirstrike();
        Destroy(gameObject);
    }
}
