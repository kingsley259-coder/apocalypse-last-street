using UnityEngine;
using UnityEngine.AI;
// Humanoid clips supplied by Mixamo; root motion remains controlled by gameplay.
public class HumanoidMotion : MonoBehaviour
{
    public Animator animator;
    public bool enemy;
    Vector3 previous;
    float fireUntil;
    void OnEnable()
    {
        previous=transform.position;
        if(animator)animator.applyRootMotion=false;
    }
    void Update()
    {
        if(!animator||!animator.runtimeAnimatorController)return;
        var game=PumpkinGame.Instance;
        bool playing=game&&game.IsPlaying;
        float speed=playing?(transform.position-previous).magnitude/Mathf.Max(Time.deltaTime,.0001f):0;
        previous=transform.position;
        animator.SetFloat("Speed",speed,.12f,Time.deltaTime);
        if(enemy)
        {
            var zombie=GetComponent<ZombieEnemy>();
            animator.SetBool("Dead",zombie&&zombie.dead);
            animator.SetBool("Attack",playing&&Vector3.Distance(transform.position,game.player.transform.position)<1.9f);
        }
        else animator.SetBool("Attack",Time.time<fireUntil);
    }
    public void Fire()
    {
        fireUntil=Time.time+.18f;
    }
}
