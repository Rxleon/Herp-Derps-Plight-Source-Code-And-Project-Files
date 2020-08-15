﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class VisualEvent
{
    public CoroutineData cData;
    public Action eventFunction;

    public bool isPlaying;
    public float startDelay;
    public float endDelay;

    public VisualEvent(Action _eventFunction, CoroutineData _cData, float _startDelay, float _endDelay)
    {
        eventFunction = _eventFunction;
        cData = _cData;
        startDelay = _startDelay;
        endDelay = _endDelay;
    }


}
