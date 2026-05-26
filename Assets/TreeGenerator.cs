using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text;
using System.Linq;

public class ExportSceneTree : EditorWindow
{
    [MenuItem("Tools/Export Current Scene Tree with Components")]
    public static void ExportCurrentSceneTree()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        if (!activeScene.isLoaded)
        {
            return;
        }

        string exportsFolder = Path.Combine(Application.dataPath, "SceneExports");
        if (!Directory.Exists(exportsFolder))
        {
            Directory.CreateDirectory(exportsFolder);
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{activeScene.name}");
        var rootObjects = activeScene.GetRootGameObjects();

        foreach (GameObject root in rootObjects)
        {
            BuildTreeString(root, sb, true, true);
        }

        string filePath = Path.Combine(exportsFolder, $"{activeScene.name}_TreeWithComponents.txt");
        File.WriteAllText(filePath, sb.ToString());

        AssetDatabase.Refresh();
    }

    private static void BuildTreeString(GameObject obj, StringBuilder sb, bool isLast, bool isRoot)
    {
        string prefix = isRoot ? "" : (isLast ? "└── " : "├── ");
        sb.AppendLine($"{prefix}{obj.name}");

        string childIndent = isRoot ? "" : (isLast ? "    " : "│   ");

        // --- Add Components ---
        Component[] components = obj.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            bool lastComponent = (i == components.Length - 1);
            sb.Append(childIndent);
            sb.Append(lastComponent ? "└── " : "├── ");
            sb.AppendLine($"[{components[i].GetType().Name}]");
        }

        // --- Add Child GameObjects ---
        var children = obj.transform.Cast<Transform>().ToList();
        for (int i = 0; i < children.Count; i++)
        {
            bool lastChild = (i == children.Count - 1);
            sb.Append(childIndent);
            BuildTreeString(children[i].gameObject, sb, lastChild, false);
        }
    }
}