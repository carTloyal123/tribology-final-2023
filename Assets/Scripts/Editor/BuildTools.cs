using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.Build;

public class BuildMenu : MonoBehaviour
{
    [MenuItem("Custom/Build/Build All Platforms")]
    static void BuildAllPlatforms()
    {
        BuildWindowsMono();
        BuildOSX();
        BuildLinux();
    }

    [MenuItem("Custom/Build/Build Windows (Mono)")]
    static void BuildWindowsMono()
    {
        BuildForPlatform(BuildTarget.StandaloneWindows, BuildTargetGroup.Standalone, BuildOptions.None, BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows, ScriptingImplementation.Mono2x);
    }

    [MenuItem("Custom/Build/Build OSX")]
    static void BuildOSX()
    {
        BuildForPlatform(BuildTarget.StandaloneOSX, BuildTargetGroup.Standalone, BuildOptions.None, BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
    }

    [MenuItem("Custom/Build/Build Linux")]
    static void BuildLinux()
    {
        BuildForPlatform(BuildTarget.StandaloneLinux64, BuildTargetGroup.Standalone, BuildOptions.None, BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64, scriptingBackend: ScriptingImplementation.Mono2x);
    }

    static void BuildForPlatform(BuildTarget target, BuildTargetGroup targetGroup, BuildOptions options, BuildTargetGroup editorTargetGroup, BuildTarget editorTarget, ScriptingImplementation scriptingBackend = ScriptingImplementation.IL2CPP)
    {
        string outputPath = Path.Combine("Builds", target.ToString(), target.ToString());
        // Create the output folder if it doesn't exist
        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }
        
        EditorUserBuildSettings.SwitchActiveBuildTarget(editorTargetGroup, editorTarget);

        PlayerSettings.SetScriptingBackend(targetGroup, scriptingBackend);

        BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, outputPath, target, options);
        Debug.Log($"Done building for: {target.ToString()}");
    }
}