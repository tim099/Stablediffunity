using System.Collections;
using System.Collections.Generic;
using UCL.Core.EditorLib.Page;
using UCL.Core.LocalizeLib;
using UnityEngine;



namespace SDU
{
    public class SDU_EditorMenuPage : UCL_EditorPage
    {
        public override string WindowName => "SDU_EditorMenu";
        protected override bool ShowCloseButton => false;
        protected override bool ShowBackButton => false;

        //UCL.Core.UCL_ObjectDictionary m_Dic = new UCL.Core.UCL_ObjectDictionary();

        /// <summary>
        /// Draw Editor Munu
        /// </summary>
        protected override void ContentOnGUI()
        {
            using (var aScope = new GUILayout.VerticalScope("box"))//, GUILayout.MaxWidth(320)
            {
                if (GUILayout.Button("Install StableDiffusion"))
                {
                    UCL_EditorPage.Create<UCL_LocalizeEditPage>();
                }
            }
        }
    }
}