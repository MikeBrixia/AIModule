using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AIModule.Navigation
{
    [CustomEditor(typeof(NavMesh2D))]
    public class NavMeshInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate Navigation Mesh"))
            {
                NavMesh2D nav = serializedObject.targetObject as NavMesh2D;
                if(nav != null)
                   nav.BakeNavigationMesh();
            }
        }
    }
}
