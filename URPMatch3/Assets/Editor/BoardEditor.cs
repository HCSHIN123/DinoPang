
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MapTool))]
[CanEditMultipleObjects]
public class BoardEditor : Editor
{
    MapTool m_Tool;
    private void OnEnable()
    {
      m_Tool = (MapTool)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        m_Tool.Number = EditorGUILayout.IntField(m_Tool.Number);
    }
}
