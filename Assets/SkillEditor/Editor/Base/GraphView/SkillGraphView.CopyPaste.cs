using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public partial class SkillGraphView
    {
        private string SerializeGraphElements(IEnumerable<GraphElement> elements)
        {
            var copyData = new CopyPasteData();
            var minPosition = new Vector2(float.MaxValue, float.MaxValue);

            foreach (var element in elements)
            {
                if (element is SkillNodeBase node)
                {
                    var nodeData = node.SaveData();

                    if (nodeData.position.x < minPosition.x)
                        minPosition.x = nodeData.position.x;  
                    if (nodeData.position.y < minPosition.y)
                        minPosition.y = nodeData.position.y;

                    copyData.nodes.Add(nodeData);
                }
            }

            copyData.minPosition = minPosition;
            return JsonUtility.ToJson(copyData);
        }

        private void UnserializeAndPaste(string operationName, string data)
        {
            var copyData = JsonUtility.FromJson<CopyPasteData>(data);

            foreach (var nodeData in copyData.nodes)
            {
                nodeData.guid = System.Guid.NewGuid().ToString();
                nodeData.position = nodeData.position - copyData.minPosition + pastePosition;

                var node = NodeFactory.CreateNodeFromData(nodeData);
                if (node != null)
                {
                    AddElement(node);
                }
            }

            ClearSelection();
            OnGraphModified?.Invoke();
        }

        private bool CanPasteSerializedData(string data)
        {
            try
            {
                var copyData = JsonUtility.FromJson<CopyPasteData>(data);
                return copyData != null && copyData.nodes != null;
            }
            catch
            {
                return false;
            }
        }
    }

    [Serializable]
    public class CopyPasteData
    {
        public List<NodeData> nodes = new List<NodeData>();
        public Vector2 minPosition;
    }
}
