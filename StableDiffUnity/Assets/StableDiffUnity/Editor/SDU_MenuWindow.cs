using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace SDU
{
    public class SDU_MenuWindow : EditorWindow
    {
        Rect m_GridRegion = new Rect();
        public SDU_EditorMenu m_Editor;

        [UnityEditor.MenuItem("SDU/Menu")]
        public static void ShowMenu()
        {
            ShowWindow(new SDU_EditorMenu());
        }
        private void OnInspectorUpdate()
        {
            Repaint();
        }
        public void Init(SDU_EditorMenu iEditor)
        {
            m_Editor = iEditor;
        }
        public static SDU_MenuWindow ShowWindow(SDU_EditorMenu iTarget)
        {
            var aWindow = EditorWindow.GetWindow<SDU_MenuWindow>("SDU_EditorMenu");
            aWindow.Init(iTarget);
            return aWindow;
        }
        private void OnGUI()
        {
            if (m_Editor == null)
            {
                m_Editor = new SDU_EditorMenu();
            }
            UCL.Core.UI.UCL_GUIStyle.IsInEditorWindow = true;
            m_Editor.Init();
            m_Editor.EditWindow(0);
            if (Event.current.type == EventType.Repaint)
            {
                var aNewRgn = GUILayoutUtility.GetLastRect();
                if (aNewRgn != m_GridRegion || UCL.Core.UI.UCL_GUILayout.s_RequireRepaint)
                {
                    UCL.Core.UI.UCL_GUILayout.s_RequireRepaint = false;
                    m_GridRegion = aNewRgn;
                    Repaint();
                }
            }
            UCL.Core.UI.UCL_GUIStyle.IsInEditorWindow = false;
        }
    }
}