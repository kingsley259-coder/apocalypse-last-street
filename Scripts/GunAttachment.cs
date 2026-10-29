using UnityEngine;
public class GunAttachment : MonoBehaviour
{
    public Animator animator;
    public Transform facing;
    void LateUpdate()
    {
        if(!animator||!facing)return;
        var hand=animator.GetBoneTransform(HumanBodyBones.RightHand);
        if(!hand)return;
        transform.position=hand.position+facing.forward*.12f;
        transform.rotation=facing.rotation;
    }
}
