using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public partial class SkillGraphView
    {
        public void LoadGraph(SkillGraphData graphData)
        {
            ClearGraph();

            // 1. 先创建所有节点
            var nodeMap = new Dictionary<string, SkillNodeBase>();
            foreach (var nodeData in graphData.nodes)
            {
                var node = NodeFactory.CreateNodeFromData(nodeData);
                if (node != null)
                {
                    AddElement(node);
                    nodeMap[node.Guid] = node;
                }
            }

            // 2. 再恢复所有连接
            foreach (var conn in graphData.connections)
            {
                // 跳过无效的连接数据
                if (string.IsNullOrEmpty(conn.outputNodeGuid) || string.IsNullOrEmpty(conn.inputNodeGuid))
                {
                    UnityEngine.Debug.LogWarning($"跳过无效连接数据: outputNodeGuid={conn.outputNodeGuid}, inputNodeGuid={conn.inputNodeGuid}");
                    continue;
                }

                if (string.IsNullOrEmpty(conn.outputPortName) || string.IsNullOrEmpty(conn.inputPortName))
                {
                    UnityEngine.Debug.LogWarning($"跳过无效连接数据: outputPortName={conn.outputPortName}, inputPortName={conn.inputPortName}");
                    continue;
                }

                if (!nodeMap.TryGetValue(conn.outputNodeGuid, out var outputNode))
                {
                    UnityEngine.Debug.LogWarning($"找不到输出节点: {conn.outputNodeGuid}");
                    continue;
                }

                if (!nodeMap.TryGetValue(conn.inputNodeGuid, out var inputNode))
                {
                    UnityEngine.Debug.LogWarning($"找不到输入节点: {conn.inputNodeGuid}");
                    continue;
                }

                // 查找输出端口（优先通过portId查找，再通过portName查找）
                var outputPort = FindOutputPort(outputNode, conn.outputPortName);
                if (outputPort == null)
                {
                    UnityEngine.Debug.LogWarning($"找不到输出端口: 节点={outputNode.Guid}, 端口={conn.outputPortName}");
                    continue;
                }

                // 查找输入端口
                var inputPort = inputNode.inputContainer
                    .Query<Port>()
                    .ToList()
                    .FirstOrDefault(p => p.portName == conn.inputPortName);

                if (inputPort == null)
                {
                    UnityEngine.Debug.LogWarning($"找不到输入端口: 节点={inputNode.Guid}, 端口={conn.inputPortName}");
                    continue;
                }

                var edge = outputPort.ConnectTo(inputPort);
                AddElement(edge);
            }
        }

        /// <summary>
        /// 查找输出端口（支持普通端口和Cue端口）
        /// </summary>
        private Port FindOutputPort(SkillNodeBase node, string portIdentifier)
        {
            // 优先使用节点自己的查找方法（支持自定义端口如TimeEffect/TimeCue）
            var port = node.FindOutputPortByIdentifier(portIdentifier);
            if (port != null) return port;

            // 回退：通过name属性在整个节点中查找（用于Cue端口，name存储的是portId）
            port = node.Query<Port>()
                .ToList()
                .FirstOrDefault(p => p.name == portIdentifier && p.direction == Direction.Output);

            return port;
        }

        public SkillGraphData SaveGraph(SkillGraphData graphData)
        {
            graphData.nodes.Clear();
            graphData.connections.Clear();

            // 1. 保存所有节点
            foreach (var node in nodes)
            {
                var skillNode = node as SkillNodeBase;
                if (skillNode != null)
                {
                    var nodeData = skillNode.SaveData();
                    graphData.nodes.Add(nodeData);
                }
            }

            // 2. 保存所有连接
            foreach (var edge in edges)
            {
                if (edge?.output == null || edge?.input == null)
                    continue;

                var outputNode = edge.output.node as SkillNodeBase;
                var inputNode = edge.input.node as SkillNodeBase;

                if (outputNode == null || inputNode == null)
                    continue;

                // 获取端口标识符（Cue端口使用name/portId，普通端口使用portName）
                var outputPortIdentifier = GetPortIdentifier(edge.output);
                var inputPortIdentifier = edge.input.portName;

                // 跳过无效的端口标识符
                if (string.IsNullOrEmpty(outputPortIdentifier) || string.IsNullOrEmpty(inputPortIdentifier))
                {
                    UnityEngine.Debug.LogWarning($"跳过无效连接: {outputNode.Guid} -> {inputNode.Guid}, outputPort={outputPortIdentifier}, inputPort={inputPortIdentifier}");
                    continue;
                }

                graphData.connections.Add(new ConnectionData
                {
                    outputNodeGuid = outputNode.Guid,
                    outputPortName = outputPortIdentifier,
                    inputNodeGuid = inputNode.Guid,
                    inputPortName = inputPortIdentifier
                });
            }

            return graphData;
        }

        /// <summary>
        /// 获取端口标识符（Cue端口返回name/portId，普通端口返回portName）
        /// </summary>
        private string GetPortIdentifier(Port port)
        {
            // 优先使用 portName
            if (!string.IsNullOrEmpty(port.portName))
            {
                return port.portName;
            }

            // 如果 portName 为空，尝试使用 name（用于 Cue 端口）
            if (!string.IsNullOrEmpty(port.name))
            {
                return port.name;
            }

            // 都为空，记录警告
            UnityEngine.Debug.LogWarning($"端口标识符为空: port.portName={port.portName}, port.name={port.name}");
            return "";
        }
    }
}
