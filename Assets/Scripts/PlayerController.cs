using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    //TODO: Need to work on wierd snapping
	public float maxTiltAngle = 45f;
	public float maxVelocityTilt;
	public float tiltSensitivity = .01f;
	public GameObject com;
    public float speed = 3;
    public float maxVelocityChange = 10;
    public Pose restPose;
    public float changeRate;


    //center of mass, also acts as the base for everything else


    private Rigidbody rb;
    private Vector3 targetRot;
	public Vector3 lastVelocity;
    private Vector3 targetForward;

	// Use this for initialization
	void Start ()
	{
		rb = GetComponent<Rigidbody> ();
        distToGround = GetComponent<SphereCollider>().bounds.extents.y;
    }
	
	// Update is called once per frame
	void Update ()
	{
        


    }

    float distToGround;


    public bool IsGrounded() {
   return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
 }



void FixedUpdate ()
	{


        Vector3 velocity = rb.velocity;
        rb.velocity = Vector3.ClampMagnitude(velocity, speed);
        Vector3 targetDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        targetDirection = Camera.main.transform.TransformDirection(targetDirection);
        targetDirection.y = 0.0f;
        targetDirection *= speed;

        // Apply a force that attempts to reach our target velocity
        var velocityChange = (targetDirection- velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange)*changeRate;
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange)*changeRate;

       

        velocityChange.y = 0;
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            velocityChange.y = 10;
            GetComponent<PoseManager>().jumpTransition = 0;
        }

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        
        //rb.velocity = new Vector3(Mathf.Sin(Time.time), velocity.y, Mathf.Cos(Time.time));
        Debug.DrawRay(transform.position, rb.velocity,Color.black);
        velocity = rb.velocity;
        

        Vector3 a = (rb.velocity - lastVelocity) / Time.fixedDeltaTime;
        Debug.DrawRay(transform.position, a, Color.yellow);
        lastVelocity = targetForward;

		Vector3 direction = new Vector3 (a.x, 0, a.z);
        Vector3 axis = Quaternion.AngleAxis(90, Vector3.up) * direction;
        axis = axis.normalized;
        //direction = com.transform.InverseTransformDirection(direction);
        //axis = com.transform.InverseTransformDirection(axis);

        targetRot = Vector3.up;

        if (Mathf.Abs(velocity.x) > 0.1f || Mathf.Abs(velocity.z) > 0.1f || true)
        {
            Vector3 vel = new Vector3(velocity.x, 0, velocity.z);
            targetForward = vel;
            //lastVelocity = vel;
        }  else
        {
            targetForward = lastVelocity;
            
                //GetComponent<PoseManager>().SetPose(restPose);
                GetComponentInChildren<SurveyorWheel>().progress = 0;
            
        }

        if (Mathf.Abs(direction.x) > 0.2f || Mathf.Abs(direction.z) > 0.2f)
        {
            targetRot = Quaternion.AngleAxis(maxTiltAngle, axis) * targetRot;
        }
 
        Debug.DrawRay(transform.position, targetRot, Color.blue);
        Debug.DrawRay(transform.position, axis, Color.cyan);
        //com.transform.rotation = Quaternion.Slerp(com.transform.rotation, Quaternion.LookRotation(targetForward)*new Quaternion(axis.x, axis.y, axis.z, maxTiltAngle*10), Time.fixedDeltaTime);
        if(targetForward != Vector3.zero)
            com.transform.rotation = Quaternion.Slerp(com.transform.rotation, Quaternion.LookRotation(targetForward), Time.fixedDeltaTime*5);
        if(axis != Vector3.zero)
            com.transform.rotation = Quaternion.Slerp(com.transform.rotation, Quaternion.AngleAxis(maxTiltAngle, axis), Time.fixedDeltaTime);
        else
        {
            Vector3 flatForward = com.transform.forward;
            flatForward.y = 0;
            com.transform.rotation = Quaternion.Slerp(com.transform.rotation, Quaternion.LookRotation(flatForward,Vector3.up), Time.fixedDeltaTime * 5);

            
        }

    }
}