#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public sealed class WebGLHostingCompatibility : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL)
            return;

        // Unity Cloud's private player could not fetch either .gz or
        // decompression-fallback .unityweb payloads. An uncompressed WebGL
        // package is larger, but it works without special response headers.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;

        // This game does not need persistent browser storage. Disabling data
        // caching avoids an IndexedDB/IDBFS startup dependency in hosted players.
        PlayerSettings.WebGL.dataCaching = false;
    }
}
#endif
