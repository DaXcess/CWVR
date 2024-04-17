using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SpatialTracking;

namespace CWVR;

internal static class Utils
{
    public static byte[] ComputeHash(byte[] input)
    {
        using var sha = SHA256.Create();

        return sha.ComputeHash(input);
    }

    public static Pose GetPoseData(this TrackedPoseDriver driver)
    {
        driver.GetPoseData(driver.m_Device, driver.m_PoseSource, out var pose);

        return pose;
    }

    public static void SetLayerRecursive(this GameObject go, int layer)
    {
        go.layer = layer;
        
        foreach (Transform child in go.transform)
        {
            child.gameObject.layer = layer;
            
            if (child.GetComponentInChildren<Transform>() is not null)
                child.gameObject.SetLayerRecursive(layer);
        }
    }
}

internal class LerpFloat(float value = 0)
{
    public float Value { get; private set; } = value;
    public float Target { get; set; } = value;
    public float Factor { get; set; } = 0.1f;
    
    public void Update()
    {
        Value = Mathf.Lerp(Value, Target, Factor);
    }
}