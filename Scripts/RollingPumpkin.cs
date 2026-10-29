using UnityEngine;
/// <summary>Kinematic pumpkin boulder with continuous crossing checks and rolling sound.</summary>
public class RollingPumpkin : MonoBehaviour
{
    public Vector3 direction=Vector3.right;
    public float speed=8f,radius=1.6f;
    float age;
    bool hitPlayer;
    AudioSource roll;
    void Start()
    {
        if(HorrorAudio.Instance&&HorrorAudio.Instance.rolling)
        {
            var a=gameObject.AddComponent<AudioSource>();
            roll=a;
            a.clip=HorrorAudio.Instance.rolling;
            a.loop=true;
            a.spatialBlend=1;
            a.minDistance=3;
            a.maxDistance=45;
            a.volume=.7f*GameAudio.Effects;
            a.Play();
        }
    }
    void Update()
    {
        if(roll)roll.volume=.7f*GameAudio.Effects;
        var g=PumpkinGame.Instance;
        if(!g||!g.IsPlaying)
        {
            if(roll)roll.Pause();
            return;
        }
        if(roll&&!roll.isPlaying)roll.UnPause();
        Vector3 old=transform.position;
        transform.position+=direction*speed*Time.deltaTime;
        transform.Rotate(Vector3.Cross(Vector3.up,direction),speed*Time.deltaTime/radius*Mathf.Rad2Deg,Space.World);
        age+=Time.deltaTime;
        Vector3 p=g.player.transform.position+Vector3.up*.9f;
        Vector3 segment=transform.position-old;
        float t=segment.sqrMagnitude>0?Mathf.Clamp01(Vector3.Dot(p-old,segment)/segment.sqrMagnitude):0;
        if(!hitPlayer&&Vector3.Distance(p,old+segment*t)<radius+.4f)
        {
            hitPlayer=true;
            g.DamagePlayer(1000);
            if(HorrorAudio.Instance)HorrorAudio.Instance.Hit(p);
        }
        foreach(var z in g.enemies.ToArray())if(z&&!z.dead&&Vector3.Distance(z.transform.position+Vector3.up,transform.position)<radius+.4f)z.TakeHit(250,false);
        if(age>6||Mathf.Abs(transform.position.x)>24)Destroy(gameObject);
    }
}
