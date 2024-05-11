using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using CWVR.Input;
using CWVR.Player;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

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

    public static string PascalToLongString(string text)
    {
        var builder = new StringBuilder(text[0].ToString());
        if (builder.Length <= 0) return builder.ToString();

        for (var index = 1; index < text.Length; index++)
        {
            var prevChar = text[index - 1];
            var nextChar = index + 1 < text.Length ? text[index + 1] : '\0';

            var isNextLower = char.IsLower(nextChar);
            var isNextUpper = char.IsUpper(nextChar);
            var isPresentUpper = char.IsUpper(text[index]);
            var isPrevLower = char.IsLower(prevChar);
            var isPrevUpper = char.IsUpper(prevChar);

            if (!string.IsNullOrWhiteSpace(prevChar.ToString()) &&
                ((isPrevUpper && isPresentUpper && isNextLower) ||
                 (isPrevLower && isPresentUpper && isNextLower) ||
                 (isPrevLower && isPresentUpper && isNextUpper)))
            {
                builder.Append(' ');
                builder.Append(text[index]);
            }
            else
            {
                builder.Append(text[index]);
            }
        }

        return builder.ToString();
    }

    public static bool InVR(this global::Player player)
    {
        if (player.IsLocal)
            return VRSession.InVR;

        return VRSession.Instance && VRSession.Instance.NetworkManager.InVR(player);
    }
    
    public static InputDevice GetDevice(this InputSystem.XRController controller)
    {
        return controller switch
        {
            InputSystem.XRController.Left => InputDevices.GetDeviceAtXRNode(XRNode.LeftHand),
            InputSystem.XRController.Right => InputDevices.GetDeviceAtXRNode(XRNode.RightHand),
            _ => throw new ArgumentOutOfRangeException(nameof(controller), controller, null)
        };
    }
}

internal static class JSON
{
    public static T Deserialize<T>(string json)
    {
        using var mem = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var serializer = new DataContractJsonSerializer(typeof(T));

        return (T)serializer.ReadObject(mem);
    }
}