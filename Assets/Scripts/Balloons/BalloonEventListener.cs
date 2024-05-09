using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Unbind events from a reference to the static balloon event 
/// </summary>
public class BalloonEventListener
{
    private UnityEvent<BalloonChainInfoStruct> balloonEvent;
    private UnityAction<BalloonChainInfoStruct> listener;

    public BalloonEventListener(UnityEvent<BalloonChainInfoStruct> balloonEvent, UnityAction<BalloonChainInfoStruct> listener)
    {
        this.balloonEvent = balloonEvent;
        this.listener = listener;
    }

    ~BalloonEventListener()
    {
        balloonEvent.RemoveListener(listener);
    }
}
