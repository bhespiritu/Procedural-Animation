using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SurveyorWheel))]
public class SurveyorWheelEditor : Editor {

        private SurveyorWheel c;

        public void OnSceneGUI()
        {
            c = target as SurveyorWheel;
            Handles.color = Color.red;
          Vector3 temp = c.transform.position;
            temp.y -= c.height;
            Handles.DrawWireDisc(temp // position
                                          , c.transform.right                       // normal
                                          , c.radius);                              // radius
        }
    
}
