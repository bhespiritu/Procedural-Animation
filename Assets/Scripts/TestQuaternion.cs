using UnityEngine;
using System.Collections;

public class TestQuaternion : MonoBehaviour {

    public Quaternion rot;
    public GameObject baseTransform;

	// Use this for initialization
	void Start () {
        Mirror();
        rot = transform.rotation;
	}

    public void Mirror()
    {
        GameObject bone = gameObject;
        Vector3 localPos = bone.transform.position - baseTransform.transform.position;
        Vector3 localRot =  bone.transform.rotation.eulerAngles;
        Debug.Log(localPos + " " + localRot);
        localPos.x *= -1;
        localRot.y *= -1;
        Debug.Log(localPos + " " + localRot);
        transform.position = baseTransform.transform.position + localPos;
        transform.rotation = Quaternion.Euler(localRot);
    }

    // Update is called once per frame
    void Update () {

        transform.rotation = rot;
	}
}
