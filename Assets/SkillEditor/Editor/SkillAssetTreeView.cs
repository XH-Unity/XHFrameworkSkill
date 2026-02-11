using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public class SkillAssetTreeView : TreeView
    {
        private const string RootPath = "Assets/Unity/Resources/ScriptObject/SkillAsset";
        public event Action<string> OnFileSelected;

        private Dictionary<int, string> idToPathMap = new Dictionary<int, string>();
        private int currentId = 1;

        public SkillAssetTreeView(TreeViewState state) : base(state)
        {
            showAlternatingRowBackgrounds = false;
            showBorder = true;
            useScrollView = true;
            Reload();
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        public void RefreshTree()
        {
            AssetDatabase.Refresh();
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            idToPathMap.Clear();
            currentId = 1;

            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            if (!Directory.Exists(RootPath))
            {
                Directory.CreateDirectory(RootPath);
                AssetDatabase.Refresh();
            }

            var rootFolder = new TreeViewItem
            {
                id = currentId++,
                depth = 0,
                displayName = "SkillAsset",
                icon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D
            };
            idToPathMap[rootFolder.id] = RootPath;
            root.AddChild(rootFolder);

            BuildTreeRecursive(RootPath, rootFolder, 1);

            SetupDepthsFromParentsAndChildren(root);

            // Expand the root SkillAsset folder by default
            SetExpanded(rootFolder.id, true);

            return root;
        }

        private void BuildTreeRecursive(string path, TreeViewItem parent, int depth)
        {
            var directories = Directory.GetDirectories(path)
                .OrderBy(d => Directory.GetCreationTime(d))
                .ToArray();
            var files = Directory.GetFiles(path, "*.asset")
                .OrderBy(f => File.GetCreationTime(f))
                .ToArray();

            foreach (var dir in directories)
            {
                var dirName = Path.GetFileName(dir);
                var item = new TreeViewItem
                {
                    id = currentId++,
                    depth = depth,
                    displayName = dirName,
                    icon = EditorGUIUtility.IconContent("Folder Icon").image as Texture2D
                };
                idToPathMap[item.id] = dir.Replace("\\", "/");
                parent.AddChild(item);
                BuildTreeRecursive(dir, item, depth + 1);
            }

            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var item = new TreeViewItem
                {
                    id = currentId++,
                    depth = depth,
                    displayName = fileName,
                    icon = EditorGUIUtility.IconContent("ScriptableObject Icon").image as Texture2D
                };
                idToPathMap[item.id] = file.Replace("\\", "/");
                parent.AddChild(item);
            }
        }

        private string GetPathFromId(int id)
        {
            return idToPathMap.TryGetValue(id, out var path) ? path : null;
        }

        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);
            var path = GetPathFromId(id);
            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path) && path.EndsWith(".asset"))
                {
                    OnFileSelected?.Invoke(path);
                }
                else if (Directory.Exists(path))
                {
                    // 选择文件夹时，传递空值来禁用编辑区
                    OnFileSelected?.Invoke(null);
                }
            }
        }

        protected override void ContextClickedItem(int id)
        {
            var path = GetPathFromId(id);
            if (string.IsNullOrEmpty(path)) return;

            var menu = new GenericMenu();

            if (Directory.Exists(path))
            {
                menu.AddItem(new GUIContent("新建文件夹"), false, () => CreateFolder(path));
                menu.AddItem(new GUIContent("新建技能文件"), false, () => CreateSkillFile(path));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("重命名"), false, () => BeginRename(id, path));
                menu.AddSeparator("");
                if (path != RootPath)
                {
                    menu.AddItem(new GUIContent("删除文件夹"), false, () => DeleteFolder(path));
                }
                else
                {
                    menu.AddDisabledItem(new GUIContent("删除文件夹"));
                }
            }
            else if (File.Exists(path))
            {
                menu.AddItem(new GUIContent("重命名"), false, () => BeginRename(id, path));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("删除文件"), false, () => DeleteFile(path));
            }

            menu.ShowAsContext();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);

            // Handle right-click on row
            if (Event.current.type == EventType.ContextClick)
            {
                var rect = args.rowRect;
                if (rect.Contains(Event.current.mousePosition))
                {
                    ContextClickedItem(args.item.id);
                    Event.current.Use();
                }
            }
        }

        protected override void ContextClicked()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("新建文件夹"), false, () => CreateFolder(RootPath));
            menu.AddItem(new GUIContent("新建技能文件"), false, () => CreateSkillFile(RootPath));
            menu.ShowAsContext();
        }

        private void CreateFolder(string parentPath)
        {
            var basePath = Path.Combine(parentPath, "NewFolder").Replace("\\", "/");
            var newFolderPath = AssetDatabase.GenerateUniqueAssetPath(basePath);
            Directory.CreateDirectory(newFolderPath);
            AssetDatabase.Refresh();
            Reload();

            foreach (var kvp in idToPathMap)
            {
                if (kvp.Value == newFolderPath)
                {
                    SetExpanded(GetParentId(kvp.Key), true);
                    SetSelection(new List<int> { kvp.Key });
                    OnFileSelected?.Invoke(null);
                    BeginRename(kvp.Key, newFolderPath);
                    break;
                }
            }
        }

        private void CreateSkillFile(string parentPath)
        {
            var basePath = Path.Combine(parentPath, "NewSkill.asset").Replace("\\", "/");
            var newFilePath = AssetDatabase.GenerateUniqueAssetPath(basePath);
            var skillData = ScriptableObject.CreateInstance<SkillGraphData>();
            AssetDatabase.CreateAsset(skillData, newFilePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Reload();

            foreach (var kvp in idToPathMap)
            {
                if (kvp.Value == newFilePath)
                {
                    SetExpanded(GetParentId(kvp.Key), true);
                    SetSelection(new List<int> { kvp.Key });
                    OnFileSelected?.Invoke(newFilePath);
                    BeginRename(kvp.Key, newFilePath);
                    break;
                }
            }
        }

        private int GetParentId(int childId)
        {
            var item = FindItem(childId, rootItem);
            return item?.parent?.id ?? 0;
        }

        private void BeginRename(int id, string path)
        {
            var isDirectory = Directory.Exists(path);
            var oldName = isDirectory ? Path.GetFileName(path) : Path.GetFileNameWithoutExtension(path);

            var popup = ScriptableObject.CreateInstance<RenamePopup>();
            popup.Initialize(oldName, newName =>
            {
                if (string.IsNullOrEmpty(newName) || newName == oldName) return;

                try
                {
                    string newPath;
                    if (isDirectory)
                    {
                        newPath = Path.Combine(Path.GetDirectoryName(path), newName).Replace("\\", "/");
                    }
                    else
                    {
                        newPath = Path.Combine(Path.GetDirectoryName(path), newName + ".asset").Replace("\\", "/");
                    }

                    var result = AssetDatabase.MoveAsset(path, newPath);
                    if (string.IsNullOrEmpty(result))
                    {
                        AssetDatabase.Refresh();
                        Reload();
                    }
                    else
                    {
                        Debug.LogError($"重命名失败: {result}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"重命名失败: {e.Message}");
                }
            });
            popup.ShowUtility();
        }

        private void DeleteFolder(string path)
        {
            var folderName = Path.GetFileName(path);
            if (EditorUtility.DisplayDialog("删除文件夹", $"确定要删除文件夹 '{folderName}' 及其所有内容吗？", "删除", "取消"))
            {
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.Refresh();
                Reload();
            }
        }

        private void DeleteFile(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            if (EditorUtility.DisplayDialog("删除文件", $"确定要删除文件 '{fileName}' 吗？", "删除", "取消"))
            {
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.Refresh();
                Reload();
            }
        }

        #region Drag and Drop

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            var path = GetPathFromId(args.draggedItem.id);
            return !string.IsNullOrEmpty(path) && path != RootPath;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();

            var draggedItems = new List<string>();
            foreach (var id in args.draggedItemIDs)
            {
                var path = GetPathFromId(id);
                if (!string.IsNullOrEmpty(path))
                {
                    draggedItems.Add(path);
                }
            }

            DragAndDrop.paths = draggedItems.ToArray();
            DragAndDrop.SetGenericData("SkillAssetTreeDrag", draggedItems);
            DragAndDrop.StartDrag("拖拽文件");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            var draggedPaths = DragAndDrop.GetGenericData("SkillAssetTreeDrag") as List<string>;
            if (draggedPaths == null || draggedPaths.Count == 0)
                return DragAndDropVisualMode.None;

            string targetPath = RootPath;

            // Only allow dropping into folders
            if (args.parentItem != null && args.parentItem.id != 0)
            {
                var parentPath = GetPathFromId(args.parentItem.id);
                if (!string.IsNullOrEmpty(parentPath) && Directory.Exists(parentPath))
                {
                    targetPath = parentPath;
                }
            }

            // Prevent dropping a folder into itself or its children
            foreach (var draggedPath in draggedPaths)
            {
                if (targetPath.StartsWith(draggedPath) || draggedPath == targetPath)
                {
                    return DragAndDropVisualMode.Rejected;
                }
            }

            if (args.performDrop)
            {
                foreach (var sourcePath in draggedPaths)
                {
                    var fileName = Path.GetFileName(sourcePath);
                    var destPath = Path.Combine(targetPath, fileName).Replace("\\", "/");

                    if (sourcePath == destPath) continue;

                    if (AssetDatabase.IsValidFolder(sourcePath) || File.Exists(sourcePath))
                    {
                        // Don't generate unique path if destination doesn't exist
                        if (!File.Exists(destPath) && !Directory.Exists(destPath))
                        {
                            var result = AssetDatabase.MoveAsset(sourcePath, destPath);
                            if (!string.IsNullOrEmpty(result))
                            {
                                Debug.LogError($"移动失败: {result}");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"目标路径已存在: {destPath}");
                        }
                    }
                }

                AssetDatabase.Refresh();
                Reload();
            }

            return DragAndDropVisualMode.Move;
        }

        #endregion
    }

    public class RenamePopup : EditorWindow
    {
        private string newName;
        private Action<string> onConfirm;
        private bool focusSet;

        public void Initialize(string currentName, Action<string> callback)
        {
            newName = currentName;
            onConfirm = callback;
            titleContent = new GUIContent("重命名");
            var size = new Vector2(300, 90);
            minSize = size;
            maxSize = size;
            focusSet = false;

            var mainWindowPos = EditorGUIUtility.GetMainWindowPosition();
            position = new Rect(
                mainWindowPos.x + (mainWindowPos.width - size.x) / 2,
                mainWindowPos.y + (mainWindowPos.height - size.y) / 2,
                size.x, size.y
            );
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("新名称:", EditorStyles.boldLabel);

            GUI.SetNextControlName("RenameField");
            newName = EditorGUILayout.TextField(newName);

            if (!focusSet)
            {
                EditorGUI.FocusTextInControl("RenameField");
                focusSet = true;
            }

            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    onConfirm?.Invoke(newName);
                    Close();
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Escape)
                {
                    Close();
                    Event.current.Use();
                }
            }

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("确定", GUILayout.Width(80)))
            {
                onConfirm?.Invoke(newName);
                Close();
            }

            if (GUILayout.Button("取消", GUILayout.Width(80)))
            {
                Close();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
}
