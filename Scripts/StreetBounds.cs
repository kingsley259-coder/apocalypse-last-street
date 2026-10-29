using UnityEngine;
public class StreetBounds : MonoBehaviour
{
    public float halfWidth=4.8f,minZ=10,maxZ=532;
    public Vector3 Clamp(Vector3 p)
    {
        var g=PumpkinGame.Instance;
        float back=g&&g.Campaign&&g.Campaign.BossPhase?460:minZ;
        p.x=Mathf.Clamp(p.x,-halfWidth,halfWidth);
        p.z=Mathf.Clamp(p.z,back,maxZ);
        return p;
    }
}
