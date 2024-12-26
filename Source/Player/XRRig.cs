using CWVR.Input;
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
    private Spectate spectate;
    private float heightOffset;
    private float rotationOffset;

    private Vector2 originOffset = Vector2.zero;

    public Vector3 DesiredCameraPosition =>
        player.refs.cameraPos.TransformPoint(player.Ragdoll() || player.data.currentBed is not null ? Vector3.zero : new Vector3(0, 0.1f, -0.1f));
    
    private void Awake()
    {
        Camera = VRSession.Instance.MainCamera;
        
        // Set up camera hierarchy
        transform.SetParent(Camera.transform.parent, false);
        transform.localScale = Vector3.one * WORLD_SCALE;
        Camera.transform.SetParent(transform, false);

        // Hello player
        player = global::Player.localPlayer;
        spectate = Camera.GetComponent<Spectate>();
        
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
        
        Logger. LogDebug($"Changed height offset to {heightOffset}");
    }

    private void Update()
    {
        // Prevent clipping the camera through walls etc
        if (Physics.OverlapBoxNonAlloc(Camera.transform.position, Vector3.one * 0.1f, cameraClipCollider,
                Quaternion.identity, HelperFunctions.GetMask(HelperFunctions.LayerType.TerrainProp)) > 0)
        {
            originOffset = -Camera.transform.localPosition.XZ();
        }

        // Detect reset height
        if (Actions.Instance["ResetHeight"].WasPressedThisFrame())
            ResetHeight();
    }

    private void LateUpdate()
    {
        if (Spectate.spectating)
        {
            DoSpectate();
            return;
        }
        
        transform.position = DesiredCameraPosition;

        if (player.Ragdoll() || player.data.dead || player.data.currentBed is not null) {
            transform.position += (transform.position - Camera.transform.position) * WORLD_SCALE;
            rotationOffset = 0;
        }
        else
        {
            transform.position += new Vector3(originOffset.x, 0, originOffset.y);
            transform.position += GamefeelHandler.instance.GetPositionOffsets() +
                                  new Vector3(0, heightOffset, 0) * WORLD_SCALE;
        }
        
        // FUCK IT, WE ROLL!
        if ((player.Ragdoll() || player.HangingUpsideDown() || player.data.currentBed is not null) &&
            !Plugin.Config.DisableRagdollCamera.Value)
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

    private void DoSpectate()
    {
        if (global::Player.observedPlayer == null)
            return;

        transform.rotation = Quaternion.LookRotation(spectate.lookDirection);
        transform.position = global::Player.observedPlayer.TransformCenter() + Vector3.up * 0.75f +
                             new Vector3(originOffset.x, 0, originOffset.y);

        var vector = transform.position + transform.forward * -3f;
        var hit = HelperFunctions.LineCheck(transform.position, vector, HelperFunctions.LayerType.TerrainProp);

        if (hit.transform)
            transform.position += -transform.forward * (hit.distance - 0.2f);
        else
            transform.position = vector + transform.forward * 0.2f;

        var pivot = Actions.Instance["Pivot"].ReadValue<Vector2>() * 3;

        // Move origin back to "center" while camera is being pivotted
        if (pivot.x > 0 || pivot.y > 0)
        {
            var desiredPosition = -Camera.transform.localPosition.XZ();

            originOffset = Vector2.Lerp(originOffset, desiredPosition, 0.1f);
        }

        spectate.look.x += pivot.x;
        spectate.look.y += pivot.y;
        spectate.look.y = Mathf.Clamp(spectate.look.y, -80f, 80f);
        spectate.lookDirection = HelperFunctions.LookToDirection(spectate.look, Vector3.forward);

        transform.eulerAngles = new Vector3(0, rotationOffset, 0);

        if (Actions.Instance["Spectate Next"].WasPressedThisFrame())
            global::Player.observedPlayer = PlayerHandler.instance.GetNextPlayerAlive(global::Player.observedPlayer, 1);
        else if (Actions.Instance["Spectate Previous"].WasPressedThisFrame())
            global::Player.observedPlayer =
                PlayerHandler.instance.GetNextPlayerAlive(global::Player.observedPlayer, -1);
    }

    public void MoveOriginOffset(Vector2 movement)
    {
        originOffset += movement;
    }

    public void AddRotation(float rotation)
    {
        rotationOffset = (rotationOffset + rotation) % 360;
    }
}