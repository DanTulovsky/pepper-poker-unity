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

    // Start is called before the first frame update

    // Update is called once per frame

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
    private void Start()
    {
        game = Manager.Instance.game;
        Assert.IsNotNull(game);
        
        clientInfo = Manager.Instance.clientInfo;
        Assert.IsNotNull(clientInfo);
    }
}
