﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using DG.Tweening;
public class CameraManager : Singleton<CameraManager>
{
    // Properties + Component References
    #region
    [Header("Component References")]
    [SerializeField] private Camera mainCamera;

    [Header("Small Shake Properties")]
    public float sMagnitude;
    public float sRoughness;
    public float sDuration;

    [Header("Medium Shake Properties")]
    public float mMagnitude;
    public float mRoughness;
    public float mDuration;

    [Header("Large Shake Properties")]
    public float lMagnitude;
    public float lRoughness;
    public float lDuration;
    #endregion
       
    // Property Accessors
    #region
    public Camera MainCamera
    {
        get { return mainCamera; }
        private set { mainCamera = value; }
    }
    #endregion

    // Camera Shake Logic
    #region
    public void CreateCameraShake(CameraShakeType shakeType)
    {
        if(shakeType == CameraShakeType.Small)
        {
            CreateSmallCameraShake();
        }
        else if (shakeType == CameraShakeType.Medium)
        {
            CreateMediumCameraShake();
        }
        else if (shakeType == CameraShakeType.Large)
        {
            CreateLargeCameraShake();
        }
    }
    private void CreateSmallCameraShake()
    {
        CameraShaker.Instance.ShakeOnce(sMagnitude, sRoughness, 0.1f, sDuration);
    }
    private void CreateMediumCameraShake()
    {
        CameraShaker.Instance.ShakeOnce(mMagnitude, mRoughness, 0.1f, mDuration);
    }
    private void CreateLargeCameraShake()
    {
        CameraShaker.Instance.ShakeOnce(lMagnitude, lRoughness, 0.1f, lDuration);
    }
    #endregion

    // Camera Zoom
    #region
    public void DoCameraZoomOut(float startOrthoSize, float endOrthoSize, float duration)
    {
        MainCamera.orthographicSize = startOrthoSize;
        MainCamera.DOOrthoSize(endOrthoSize, duration);

    }
    #endregion
}
