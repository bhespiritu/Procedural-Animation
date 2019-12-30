using UnityEngine;
using System.Collections;

public class SurveyorWheel : MonoBehaviour {

    public Rigidbody rb;
    public const float TAU = 2 * Mathf.PI;
    public float radius = 3;
    public float height = 1;
    public float progress = 0;
    public int lastStage = 0;
    private float speed;

    public float bounceHeight = 0.5f;
    public float bounceFrequency = 8;

    public float baseHeight;
    

	// Use this for initialization
	void Start () {
        speed = transform.parent.GetComponent<PlayerController>().speed;
        rb = transform.parent.GetComponent<Rigidbody>();
        baseHeight = transform.localPosition.y;
	}



	void FixedUpdate () {
        Vector3 localPos = transform.localPosition;
        float scale = rb.velocity.magnitude / speed;
        if (!transform.parent.GetComponent<PlayerController>().IsGrounded()) scale = 0;
        localPos.y = baseHeight + (Mathf.Abs(Mathf.Sin(progress*bounceFrequency*Mathf.Clamp(1f/scale,.2f,1))*bounceHeight*scale));
        transform.localPosition = localPos;
        float dist = Mathf.Clamp(rb.velocity.magnitude,0,transform.parent.GetComponent<PlayerController>().speed) * Time.fixedDeltaTime;
        progress += dist;
        progress %= TAU * (radius);

        transform.parent.GetComponent<PoseManager>().progress = (progress % ((TAU / 4) * radius)/((TAU / 4) * radius));

        Vector3 temp = transform.position;
        temp.y -= height;
        
        for(int i = 0; i < 8; i++)
        {
            
            Vector3 rot = Quaternion.AngleAxis(Mathf.Rad2Deg * ((progress / radius)+ i*(TAU/8)) , transform.right) * Vector3.down;
            Debug.DrawRay(temp, rot * radius, (i % 2 == 0) ? Color.black : Color.gray);
        }

        int stage = Mathf.FloorToInt(progress / ((TAU / 4)*radius)) % 4;
        if (stage != lastStage)
        {
            lastStage = stage;
            transform.parent.GetComponent<PoseManager>().step();
        }
	}
}
