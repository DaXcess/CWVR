using System;
using CWVR.Input;
using CWVR.MultiLoader.Common;
using UnityEngine;

namespace CWVR.Player;

public class VRPlayer : MonoBehaviour
{
    private global::Player player;
    private IKRigHandler rigHandler;
    
    public XRRig Rig { get; private set; }
    
    public Interactor PrimaryInteractor { get; private set; }
    
    private Vector3 Position => player.refs.cameraPos.position;
    
    private Vector2 prevPlayerPosition;

    /// <summary>
    /// Used to keep track of turning when using snap turn
    /// </summary>
    private bool turnedLastInput;

    private void Awake()
    {
        player = GetComponent<global::Player>();
        rigHandler = transform.Find("RigCreator").GetComponent<IKRigHandler>();

        Rig = new GameObject("XR Rig").AddComponent<XRRig>();

        // Create interactor
        var leftHand = player.refs.rigRoot.transform.Find("Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L");
        var rightHand = player.refs.rigRoot.transform.Find("Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R");

        PrimaryInteractor = rightHand.gameObject.AddComponent<Interactor>();

        // Set up position state
        prevPlayerPosition = Position.XZ();
    }

    private void Update()
    {
        UpdatePlayerToCamera();
        UpdateTurning();
    }

    private void FixedUpdate()
    {
        // Set animation rig weights
        var weight = player.data.dead ? 0 : 1;
        
        player.refs.IK_Left.weight = weight;
        player.refs.IK_Right.weight = weight;
    }

    private void LateUpdate()
    {
        UpdateIK();
        PushPlayerToCamera();
    }

    /// <summary>
    /// If the player is moving towards the camera (HMD) when not centered, move the origin back to re-align
    /// </summary>
    private void UpdatePlayerToCamera()
    {
        var movement = Position.XZ() - prevPlayerPosition;
        var cameraMovement = Vector2.zero;
        
        var desiredCameraPosition = Rig.DesiredCameraPosition.XZ();

        if (Vector2.Distance(desiredCameraPosition + new Vector2(movement.x, 0), Rig.Camera.transform.position.XZ()) <
            Vector2.Distance(desiredCameraPosition, Rig.Camera.transform.position.XZ()))
            cameraMovement += new Vector2(-movement.x, 0);

        if (Vector2.Distance(desiredCameraPosition + new Vector2(0, movement.y), Rig.Camera.transform.position.XZ()) <
            Vector2.Distance(desiredCameraPosition, Rig.Camera.transform.position.XZ()))
            cameraMovement += new Vector2(0, -movement.y);

        prevPlayerPosition = Position.XZ();

        Rig.MoveOriginOffset(cameraMovement);
    }

    /// <summary>
    /// Handle smooth/snap turning
    /// </summary>
    private void UpdateTurning()
    {
        // Don't allow rotating while in ragdoll, but do allow when spectating
        if (player.Ragdoll() && !Spectate.spectating)
            return;

        if (!GlobalInputHandler.CanTakeInput())
            return;
        
        switch (Plugin.Config.TurnProvider.Value)
        {
            case IConfig.TurnProviderOption.Snap:
                var value = Actions.Instance["Turn"].ReadValue<float>();
                var should = MathF.Abs(value) > 0.75;

                if (!turnedLastInput && should)
                    if (value > 0)
                        Rig.AddRotation(Plugin.Config.SnapTurnSize.Value);
                    else
                        Rig.AddRotation(-Plugin.Config.SnapTurnSize.Value);

                turnedLastInput = should;

                break;

            case IConfig.TurnProviderOption.Smooth:
                Rig.AddRotation(180 * Time.deltaTime * Plugin.Config.SmoothTurnSpeedModifier.Value *
                                Actions.Instance["Turn"].ReadValue<float>());
                break;

            case IConfig.TurnProviderOption.Disabled:
            default:
                break;
        }
    }

    /// <summary>
    /// Set hand positions
    /// </summary>
    private void UpdateIK()
    {
        if (player.data.dead || player.data.currentBed is not null)
            return;
        
        rigHandler.SetLeftHandPosition(Rig.LeftHand.position, Rig.LeftHand.rotation);
        rigHandler.SetRightHandPosition(Rig.RightHand.position, Rig.RightHand.rotation);

        // Force hand rotations
        var leftHand = player.refs.ragdoll.GetBodypart(BodypartType.Hand_L);
        var rightHand = player.refs.ragdoll.GetBodypart(BodypartType.Hand_R);
        
        leftHand.rig.transform.rotation = leftHand.animationTarget.transform.rotation;
        rightHand.rig.transform.rotation = rightHand.animationTarget.transform.rotation;
    }

    /// <summary>
    /// If the camera (HMD) is too far away from the player position, apply a force to push the player towards the camera
    /// </summary>
    private void PushPlayerToCamera()
    {
        // Don't force push the player when they're dead or eeping
        if (player.data.dead || player.data.currentBed is not null)
            return;

        // Push player towards camera if it's too far away
        var vector = Rig.Camera.transform.position.XZ() - Rig.DesiredCameraPosition.XZ();

        // Slowly return camera back to player when moving using controllers
        if (player.input.movementInput.sqrMagnitude > 0)
        {
            Rig.MoveOriginOffset(-Vector2.MoveTowards(Vector2.zero, vector, Time.deltaTime));
            return;
        }
        
        if (vector.sqrMagnitude < 0.5f && !player.Ragdoll())
        {
            player.refs.ragdoll.AddForce(
                new Vector3(vector.x, 0, vector.y) * (player.refs.controller.movementForce * 8),
                ForceMode.Acceleration);
        }
        else
            // Camera is too far away, or player is in ragdoll: just teleport the camera
            Rig.MoveOriginOffset(-vector);
    }
}