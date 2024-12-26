using CWVR.Assets;
using UnityEngine;

namespace CWVR.Player;

public class Interactor : MonoBehaviour
{
    private const float INTERACTOR_LENGTH = 3f;

    private LineRenderer debugLineRenderer;
    
    private void Awake()
    {
        debugLineRenderer = gameObject.AddComponent<LineRenderer>();
        debugLineRenderer.widthCurve.keys = [new Keyframe(0, 1)];
        debugLineRenderer.widthMultiplier = 0.005f;
        debugLineRenderer.positionCount = 2;
        debugLineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
        debugLineRenderer.numCornerVertices = 4;
        debugLineRenderer.numCapVertices = 4;
        debugLineRenderer.alignment = LineAlignment.View;
        debugLineRenderer.shadowBias = 0.5f;
        debugLineRenderer.useWorldSpace = true;
        debugLineRenderer.maskInteraction = SpriteMaskInteraction.None;
        debugLineRenderer.SetMaterials([AssetManager.WhiteMat]);
        debugLineRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        var origin = transform.position + transform.up * 0.1f;
        var end = transform.position + transform.up * INTERACTOR_LENGTH;

        debugLineRenderer.SetPositions(new[] { origin, end });
    }

    public RaycastHit[] GetRaycastHits()
    {
        return HelperFunctions.LineCheckAll(transform.position, transform.position + transform.up * INTERACTOR_LENGTH,
            HelperFunctions.LayerType.All);
    }
}