using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 自定义端口 - 监听连接和断开事件
    /// </summary>
    public class TimelinePort : Port
    {
        public event Action OnConnectionChanged;

        protected TimelinePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type)
            : base(portOrientation, portDirection, portCapacity, type)
        {
        }

        public static TimelinePort Create<TEdge>(Orientation orientation, Direction direction, Capacity capacity, Type type)
            where TEdge : Edge, new()
        {
            var connectorListener = new DefaultEdgeConnectorListener();
            var port = new TimelinePort(orientation, direction, capacity, type)
            {
                m_EdgeConnector = new EdgeConnector<TEdge>(connectorListener)
            };
            port.AddManipulator(port.m_EdgeConnector);
            return port;
        }

        /// <summary>
        /// 重写Connect方法，监听连接事件
        /// </summary>
        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            // 延迟触发事件，确保连接状态已更新
            schedule.Execute(() =>
            {
                OnConnectionChanged?.Invoke();
            }).ExecuteLater(50);
        }

        /// <summary>
        /// 重写Disconnect方法，监听断开事件
        /// </summary>
        public override void Disconnect(Edge edge)
        {
            base.Disconnect(edge);
            // 延迟触发事件，确保连接状态已更新
            schedule.Execute(() =>
            {
                OnConnectionChanged?.Invoke();
            }).ExecuteLater(50);
        }

        /// <summary>
        /// 重写DisconnectAll方法，监听全部断开事件
        /// </summary>
        public override void DisconnectAll()
        {
            base.DisconnectAll();
            // 延迟触发事件，确保连接状态已更新
            schedule.Execute(() =>
            {
                OnConnectionChanged?.Invoke();
            }).ExecuteLater(50);
        }

        /// <summary>
        /// 默认的EdgeConnector监听器
        /// </summary>
        private class DefaultEdgeConnectorListener : IEdgeConnectorListener
        {
            public void OnDropOutsidePort(Edge edge, UnityEngine.Vector2 position) { }
            public void OnDrop(GraphView graphView, Edge edge)
            {
                // 让GraphView处理连接
                var edgesToCreate = new System.Collections.Generic.List<Edge> { edge };
                var edgesToDelete = new System.Collections.Generic.List<GraphElement>();

                // 如果输入端口已有连接且容量为Single，删除旧连接
                if (edge.input.capacity == Capacity.Single)
                {
                    foreach (var connection in edge.input.connections)
                    {
                        if (connection != edge)
                            edgesToDelete.Add(connection);
                    }
                }

                // 如果输出端口已有连接且容量为Single，删除旧连接
                if (edge.output.capacity == Capacity.Single)
                {
                    foreach (var connection in edge.output.connections)
                    {
                        if (connection != edge)
                            edgesToDelete.Add(connection);
                    }
                }

                if (edgesToDelete.Count > 0)
                    graphView.DeleteElements(edgesToDelete);

                var edgesToAdd = edgesToCreate;
                if (graphView.graphViewChanged != null)
                {
                    edgesToAdd = graphView.graphViewChanged(new GraphViewChange
                    {
                        edgesToCreate = edgesToCreate
                    }).edgesToCreate;
                }

                if (edgesToAdd != null)
                {
                    foreach (var e in edgesToAdd)
                    {
                        graphView.AddElement(e);
                        e.input.Connect(e);
                        e.output.Connect(e);
                    }
                }
            }
        }
    }
}
