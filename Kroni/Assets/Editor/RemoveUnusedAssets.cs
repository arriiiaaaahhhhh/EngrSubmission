#if UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class FindUnusedAssets : EditorWindow
{
    private HashSet<string> usedAssetPaths = new HashSet<string>();
    private List<string> allAssets = new List<string>();
    private List<string> unusedAssets = new List<string>();
    private Vector2 scroll;

    [MenuItem("Tools/Find Unused Assets")]
    public static void ShowWindow()
    {
        GetWindow<FindUnusedAssets>("Find Unused Assets");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Scan Build Scenes for Dependencies"))
        {
            Scan();
        }

        GUILayout.Space(10);
        GUILayout.Label($"Unused Assets: {unusedAssets.Count}", EditorStyles.boldLabel);

        scroll = GUILayout.BeginScrollView(scroll, false, true, GUILayout.Height(300));
        foreach (var path in unusedAssets)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(path, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Ping", GUILayout.Width(60)))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                EditorGUIUtility.PingObject(obj);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();

        GUILayout.Space(10);

        if (unusedAssets.Count > 0)
        {
            if (GUILayout.Button("Move Unused to Assets/_Unused (Review Manually)"))
            {
                MoveUnused();
            }
        }
    }

    void Scan()
    {
        usedAssetPaths.Clear();
        allAssets.Clear();
        unusedAssets.Clear();

        // Build Settings scenes
        var scenePaths = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        // Get all dependencies from those scenes
        var deps = AssetDatabase.GetDependencies(scenePaths, true);
        foreach (var d in deps)
        {
            if (d.StartsWith("Assets/"))
                usedAssetPaths.Add(d.Replace('\\', '/'));
        }

        // All assets under Assets/
        allAssets = AssetDatabase.GetAllAssetPaths()
            .Where(p => p.StartsWith("Assets/"))
            .Select(p => p.Replace('\\', '/'))
            .ToList();

        // Ignore folders and scripts (optional; scripts are usually "used" indirectly)
        var ignoreExtensions = new HashSet<string> { ".cs", ".shader", ".cginc", ".hlsl" };
        foreach (var path in allAssets)
        {
            if (Directory.Exists(path)) continue;

            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ignoreExtensions.Contains(ext)) continue;

            if (!usedAssetPaths.Contains(path))
            {
                unusedAssets.Add(path);
            }
        }

        // Sort by size (largest first) – optional
        unusedAssets = unusedAssets
            .OrderByDescending(p => new FileInfo(p).Length)
            .ToList();

        Debug.Log($"Scan complete. Unused: {unusedAssets.Count}");
    }

    void MoveUnused()
    {
        const string targetFolder = "Assets/_Unused";
        if (!AssetDatabase.IsValidFolder(targetFolder))
        {
            AssetDatabase.CreateFolder("Assets", "_Unused");
        }

        foreach (var path in unusedAssets)
        {
            var fileName = Path.GetFileName(path);
            var dest = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{fileName}");
            AssetDatabase.MoveAsset(path, dest);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Moved unused assets to Assets/_Unused. Review and delete if safe.");
    }
}
#endif
