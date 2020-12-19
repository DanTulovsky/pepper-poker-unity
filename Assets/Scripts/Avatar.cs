using System;
using Poker;
using UnityEngine;
using UnityEngine.Assertions;

public class Avatar : MonoBehaviour
{
    public Animator anim;
    
    private Game game;
    private ClientInfo clientInfo;

    private static readonly int ShakeHead = Animator.StringToHash("HeadShake");
    private static readonly int Defeated = Animator.StringToHash("Defeated");
    private static readonly int Victory = Animator.StringToHash("Victory");

    public void AnimateShrug(string msg)
    {
        anim.SetTrigger(ShakeHead);
        Debug.Log(msg);
    }

    public void AnimateDefeat(string msg)
    {
        anim.SetTrigger(Defeated);
        Debug.Log(msg);
    }
    
    public void AnimateVictory()
    {
        anim.SetTrigger(Victory );
    }
    
    private void Start()
    {
        game = Manager.Instance.game;
        Assert.IsNotNull(game);
        
        clientInfo = Manager.Instance.clientInfo;
        Assert.IsNotNull(clientInfo);
    }
}
