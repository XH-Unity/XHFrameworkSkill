using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;

public class LubanToolsEditor
{
    private const string LubanDataPath = "Luban/MiniTemplate/Datas";
    private const string LubanGenBatPath = "Luban/MiniTemplate/gen.bat";

    [MenuItem("Tools/Luban/打开配置表文件夹")]
    public static void OpenLubanDataFolder()
    {
        string projectPath = Directory.GetParent(Application.dataPath).FullName;
        string fullPath = Path.Combine(projectPath, LubanDataPath);

        if (Directory.Exists(fullPath))
        {
            Process.Start("explorer.exe", fullPath.Replace("/", "\\"));
            UnityEngine.Debug.Log($"已打开文件夹: {fullPath}");
        }
        else
        {
            EditorUtility.DisplayDialog("错误", $"文件夹不存在: {fullPath}", "确定");
        }
    }

    [MenuItem("Tools/Luban/执行生成脚本")]
    public static void ExecuteGenBat()
    {
        string projectPath = Directory.GetParent(Application.dataPath).FullName;
        string batPath = Path.Combine(projectPath, LubanGenBatPath);
        string workingDirectory = Path.GetDirectoryName(batPath);

        if (File.Exists(batPath))
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = batPath,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                Process process = Process.Start(startInfo);
                UnityEngine.Debug.Log($"正在执行: {batPath}");

                // 可选：等待进程完成
                if (process != null)
                {
                    process.WaitForExit();
                    if (process.ExitCode == 0)
                    {
                        UnityEngine.Debug.Log("Luban生成脚本执行成功！");
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"Luban生成脚本执行完成，退出代码: {process.ExitCode}");
                    }
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("错误", $"执行失败: {e.Message}", "确定");
                UnityEngine.Debug.LogError($"执行gen.bat失败: {e.Message}");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("错误", $"文件不存在: {batPath}", "确定");
        }
    }
}
