using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    public Vector3 offset;
    public GameObject parent;
    public float rotation, mouseSensitivity;

	// Use this for initialization
	void Start () {
        offset = parent.transform.InverseTransformPoint(transform.position);
	}
	
	// Update is called once per frame
	void Update () {
        float dir = Input.GetAxis("Mouse X") * mouseSensitivity;
        rotation += dir;
        transform.position = parent.transform.position + (Quaternion.AngleAxis(rotation, Vector3.up) * offset);
        Vector3 plane = parent.transform.position;
        plane.y = transform.position.y;
        transform.LookAt(plane);
    }
}
