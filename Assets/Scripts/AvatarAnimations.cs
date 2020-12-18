using UnityEngine;

public class AvatarAnimations : MonoBehaviour
{
    public Animator anim;

    private static readonly int Active = Animator.StringToHash("Active");

    // Start is called before the first frame update

    // Update is called once per frame

    public void AnimateShrug()
    {
        anim.SetTrigger(Active);
    }
}
