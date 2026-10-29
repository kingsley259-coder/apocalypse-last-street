using UnityEngine;
public static class RunnerVfx
{
    static Material lineMaterial,sparkMaterial;
    public static Material Material(Color color)
    {
        var m=new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        m.SetColor("_BaseColor",color);
        return m;
    }
    public static void PlaySound(AudioClip clip,Vector3 point,float volume)
    {
        if(!clip)return;
        var go=new GameObject("Runner sound");
        go.transform.position=point;
        var source=go.AddComponent<AudioSource>();
        source.clip=clip;
        source.volume=volume*GameAudio.Effects;
        source.spatialBlend=.7f;
        source.minDistance=8;
        source.maxDistance=90;
        source.Play();
        Object.Destroy(go,clip.length+.2f);
    }
    public static void Beam(Vector3 a,Vector3 b,Color color,float width=.08f)
    {
        var go=new GameObject("Laser streak");
        var line=go.AddComponent<LineRenderer>();
        if(!lineMaterial)lineMaterial=Material(Color.white);
        line.sharedMaterial=lineMaterial;
        line.startColor=line.endColor=color;
        line.startWidth=width;
        line.endWidth=width*.3f;
        line.positionCount=2;
        line.SetPosition(0,a);
        line.SetPosition(1,b);
        Object.Destroy(go,.12f);
    }
    public static void Sparks(Vector3 point,Color color,float size)
    {
        var go=new GameObject("Impact sparks");
        go.transform.position=point;
        var ps=go.AddComponent<ParticleSystem>();
        ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
        var main=ps.main;
        main.loop=false;
        main.duration=.15f;
        main.startLifetime=.22f;
        main.startSpeed=3;
        main.startSize=size;
        main.startColor=color;
        var emission=ps.emission;
        emission.rateOverTime=0;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0,8)
        }
        );
        if(!sparkMaterial)sparkMaterial=Material(new Color(3,1.3f,.15f));
        ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=sparkMaterial;
        ps.Play();
        Object.Destroy(go,1);
    }
    public static void Burst(Vector3 point,float scale=1)
    {
        var director=MissileDirector.Instance;
        if(director&&director.explosionPrefab)
        {
            var root=new GameObject("Missile explosion");root.transform.position=point;root.transform.localScale=Vector3.one*scale*6;
            var fx=Object.Instantiate(director.explosionPrefab,root.transform);fx.transform.localPosition=Vector3.zero;
            foreach(var ps in fx.GetComponentsInChildren<ParticleSystem>())
            {
                var main=ps.main;
                main.scalingMode=ParticleSystemScalingMode.Hierarchy;
                main.stopAction=ParticleSystemStopAction.None;
                ps.Play();
            }
            Object.Destroy(root,5);
        }
        else
        {
            var go=new GameObject("Small blast");
            go.transform.position=point;
            var ps=go.AddComponent<ParticleSystem>();
            ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;
            main.loop=false;
            main.duration=.3f;
            main.startLifetime=.55f;
            main.startSpeed=4*scale;
            main.startSize=.5f*scale;
            main.startColor=new Color(1,.35f,.04f);
            var emission=ps.emission;
            emission.rateOverTime=0;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0,35)
            }
            );
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=Material(new Color(2,.6f,.05f));
            ps.Play();
            Object.Destroy(go,2);
        }
    }
}
