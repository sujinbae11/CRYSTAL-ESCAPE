using System.Runtime.InteropServices;
using UnityEngine;

namespace CrystalEscape
{
    internal static class WebGLIDBFSLinker
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void ForceIDBFSLink();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureIDBFSIsLinked()
        {
            ForceIDBFSLink();
        }
#endif
    }
}
