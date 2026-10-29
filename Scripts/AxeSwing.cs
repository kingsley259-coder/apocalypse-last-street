using UnityEngine;
public class AxeSwing : MonoBehaviour
{
    public AxePursuer pursuer;
    Quaternion rest;
    void Start()
    {
        rest=transform.localRotation;
    }
    void LateUpdate()
    {
        if(!pursuer)return;
        float t=pursuer.SwingProgress;
        transform.localRotation=t>0?rest*Quaternion.Euler(Mathf.Lerp(-75,55,Mathf.SmoothStep(0,1,t)),0,0):rest;
    }
}
