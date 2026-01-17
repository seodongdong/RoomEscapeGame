// 에디터 헬퍼 스크립트 (테스트용)
// Assets/Editor/UISlotPositioner.cs

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class UISlotPositioner : EditorWindow
{
    public Transform slot3D;
    public RectTransform slotUI;
    
    [MenuItem("Tools/Position UI Slot")]
    static void Init()
    {
        GetWindow<UISlotPositioner>().Show();
    }
    
    void OnGUI()
    {
        slot3D = EditorGUILayout.ObjectField("3D Slot", slot3D, typeof(Transform), true) as Transform;
        slotUI = EditorGUILayout.ObjectField("UI Slot", slotUI, typeof(RectTransform), true) as RectTransform;
        
        if (GUILayout.Button("Position UI"))
        {
            if (slot3D != null && slotUI != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(slot3D.position);
                slotUI.position = screenPos;
            }
        }
    }
}
#endif