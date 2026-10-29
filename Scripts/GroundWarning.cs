using UnityEngine;
/// <summary>World-space warnings stay on the ground and brighten before impact.</summary>
public class GroundWarning : MonoBehaviour
{
    public float duration=3f;
    public bool strip,permanent;
    public Color tint=Color.red;
    public float age;
    LineRenderer outline;
    MeshRenderer fill;
    public static GroundWarning Circle(Vector3 point,float radius,Color color,float seconds)
    {
        var go=new GameObject("Ground target warning");
        go.transform.position=point+Vector3.up*.06f;
        var w=go.AddComponent<GroundWarning>();
        w.tint=color;
        w.duration=seconds;
        w.Build(radius);
        return w;
    }
    public static GroundWarning Lane(float z)
    {
        var w=Circle(new Vector3(0,0,z),1, new Color(1,.35f,.02f),3);
        w.strip=true;
        w.fill.enabled=false;
        w.permanent=true;
        w.transform.localScale=new Vector3(16,1,1.8f);
        w.outline.loop=true;
        w.outline.positionCount=5;
        w.outline.SetPositions(new[]
        {
            new Vector3(-1,0,-1),new Vector3(1,0,-1),new Vector3(1,0,1),new Vector3(-1,0,1),new Vector3(-1,0,-1)
        }
        );
        return w;
    }
    void Build(float radius)
    {
        outline=gameObject.AddComponent<LineRenderer>();
        outline.useWorldSpace=false;
        outline.loop=true;
        outline.positionCount=64;
        outline.sharedMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        outline.widthMultiplier=.10f;
        for(int i=0;i<64;i++)
        {
            float a=i*Mathf.PI*2/64;
            outline.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,.01f,Mathf.Sin(a)*radius));
        }
        var disk=new GameObject("Warning glow");
        disk.transform.SetParent(transform,false);
        var mesh=new Mesh();
        var v=new Vector3[66];
        var triangles=new int[64*3];
        for(int i=0;i<=64;i++)
        {
            float a=i*Mathf.PI*2/64;
            v[i+1]=new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);
            if(i<64)
            {
                triangles[i*3]=0;
                triangles[i*3+1]=i+2;
                triangles[i*3+2]=i+1;
            }
        }
        mesh.vertices=v;
        mesh.triangles=triangles;
        mesh.RecalculateBounds();
        disk.AddComponent<MeshFilter>().sharedMesh=mesh;
        fill=disk.AddComponent<MeshRenderer>();
        var mat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.SetFloat("_Surface",1);
        mat.SetFloat("_SrcBlend",5);
        mat.SetFloat("_DstBlend",10);
        mat.SetFloat("_ZWrite",0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue=3000;
        fill.sharedMaterial=mat;
    }
    void Update()
    {
        var g=PumpkinGame.Instance;
        if(g&&!g.IsPlaying)return;
        age+=Time.deltaTime;
        float t=Mathf.Clamp01(age/Mathf.Max(.1f,duration));
        float glow=Mathf.Lerp(.25f,3f,t)+Mathf.Sin(age*(4+t*12))*.12f;
        outline.sharedMaterial.SetColor("_BaseColor",tint*glow);
        fill.sharedMaterial.SetColor("_BaseColor",new Color(tint.r,tint.g,tint.b,Mathf.Lerp(.06f,.35f,t)));
        if(!permanent&&age>duration+.8f)Destroy(gameObject);
    }
    void OnDestroy()
    {
        if(outline)Destroy(outline.sharedMaterial);
        if(fill)
        {
            Destroy(fill.sharedMaterial);
            Destroy(fill.GetComponent<MeshFilter>().sharedMesh);
        }
    }
}
