using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Balloon : MonoBehaviour
{
    private static UnityEvent<BalloonChainInfoStruct> onBalloonPopped;

    public static BalloonEventListener AddListener(UnityAction<BalloonChainInfoStruct> listener)
    {
        BalloonEventListener newListener = new BalloonEventListener(onBalloonPopped, listener);  
        return newListener;
    } 
}
