using UnityEngine;
public class GameAudio : MonoBehaviour
{
    public static float Effects=>PlayerPrefs.GetFloat("AP.FX",.65f);
    public AudioSource music;
    void Update()
    {
        AudioListener.volume=PlayerPrefs.GetFloat("AP.Master",.8f);
        if(music)music.volume=PlayerPrefs.GetFloat("AP.Music",.4f)*.6f;
    }
    public static void Set(string key,float value)
    {
        PlayerPrefs.SetFloat("AP."+key,Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }
}
