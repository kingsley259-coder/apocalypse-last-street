using UnityEngine;
public class HorrorAudio : MonoBehaviour
{
    public static HorrorAudio Instance;
    public AudioClip impact,warning,rolling,footstep;
    AudioSource fx;
    Vector3 previous;
    float stepAt;
    void Awake()
    {
        Instance=this;
        fx=gameObject.AddComponent<AudioSource>();
        fx.volume=.5f;
    }
    void Update()
    {
        fx.volume=.5f*GameAudio.Effects;
        var g=PumpkinGame.Instance;
        if(!g||!g.player)return;
        var pos=g.player.transform.position;
        if(g.IsPlaying&&g.player.IsGrounded&&Vector3.Distance(pos,previous)>.005f&&Time.time>stepAt)
        {
            fx.PlayOneShot(footstep,.18f);
            stepAt=Time.time+(g.player.IsSprinting?.22f:.38f);
        }
        previous=pos;
    }
    public void Hit(Vector3 position)
    {
        if(impact)fx.PlayOneShot(impact,.45f);
    }
    public void Warn()
    {
        if(warning)fx.PlayOneShot(warning,.65f);
    }
    void OnDestroy()
    {
        if(Instance==this)Instance=null;
    }
}
