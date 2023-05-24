using System.Collections;
using System.Collections.Generic;
using UCL.Core.EditorLib.Page;
using UCL.Core.Game;
using UCL.Core.LocalizeLib;
using UnityEngine;



namespace SDU
{
    public class SDU_EditorMenuPage : UCL_EditorPage
    {
        public static string SDU_Version => Application.version;
        public override string WindowName => $"SDU_EditorMenu {SDU_Version}";

        protected override bool ShowCloseButton => false;
        protected override bool ShowBackButton => false;

        protected override void TopBar()
        {
            //base.TopBar();
        }
        //UCL.Core.UCL_ObjectDictionary m_Dic = new UCL.Core.UCL_ObjectDictionary();

        /// <summary>
        /// Draw Editor Munu
        /// </summary>
        protected override void ContentOnGUI()
        {
            using (var aScope = new GUILayout.VerticalScope("box"))//, GUILayout.MaxWidth(320)
            {
                if (GUILayout.Button("Install StableDiffusion", UCL.Core.UI.UCL_GUIStyle.ButtonStyle))
                {
                    SDU_StableDiffusionPage.Create();
                }

#if !UNITY_EDITOR
                GUILayout.Space(30);
                if (GUILayout.Button("Exit SDU"))
                {
                    UCL_GameManager.Instance.ExitGame();
                }
#endif
            }
        }
    }
}