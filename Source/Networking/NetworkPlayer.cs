using UnityEngine;
using Zorro.Core.Serizalization;

namespace CWVR.Networking;

public class NetworkPlayer : MonoBehaviour
{
    private global::Player player;
    private IKRigHandler rigHandler;

    private Vector3 leftHandWorldPos;
    private Vector3 leftHandRot;

    private Vector3 rightHandWorldPos;
    private Vector3 rightHandRot;
    
    private void Awake()
    {
        player = GetComponent<global::Player>();
        rigHandler = GetComponentInChildren<IKRigHandler>();
        
        player.refs.IK_Left.weight = 1;
        player.refs.IK_Right.weight = 1;
    }

    private void LateUpdate()
    {
        UpdateIK();
    }

    private void UpdateIK()
    {
        if (player.data.dead || player.data.currentBed is not null)
            return;
        
        rigHandler.SetLeftHandPosition(leftHandWorldPos, Quaternion.Euler(leftHandRot));
        rigHandler.SetRightHandPosition(rightHandWorldPos, Quaternion.Euler(rightHandRot));
        
        // Force hand rotations
        var leftHand = player.refs.ragdoll.GetBodypart(BodypartType.Hand_L);
        var rightHand = player.refs.ragdoll.GetBodypart(BodypartType.Hand_R);
        
        leftHand.rig.transform.rotation = leftHand.animationTarget.transform.rotation;
        rightHand.rig.transform.rotation = rightHand.animationTarget.transform.rotation;
    }

    public void DeserializeRig(BinaryDeserializer deserializer)
    {
        leftHandWorldPos = deserializer.ReadFloat3();
        leftHandRot = deserializer.ReadFloat3();

        rightHandWorldPos = deserializer.ReadFloat3();
        rightHandRot = deserializer.ReadFloat3();
    }
}