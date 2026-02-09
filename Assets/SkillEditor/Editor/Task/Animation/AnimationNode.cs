using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using SkillEditor.Data;
using Spine.Unity;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 动画节点 - 支持拖拽Spine文件、选择动画、预览播放，并在时间轴上触发效果和Cue
    /// </summary>
    public class AnimationNode : SkillNodeBase<AnimationNodeData>
    {
        // 第一行：Spine资源
        private ObjectField _skeletonDataAssetField;

        // 第二行：动画选择 + 帧数 + 循环
        private PopupField<string> _animationPopup;
        private TextField _animationDurationField;
        private Toggle _isAnimationLoopingToggle;
        private List<string> _animationChoices = new List<string> { "(无)" };
        private VisualElement _animConfigRow;

        // 第三行：Spine预览区域
        private IMGUIContainer _previewContainer;
        private VisualElement _previewSection;
        private Button _playPauseButton;

        // Spine预览渲染器
        private SpinePreviewRenderer _spineRenderer;

        // Timeline视图
        private TimelineView _timelineView;
        private VisualElement _timelineContainer;
        private bool _timelineSectionFolded = false;

        // EditorUpdate注册状态
        private bool _editorUpdateRegistered;

        public AnimationNode(Vector2 position) : base(NodeType.Animation, position) { }

        protected override string GetNodeTitle() => "动画";
        protected override float GetNodeWidth() => 1020;

        protected override void CreateContent()
        {
            // 创建动画配置区域
            CreateAnimationConfigSection();

            // 创建Timeline区域
            CreateTimelineSection();
        }

        #region 动画配置区域

        private void CreateAnimationConfigSection()
        {
            var container = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(56f / 255f, 56f / 255f, 56f / 255f),
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 6,
                    paddingBottom = 6,
                    marginTop = 8
                }
            };

            // === 第一行：Spine资源拖拽 ===
            var row1 = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 }
            };

            _skeletonDataAssetField = new ObjectField("Spine资源")
            {
                objectType = typeof(SkeletonDataAsset),
                value = TypedData?.skeletonDataAsset
            };
            _skeletonDataAssetField.style.flexGrow = 0;
            _skeletonDataAssetField.style.width = 300;
            _skeletonDataAssetField.labelElement.style.minWidth = 60;
            _skeletonDataAssetField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.skeletonDataAsset = evt.newValue as SkeletonDataAsset;
                    OnSkeletonDataAssetChanged();
                    NotifyDataChanged();
                }
            });
            row1.Add(_skeletonDataAssetField);
            container.Add(row1);

            // === 第二行：动画选择 + 帧数 + 循环 ===
            _animConfigRow = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, marginBottom = 4 }
            };

            // 动画下拉列表
            _animationPopup = new PopupField<string>("动画", _animationChoices, 0);
            _animationPopup.style.width = 250;
            _animationPopup.style.marginRight = 8;
            _animationPopup.labelElement.style.minWidth = 30;
            _animationPopup.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null && evt.newValue != "(无)")
                {
                    TypedData.animationName = evt.newValue;
                    OnAnimationSelected(evt.newValue);
                    NotifyDataChanged();
                }
            });
            _animConfigRow.Add(_animationPopup);

            // 动画帧数
            _animationDurationField = new TextField("帧数") { value = TypedData?.animationDuration ?? "10" };
            _animationDurationField.style.width = 100;
            _animationDurationField.style.marginRight = 8;
            _animationDurationField.labelElement.style.minWidth = 30;
            _animationDurationField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.animationDuration = evt.newValue;
                    _timelineView?.UpdateDuration();
                    NotifyDataChanged();
                }
            });
            _animConfigRow.Add(_animationDurationField);

            // 循环播放
            _isAnimationLoopingToggle = new Toggle("循环") { value = TypedData?.isAnimationLooping ?? false };
            _isAnimationLoopingToggle.style.marginRight = 8;
            _isAnimationLoopingToggle.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.isAnimationLooping = evt.newValue;
                    // 更新预览的循环状态
                    if (_spineRenderer != null && _spineRenderer.IsInitialized && !string.IsNullOrEmpty(TypedData.animationName))
                    {
                        _spineRenderer.SetAnimation(TypedData.animationName, evt.newValue);
                    }
                    NotifyDataChanged();
                }
            });
            _animConfigRow.Add(_isAnimationLoopingToggle);

            container.Add(_animConfigRow);

            // === 第三行：Spine预览区域 ===
            _previewSection = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Center
                }
            };

            // 预览画面（IMGUI渲染）
            _previewContainer = new IMGUIContainer(OnPreviewGUI)
            {
                style =
                {
                    width = 300,
                    height = 200,
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    borderTopLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderBottomLeftRadius = 4,
                    borderBottomRightRadius = 4,
                    marginBottom = 4
                }
            };
            _previewSection.Add(_previewContainer);

            // 播放控制行
            var controlRow = new VisualElement
            {
                style = { flexDirection = FlexDirection.Row, alignItems = Align.Center, justifyContent = Justify.Center }
            };

            _playPauseButton = new Button(OnPlayPauseClicked) { text = "▶ 播放" };
            _playPauseButton.style.width = 80;
            _playPauseButton.style.height = 22;
            controlRow.Add(_playPauseButton);

            _previewSection.Add(controlRow);

            // 默认隐藏预览区域（没有Spine资源时）
            _previewSection.style.display = DisplayStyle.None;
            container.Add(_previewSection);

            mainContainer.Add(container);

            // 如果已有Spine资源，初始化预览
            if (TypedData?.skeletonDataAsset != null)
            {
                OnSkeletonDataAssetChanged();
            }
        }

        #endregion

        #region Spine预览

        /// <summary>
        /// SkeletonDataAsset变更时的处理
        /// </summary>
        private void OnSkeletonDataAssetChanged()
        {
            // 清理旧的渲染器
            CleanupSpineRenderer();

            var asset = TypedData?.skeletonDataAsset;
            if (asset == null)
            {
                _previewSection.style.display = DisplayStyle.None;
                ResetAnimationChoices();
                return;
            }

            // 初始化渲染器
            _spineRenderer = new SpinePreviewRenderer();
            if (!_spineRenderer.Initialize(asset))
            {
                CleanupSpineRenderer();
                _previewSection.style.display = DisplayStyle.None;
                ResetAnimationChoices();
                return;
            }

            // 显示预览区域
            _previewSection.style.display = DisplayStyle.Flex;

            // 填充动画下拉列表
            var names = _spineRenderer.GetAnimationNames();
            _animationChoices.Clear();
            _animationChoices.Add("(无)");
            _animationChoices.AddRange(names);

            // 恢复之前选择的动画
            string currentAnim = TypedData?.animationName ?? "";
            if (!string.IsNullOrEmpty(currentAnim) && _animationChoices.Contains(currentAnim))
            {
                _animationPopup.SetValueWithoutNotify(currentAnim);
                OnAnimationSelected(currentAnim);
            }
            else
            {
                _animationPopup.SetValueWithoutNotify("(无)");
            }

            // 注册EditorUpdate
            RegisterEditorUpdate();
        }

        /// <summary>
        /// 动画选择后的处理
        /// </summary>
        private void OnAnimationSelected(string animationName)
        {
            if (_spineRenderer == null || !_spineRenderer.IsInitialized) return;
            if (string.IsNullOrEmpty(animationName) || animationName == "(无)") return;

            bool loop = TypedData?.isAnimationLooping ?? false;
            _spineRenderer.SetAnimation(animationName, loop);

            // 自动计算帧数并写入
            int totalFrames = _spineRenderer.TotalFrames;
            if (totalFrames > 0)
            {
                TypedData.animationDuration = totalFrames.ToString();
                _animationDurationField.SetValueWithoutNotify(totalFrames.ToString());

                // 更新Timeline
                _timelineView?.UpdateDuration();
            }

            // 显示播放指示器
            _timelineView?.SetPlaybackIndicatorVisible(true);
            _timelineView?.SetPlaybackFrame(0);

            UpdatePlayPauseButton();
            RepaintPreview();
        }

        /// <summary>
        /// 播放/暂停按钮点击
        /// </summary>
        private void OnPlayPauseClicked()
        {
            if (_spineRenderer == null || !_spineRenderer.IsInitialized) return;

            _spineRenderer.TogglePlayPause();
            UpdatePlayPauseButton();
        }

        /// <summary>
        /// 更新播放/暂停按钮文本
        /// </summary>
        private void UpdatePlayPauseButton()
        {
            if (_playPauseButton == null) return;
            bool playing = _spineRenderer?.IsPlaying ?? false;
            _playPauseButton.text = playing ? "⏸ 暂停" : "▶ 播放";
        }

        /// <summary>
        /// IMGUI预览渲染回调
        /// </summary>
        private void OnPreviewGUI()
        {
            if (_spineRenderer == null || !_spineRenderer.IsInitialized) return;

            var texture = _spineRenderer.RenderResult;
            if (texture != null)
            {
                var rect = GUILayoutUtility.GetRect(300, 200);
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            }

            // 显示帧信息
            if (_spineRenderer.TotalFrames > 0)
            {
                var infoRect = new Rect(4, 180, 292, 18);
                var oldColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.7f);
                GUI.Label(infoRect, $"帧: {_spineRenderer.CurrentFrame} / {_spineRenderer.TotalFrames}",
                    EditorStyles.miniLabel);
                GUI.color = oldColor;
            }
        }

        /// <summary>
        /// EditorApplication.update回调 - 驱动Spine预览更新
        /// </summary>
        private void OnEditorUpdate()
        {
            if (_spineRenderer == null || !_spineRenderer.IsInitialized) return;

            _spineRenderer.EditorUpdate();

            // 更新播放指示器位置
            if (_spineRenderer.IsPlaying && _timelineView != null)
            {
                _timelineView.SetPlaybackFrame(_spineRenderer.CurrentFrame);
            }

            // 更新按钮状态（播放结束时自动切换）
            UpdatePlayPauseButton();

            // 请求重绘预览
            RepaintPreview();
        }

        /// <summary>
        /// 注册EditorUpdate
        /// </summary>
        private void RegisterEditorUpdate()
        {
            if (_editorUpdateRegistered) return;
            EditorApplication.update += OnEditorUpdate;
            _editorUpdateRegistered = true;
        }

        /// <summary>
        /// 取消注册EditorUpdate
        /// </summary>
        private void UnregisterEditorUpdate()
        {
            if (!_editorUpdateRegistered) return;
            EditorApplication.update -= OnEditorUpdate;
            _editorUpdateRegistered = false;
        }

        /// <summary>
        /// 清理Spine渲染器
        /// </summary>
        private void CleanupSpineRenderer()
        {
            if (_spineRenderer != null)
            {
                _spineRenderer.Cleanup();
                _spineRenderer = null;
            }
            UnregisterEditorUpdate();
            _timelineView?.SetPlaybackIndicatorVisible(false);
        }

        /// <summary>
        /// 重置动画下拉列表
        /// </summary>
        private void ResetAnimationChoices()
        {
            _animationChoices.Clear();
            _animationChoices.Add("(无)");
            _animationPopup?.SetValueWithoutNotify("(无)");
        }

        /// <summary>
        /// 播放指示器拖拽跳转回调
        /// </summary>
        private void OnPlaybackSeek(int frame)
        {
            if (_spineRenderer == null || !_spineRenderer.IsInitialized) return;
            _spineRenderer.SeekToFrame(frame);
            RepaintPreview();
        }

        /// <summary>
        /// 强制重绘预览区域（MarkDirtyRepaint仅标记脏，需要额外触发EditorWindow.Repaint才能刷新画面）
        /// </summary>
        private EditorWindow _cachedEditorWindow;
        private void RepaintPreview()
        {
            _previewContainer?.MarkDirtyRepaint();

            if (_cachedEditorWindow == null)
            {
                var panel = _previewContainer?.panel;
                if (panel != null)
                {
                    foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
                    {
                        if (w.rootVisualElement?.panel == panel)
                        {
                            _cachedEditorWindow = w;
                            break;
                        }
                    }
                }
            }
            _cachedEditorWindow?.Repaint();
        }

        /// <summary>
        /// 绑定播放指示器事件
        /// </summary>
        private void BindPlaybackIndicator()
        {
            if (_timelineView == null) return;

            var indicator = _timelineView.GetPlaybackIndicator();
            if (indicator != null)
            {
                indicator.OnSeekToFrame -= OnPlaybackSeek;
                indicator.OnSeekToFrame += OnPlaybackSeek;
            }
        }

        #endregion

        #region Timeline区域

        private void CreateTimelineSection()
        {
            // 时间轴整个区域
            _timelineContainer = new VisualElement
            {
                name = "TimelineSection",
                style =
                {
                    backgroundColor = new Color(56f / 255f, 56f / 255f, 56f / 255f),
                    borderTopLeftRadius = 8,
                    borderTopRightRadius = 8,
                    borderBottomLeftRadius = 8,
                    borderBottomRightRadius = 8,
                    paddingLeft = 8,
                    paddingRight = 8,
                    paddingTop = 8,
                    paddingBottom = 8,
                    marginTop = 8,
                    minWidth = 1004
                }
            };

            // 创建Timeline视图
            _timelineView = new TimelineView();
            _timelineView.style.display = _timelineSectionFolded ? DisplayStyle.None : DisplayStyle.Flex;
            _timelineView.OnDataChanged += NotifyDataChanged;
            _timelineView.OnAddButtonClicked += OnTimelineAddClicked;
            _timelineContainer.Add(_timelineView);

            mainContainer.Add(_timelineContainer);

            // 初始化Timeline
            RefreshTimeline();
        }

        private void ToggleTimelineSection()
        {
            _timelineSectionFolded = !_timelineSectionFolded;
            if (_timelineView != null)
                _timelineView.style.display = _timelineSectionFolded ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void OnTimelineAddClicked()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("时间效果"), false, () =>
            {
                if (TypedData == null) return;
                if (TypedData.timeEffects == null)
                    TypedData.timeEffects = new List<TimeEffectData>();

                TypedData.timeEffects.Add(new TimeEffectData());

                if (_timelineSectionFolded)
                    ToggleTimelineSection();

                _timelineView?.AddNewTrack(false);
                RefreshPorts();
                NotifyDataChanged();
            });
            menu.AddItem(new GUIContent("时间Cue"), false, () =>
            {
                if (TypedData == null) return;
                if (TypedData.timeCues == null)
                    TypedData.timeCues = new List<TimeCueData>();

                TypedData.timeCues.Add(new TimeCueData());

                if (_timelineSectionFolded)
                    ToggleTimelineSection();

                _timelineView?.AddNewTrack(true);
                RefreshPorts();
                NotifyDataChanged();
            });
            menu.ShowAsContext();
        }

        private void RefreshTimeline()
        {
            if (_timelineView == null || TypedData == null) return;

            _timelineView.Initialize(TypedData, () =>
            {
                var port = TimelinePort.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
                return port;
            });

            RefreshPorts();

            // 绑定播放指示器事件
            BindPlaybackIndicator();
        }

        #endregion

        #region 端口查找

        /// <summary>
        /// 根据端口标识符查找输出端口（支持普通端口和Timeline端口）
        /// </summary>
        public override Port FindOutputPortByIdentifier(string portIdentifier)
        {
            if (_timelineView != null)
            {
                var port = _timelineView.FindPortByIdentifier(portIdentifier);
                if (port != null) return port;
            }

            return base.FindOutputPortByIdentifier(portIdentifier);
        }

        #endregion

        #region 数据加载/保存

        public override void LoadData(NodeData data)
        {
            base.LoadData(data);
            SyncUIFromData();
        }

        public override void SyncUIFromData()
        {
            base.SyncUIFromData();
            if (TypedData == null) return;

            // 同步Spine资源
            if (_skeletonDataAssetField != null)
                _skeletonDataAssetField.SetValueWithoutNotify(TypedData.skeletonDataAsset);

            // 同步动画帧数
            if (_animationDurationField != null)
                _animationDurationField.SetValueWithoutNotify(TypedData.animationDuration ?? "10");

            // 同步循环
            if (_isAnimationLoopingToggle != null)
                _isAnimationLoopingToggle.SetValueWithoutNotify(TypedData.isAnimationLooping);

            // 如果有Spine资源，重新初始化预览
            if (TypedData.skeletonDataAsset != null)
            {
                OnSkeletonDataAssetChanged();
            }
            else
            {
                // 同步动画名称到下拉列表（无Spine资源时显示为文本）
                ResetAnimationChoices();
            }

            // 刷新Timeline
            RefreshTimeline();
        }

        #endregion

        #region 生命周期

        ~AnimationNode()
        {
            CleanupSpineRenderer();
        }

        #endregion
    }
}
