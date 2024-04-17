using UnityEngine;

namespace CWVR.Assets;

internal static class AssetManager
{
    private static AssetBundle assetBundle;

    public static GameObject cube;
    public static GameObject leftHandModel;
    public static GameObject rightHandModel;

    public static Material whiteMat;

    public static bool LoadAssets()
    {
        assetBundle = AssetBundle.LoadFromFile(@"C:\Users\DaXcess\CWVR-Unity\AssetBundles\StandaloneWindows\contentwarningvr");
        
        if (assetBundle == null)
        {
            Logger.LogError("Failed to load asset bundle!");
            return false;
        }

        cube = assetBundle.LoadAsset<GameObject>("Cube");
        leftHandModel = assetBundle.LoadAsset<GameObject>("Left Hand Model");
        rightHandModel = assetBundle.LoadAsset<GameObject>("Right Hand Model");
        
        whiteMat = assetBundle.LoadAsset<Material>("White");
        
        return true;
    }
}