using UnityEngine;
public class GunFeedback : MonoBehaviour
{
    public Transform muzzle;
    public Light flash;
    public AudioSource source;
    public AudioClip shot;
    Vector3 rest;
    float recoil;
    void Start()
    {
        rest=transform.localPosition;
        if(flash)flash.enabled=false;
    }
    public void Fire(bool playSound=true)
    {
        recoil=1;
        if(flash&&PlayerPrefs.GetInt("AP.ReducedFlashes",0)==0)
        {
            flash.enabled=true;
            CancelInvoke(nameof(HideFlash));
            Invoke(nameof(HideFlash),.045f);
        }
        if(playSound&&source&&shot)source.PlayOneShot(shot,.35f*GameAudio.Effects);
    }
    void HideFlash()
    {
        if(flash)flash.enabled=false;
    }
    void LateUpdate()
    {
        recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*9);
        transform.localPosition=rest+Vector3.back*recoil*.09f;
    }
}
