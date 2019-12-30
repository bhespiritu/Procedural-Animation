using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PoseManager : MonoBehaviour {

    public Pose[] animation = new Pose[4];
    public AnimCycle walk, run;
    public float runThreshold;
    public PlayerController pc;
    public Pose jumpPose;

    public bool isGrounded = false;

    public float jumpTransition = 0;

    public int poseIndex = 0;

    public float interpolationSpeed = 0.1f;

    //public GameObject hip, chest, neck, head, upperArmL, upperArmR, lowerArmL, lowerArmR, handL, handR, upperLegL, upperLegR, lowerLegL, lowerLegR, footL, footR;
    public GameObject[] bones = new GameObject[16];
    public Pose restPose;
    public GameObject baseTransform;
    public Pose currentPose;
    public bool isTransitioning;
    public float progress = 0;
    public float restThreshold;
	// Use this for initialization
	void Start () {
        SetPose(animation[0]);
        lastPose = animation[0];
        pc = GetComponent<PlayerController>();
        //Time.timeScale = 0.1f;
	}

    public void SetPose(Pose p)
    {
        for(int i = 0; i < 16; i++)
        {
            setToBone(bones[i].transform, p.bones[i]);
        }
        /*setToBone(hip.transform, p.hip);
        setToBone(chest.transform, p.chest);
        setToBone(neck.transform, p.neck);
        setToBone(head.transform, p.head);
        setToBone(upperArmL.transform, p.upperArmL);
        setToBone(upperArmR.transform, p.upperArmR);
        setToBone(lowerArmL.transform, p.lowerArmL);
        setToBone(lowerArmR.transform, p.lowerArmR);
        setToBone(handL.transform, p.handL);
        setToBone(handR.transform, p.handR);
        setToBone(upperLegL.transform, p.upperLegL);
        setToBone(upperLegR.transform, p.upperLegR);
        setToBone(lowerLegL.transform, p.lowerLegL);
        setToBone(lowerLegR.transform, p.lowerLegR);
        setToBone(footL.transform, p.footL);
        setToBone(footR.transform, p.footR);
        */
    }

    public void Mirror()
    {

            //Vector3 temp = transform.localScale;
            //temp.x *= -1;
            //transform.localScale = temp;
        
        Vector3[] targetPos = new Vector3[16];
        Quaternion[] targetRot = new Quaternion[16];
        for (int i = 0; i < bones.Length; i++)
        {
            GameObject bone = bones[i];
            Vector3 localPos = bone.transform.position - baseTransform.transform.position;
            


            Quaternion localRot = bone.transform.rotation;

            Debug.Log(localPos + " " + localRot);
            localPos.x *= -1;

            localRot.x *= -1;
            localRot.w *= -1;
            Debug.Log(localPos + " " + localRot);
            int other = i;
            if (i > 3)
            {
                if (i % 2 == 0)
                    other++;
                else
                {
                    other--;
                }
            }
            targetPos[other] = localPos;
            targetRot[other] = localRot;
        }
        for (int i = 0; i < bones.Length; i++)
        {
            GameObject bone = bones[i];
            bone.transform.position = baseTransform.transform.position + targetPos[i];
            bone.transform.rotation = targetRot[i];
        }
    }

    public void step()
    {
        Debug.Log("SWITCH!");
        currentPose = animation[poseIndex];
        poseIndex+=2;
        poseIndex %= animation.Length;
        //SetPose(animation[poseIndex]);
    }

    private void setToBone(Transform transform, Bone bone)
    {
        transform.position = baseTransform.transform.TransformPoint(bone.pos);
        transform.rotation = baseTransform.transform.rotation * bone.rot;
        transform.localScale = bone.scale;
   
    }

    Pose lastPose;

    // Update is called once per frame
    void FixedUpdate () {
        int temp = poseIndex+1;
        temp %= animation.Length;
        currentPose = restPose;


        Vector3 targetDirection;// = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")); // change to rb velocity
        targetDirection = GetComponent<Rigidbody>().velocity;
        isGrounded = pc.IsGrounded();
        if (isGrounded)
        {
            if (Mathf.Abs(targetDirection.x) < restThreshold && Mathf.Abs(targetDirection.z) < restThreshold) //SetPose(restPose);
            {
                Pose nextPose = Pose.lerp(lastPose, restPose, interpolationSpeed);
                lastPose = nextPose;
                SetPose(nextPose);
            }
            else
            {
                float t = targetDirection.magnitude / (pc.speed); // look into compute shaders
                float transition = Mathf.Clamp01((t - .2f) / runThreshold);
                //AnimCycle sel = (t <= runThreshold) ? walk : run;
                Pose runP = Pose.splineInterp(run[temp], run[(temp + 1) % animation.Length], run[(temp + 2) % animation.Length], progress);
                Pose walkP = Pose.splineInterp(walk[temp], walk[(temp + 1) % animation.Length], walk[(temp + 2) % animation.Length], progress);
                Pose lerpP = Pose.lerp(walkP, runP, transition);
                SetPose(lerpP);
                lastPose = lerpP;
            }
        } else
        {
            jumpTransition += Time.fixedDeltaTime*10;
            jumpTransition = Mathf.Clamp01(jumpTransition);
            Pose jumpLerp = Pose.lerp(lastPose, jumpPose, jumpTransition);
            SetPose(jumpLerp);
        } 


    }

    

}

[System.Serializable]
[CreateAssetMenu(fileName = "Animation", menuName = "Anim/Animation Cycle", order = 2)]
public class AnimCycle : ScriptableObject
{
    public List<Pose> animation = new List<Pose>();
    public Pose this[int param]
    {
        get { return animation[param]; }
        set { animation[param] = value; }
    }
}

    [System.Serializable]
[CreateAssetMenu(fileName = "NewPose", menuName = "Anim/Pose", order = 1)]
public class Pose : ScriptableObject
{
    //public Bone hip, chest, neck, head, upperArmL, upperArmR, lowerArmL, lowerArmR, handL, handR, upperLegL, upperLegR, lowerLegL, lowerLegR, footL, footR;
    public Bone[] bones = new Bone[16];
    public void writePose(PoseManager pm) // change it so that it is all relative to the base
    {
        for(int i = 0; i < 16; i++)
        {
            bones[i] = Bone.TransformToBone(pm.bones[i].transform, pm.baseTransform);
        }
        /*hip = Bone.TransformToBone(pm.hip.transform, pm.baseTransform);
        chest = Bone.TransformToBone(pm.chest.transform, pm.baseTransform);
        neck = Bone.TransformToBone(pm.neck.transform, pm.baseTransform);
        head = Bone.TransformToBone(pm.head.transform, pm.baseTransform);
        upperArmL = Bone.TransformToBone(pm.upperArmL.transform, pm.baseTransform);
        upperArmR = Bone.TransformToBone(pm.upperArmR.transform, pm.baseTransform);
        lowerArmL = Bone.TransformToBone(pm.lowerArmL.transform, pm.baseTransform);
        lowerArmR = Bone.TransformToBone(pm.lowerArmR.transform, pm.baseTransform);
        handL = Bone.TransformToBone(pm.handL.transform, pm.baseTransform);
        handR = Bone.TransformToBone(pm.handR.transform, pm.baseTransform);
        upperLegL = Bone.TransformToBone(pm.upperLegL.transform, pm.baseTransform);
        upperLegR = Bone.TransformToBone(pm.upperLegR.transform, pm.baseTransform);
        lowerLegL = Bone.TransformToBone(pm.lowerLegL.transform, pm.baseTransform);
        lowerLegR = Bone.TransformToBone(pm.lowerLegR.transform, pm.baseTransform);
        footL = Bone.TransformToBone(pm.footL.transform, pm.baseTransform);
        footR = Bone.TransformToBone(pm.footR.transform, pm.baseTransform);
        */
    }
    public static Pose lerp(Pose a, Pose b, float t)
    {
        Pose c = new Pose();
        for(int i = 0; i < 16; i++)
        {
            c.bones[i] = Bone.lerp(a.bones[i], b.bones[i], t);
        }
        return c;
    }

    public static Pose splineInterp(Pose a, Pose b, Pose c, float t)
    {
        Pose d = new Pose();
        for (int i = 0; i < 16; i++)
        {
            d.bones[i] = Bone.splineLerp(a.bones[i], b.bones[i], c.bones[i], t);
        }
        return d;
    }

}
[System.Serializable]
public struct Bone
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;

    public Bone(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        this.pos = pos;
        this.rot = rot;
        this.scale = scale;
    }

    public static Bone lerp(Bone a, Bone b, float t)
    {
        return new Bone(Vector3.Lerp(a.pos, b.pos, t), Quaternion.Slerp(a.rot, b.rot, t), Vector3.Lerp(a.scale, b.scale, t));
    }

    public static Bone splineLerp(Bone a, Bone b, Bone c, float t)
    {
        return new Bone(splineLerp(a.pos, b.pos, c.pos, t), splineLerp(a.rot, b.rot, c.rot,t), splineLerp(a.scale,b.scale,c.scale,t));
    }

    public static Vector3 splineLerp(Vector3 a, Vector3 b, Vector3 c, float t)
    {
    
        return Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);
    }

    public static Quaternion splineLerp(Quaternion a, Quaternion b, Quaternion c, float t)
    {
        return Quaternion.Lerp(Quaternion.Lerp(a, b, t), Quaternion.Lerp(b, c, t), t);
    }

    public static Bone TransformToBone(Transform t, GameObject center)
    {
        return new Bone(
            center.transform.InverseTransformPoint(t.position)
        
            , Quaternion.Inverse(center.transform.rotation) * t.rotation, t.localScale);//see if localScale is right
    }
}

public enum HumanoidSkeleton : int
{
    hip = 0, chest, neck, head, upperArmL, upperArmR, lowerArmL, lowerArmR, handL, handR, upperLegL, upperLegR, lowerLegL, lowerLegR, footL, footR
}
