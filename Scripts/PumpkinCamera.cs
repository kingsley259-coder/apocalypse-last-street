using UnityEngine;
/// <summary>Fixed street-facing camera. Movement never rotates or orbits the view.</summary>
public class PumpkinCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset=new Vector3(0,4.8f,-8);
    public float followSharpness=10,turnSharpness=7;
    Quaternion view;
    void Awake()
    {
        view=Quaternion.LookRotation(Vector3.up*1.1f-offset,Vector3.up);
    }
    public void Snap()
    {
        if(!target)return;
        transform.position=target.position+offset;
        transform.rotation=Quaternion.LookRotation(Vector3.up*1.1f-offset,Vector3.up);
        view=transform.rotation;
    }
    void LateUpdate()
    {
        if(!target)return;
        transform.position=target.position+offset;
        transform.rotation=view;
    }
}
