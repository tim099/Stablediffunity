using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SDU
{
    public enum CMDArg
    {
        /// <summary>
        /// --api
        /// </summary>
        Api,
        /// <summary>
        /// --xformers
        /// </summary>
        Xformers,
        /// <summary>
        /// --no-half-vae
        /// </summary>
        NoHalfVae,
        /// <summary>
        /// --reinstall-xformers
        /// </summary>
        ReinstallXformers,
    }
    public enum PythonArg
    {
        /// <summary>
        /// -Xfrozen_modules=off
        /// </summary>
        XfrozenModuleOff,

    }
    public static partial class ArgExtensions
    {
        public static string ArgToString(this CMDArg iArg)
        {
            switch (iArg)
            {
                case CMDArg.Api: return "--api";
                case CMDArg.Xformers: return "--xformers";
                case CMDArg.NoHalfVae: return "--no-half-vae";
                case CMDArg.ReinstallXformers: return "--reinstall-xformers";
            }
            return string.Empty;
        }
        public static string ArgsToString(this List<CMDArg> iArgs)
        {
            if (iArgs.IsNullOrEmpty()) return string.Empty;
            return " " + iArgs.ConcatString(iArg => iArg.ArgToString(), " ");
        }

        public static string ArgToString(this PythonArg iArg)
        {
            switch (iArg)
            {
                case PythonArg.XfrozenModuleOff: return "-Xfrozen_modules=off";

            }
            return string.Empty;
        }
        public static string ArgsToString(this List<PythonArg> iArgs)
        {
            if (iArgs.IsNullOrEmpty()) return string.Empty;
            return " " + iArgs.ConcatString(iArg => iArg.ArgToString(), " ");
        }
    }
    public class BootSetting : UCL.Core.JsonLib.UnityJsonSerializable
    {
        public string m_CommandlineArg = "";//"--api --xformers";
        public List<CMDArg> m_CommandlineArgs = new List<CMDArg>() { CMDArg.Api, CMDArg.Xformers };
        public string m_PythonArg;//-Xfrozen_modules=off
        public List<PythonArg> m_PythonArgs = new List<PythonArg>();

        public string CommandlineArgs => m_CommandlineArg + m_CommandlineArgs.ArgsToString();
        public string PythonArgs => m_PythonArg + m_PythonArgs.ArgsToString();
    }
}