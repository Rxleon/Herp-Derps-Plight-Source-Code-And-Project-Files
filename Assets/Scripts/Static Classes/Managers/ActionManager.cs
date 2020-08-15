﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public List<OldCoroutineData> actionQueue;

    // Singleton Set up
    #region
    public static ActionManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            actionQueue = new List<OldCoroutineData>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    // Modify Queue + Check queue status
    #region
    public void AddActionToQueue(OldCoroutineData action)
    {
        actionQueue.Add(action);
        Debug.Log("Adding action to queue. Current queue count = " + actionQueue.Count.ToString());
    }
    public void RemoveActionFromQueue(OldCoroutineData action)
    {
        if (actionQueue.Contains(action))
        {
            actionQueue.Remove(action);
            Debug.Log("Removing action from queue. Current queue count = " + actionQueue.Count.ToString());
        }
    }
    public bool UnresolvedCombatActions()
    {
        Debug.Log("ActionManager.UnresolvedCombatActions() called, checking for unresolved combat actions...");
        bool boolReturned = false;

        foreach(OldCoroutineData action in actionQueue)
        {
            if(action.coroutineCompleted == false && action.combatAction)
            {
                boolReturned = true;
                break;
            }
        }

        return boolReturned;
    }
    public void FlushActionQueue()
    {
        Debug.Log("ActionManager.ForceResolveAllQueueActions() called...");

        ForceResolveAllQueueActions();
        actionQueue.Clear();
    }
    public void ForceResolveAllQueueActions()
    {
        Debug.Log("ActionManager.ForceResolveAllQueueActions() called...");

        foreach(OldCoroutineData action in actionQueue)
        {
            action.coroutineCompleted = true;
        }
    }
    #endregion
}
