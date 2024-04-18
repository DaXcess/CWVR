using UnityEngine;
using UnityEngine.SpatialTracking;

namespace CWVR.Player;

public class XRRig : MonoBehaviour
{
    private const float WORLD_SCALE = 1.25f;

    private readonly Collider[] cameraClipCollider = new Collider[1];

    public Camera Camera { get; private set; }

    // Controllers
    public Transform LeftController { get; private set; }
    public Transform RightController { get; private set; }

    // IK targets
    public Transform LeftHand { get; private set; }
    public Transform RightHand { get; private set; }

    // Trackers
    private TrackedPoseDriver cameraTracker;
    private TrackedPoseDriver leftHandTracker;
    private TrackedPoseDriver rightHandTracker;

    private global::Player player;
    private float heightOffset;
    private float rotationOffset;

    public Vector3 DesiredCameraPosition =>
        player.refs.cameraPos.TransformPoint((player.Ragdoll() || player.data.bed is not null) ? Vector3.zero : new Vector3(0, 0.1f, -0.1f));

    public Vector2 OriginOffset { get; private set; } = Vector2.zero;

    private void Awake()
    {
        Camera = VRSession.Instance.MainCamera;
        transform.SetParent(Camera.transform.parent, false);
        transform.localScale = Vector3.one * WORLD_SCALE;

        Camera.transform.SetParent(transform, false);

        // Hello player
        player = global::Player.localPlayer;

        // Create HMD tracker
        cameraTracker = Camera.gameObject.AddComponent<TrackedPoseDriver>();
        cameraTracker.deviceType = TrackedPoseDriver.DeviceType.GenericXRDevice;
        cameraTracker.poseSource = TrackedPoseDriver.TrackedPose.Center;
        cameraTracker.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;

        // Create controllers
        LeftController = new GameObject("Left VR Controller").transform;
        RightController = new GameObject("Right VR Controller").transform;

        LeftController.transform.SetParent(transform);
        RightController.transform.SetParent(transform);

        leftHandTracker = LeftController.gameObject.AddComponent<TrackedPoseDriver>();
        leftHandTracker.deviceType = TrackedPoseDriver.DeviceType.GenericXRController;
        leftHandTracker.poseSource = TrackedPoseDriver.TrackedPose.LeftPose;
        leftHandTracker.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
        leftHandTracker.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

        rightHandTracker = RightController.gameObject.AddComponent<TrackedPoseDriver>();
        rightHandTracker.deviceType = TrackedPoseDriver.DeviceType.GenericXRController;
        rightHandTracker.poseSource = TrackedPoseDriver.TrackedPose.RightPose;
        rightHandTracker.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
        rightHandTracker.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

        // Create controller IK targets
        LeftHand = new GameObject("IK Target").transform;
        RightHand = new GameObject("IK Target").transform;

        LeftHand.SetParent(LeftController, false);
        RightHand.SetParent(RightController, false);

        // Left hand offsets
        LeftHand.localPosition = Vector3.zero;
        LeftHand.localEulerAngles = new Vector3(345, 90, 145);
        LeftHand.localScale = Vector3.one;

        // Right hand offsets
        RightHand.localPosition = Vector3.zero;
        RightHand.localEulerAngles = new Vector3(345, 270, 215);
        RightHand.localScale = Vector3.one;

        ResetHeight();
    }

    /// <summary>
    /// Resets the height so that the camera is flush with the XR Origin, unless a custom offset value is provided
    /// </summary>
    public void ResetHeight(float? offset = null)
    {
        if (offset is { } off)
            heightOffset = off;
        else
            heightOffset = -cameraTracker.GetPoseData().position.y;
    }
    
    private void Update()
    {
        // Prevent clipping the camera through walls etc
        if (Physics.OverlapBoxNonAlloc(Camera.transform.position, Vector3.one * 0.1f, cameraClipCollider,
                Quaternion.identity, 1 << 9) > 0)
        {
            OriginOffset = -Camera.transform.localPosition.XZ();
        }
    }

    private void LateUpdate()
    {
        transform.position = DesiredCameraPosition;

        if (player.Ragdoll() || player.data.dead || player.data.bed is not null) {
            transform.position += (transform.position - Camera.transform.position) * WORLD_SCALE;
            rotationOffset = 0;
        }
        else
        {
            transform.position += new Vector3(OriginOffset.x, 0, OriginOffset.y);
            transform.position += GamefeelHandler.instance.GetPositionOffsets() +
                                  new Vector3(0, heightOffset, 0) * WORLD_SCALE;
        }
        
        // FUCK IT, WE ROLL!
        if ((player.Ragdoll() || player.HangingUpsideDown() || player.data.bed is not null) && !Plugin.Config.DisableRagdollCamera.Value)
        {
            var rotation = player.refs.ragdoll.GetBodypart(BodypartType.Head).rig.transform.rotation;

            transform.rotation = Quaternion.LookRotation(player.data.lookDirection);
            if (player.data.CameraPhysicsAmount() > 0.01f)
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, player.data.CameraPhysicsAmount());

            transform.localEulerAngles +=
                GamefeelHandler.instance.GetEulerOffsets() + new Vector3(0, rotationOffset, 0);
        }
        else
            transform.localEulerAngles = new Vector3(0, rotationOffset, 0);
    }

    public void MoveOriginOffset(Vector2 movement)
    {
        OriginOffset += movement;
    }

    public void RotateOffset(float rotation)
    {
        rotationOffset = (rotationOffset + rotation) % 360;
    }
}