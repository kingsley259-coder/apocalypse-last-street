using UnityEngine;
public class RunnerFeedback : MonoBehaviour
{
    public ParticleSystem footsteps;
    PumpkinPlayer player;
    void Awake()
    {
        player=GetComponent<PumpkinPlayer>();
    }
    void Update()
    {
        if(!footsteps||!player)return;
        var emission=footsteps.emission;
        emission.rateOverTime=PumpkinGame.Instance&&PumpkinGame.Instance.IsPlaying?(player.IsSprinting&&player.MoveSpeed>5?24:player.MoveSpeed>1?4:0):0;
    }
}
