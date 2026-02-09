using UnityEngine;
using UnityEditor;
using Spine;
using Spine.Unity;
using Spine.Unity.Editor;
using System;
using System.Collections.Generic;

namespace SkillEditor.Editor
{
    /// <summary>
    /// Spine预览渲染器 - 在编辑器中将Spine动画渲染到RenderTexture
    /// 参考Spine官方SkeletonInspectorPreview实现
    /// </summary>
    public class SpinePreviewRenderer : IDisposable
    {
        // 预览层（与Spine官方一致）
        private const int PREVIEW_LAYER = 30;
        private const int PREVIEW_CAMERA_CULLING_MASK = 1 << PREVIEW_LAYER;

        // 预览环境
        private GameObject _cameraObject;
        private Camera _camera;
        private GameObject _previewObject;
        private SkeletonAnimation _skeletonAnimation;
        private Renderer _renderer;
        private RenderTexture _renderTexture;

        // 状态
        private bool _isInitialized;
        private bool _isPlaying;
        private int _currentFrame;
        private int _totalFrames;
        private float _animationDuration;
        private double _lastEditorTime;
        private List<string> _animationNames = new List<string>();
        private bool _needsRepaint;

        // 配置
        private int _width;
        private int _height;

        /// <summary>当前帧</summary>
        public int CurrentFrame => _currentFrame;

        /// <summary>总帧数</summary>
        public int TotalFrames => _totalFrames;

        /// <summary>是否正在播放</summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>渲染结果纹理</summary>
        public Texture RenderResult => _renderTexture;

        /// <summary>是否已初始化</summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// 初始化预览环境
        /// </summary>
        public bool Initialize(SkeletonDataAsset skeletonDataAsset, int width = 300, int height = 200)
        {
            Cleanup();

            if (skeletonDataAsset == null) return false;

            var skeletonData = skeletonDataAsset.GetSkeletonData(false);
            if (skeletonData == null) return false;

            _width = width;
            _height = height;

            try
            {
                // 使用Spine官方EditorInstantiation创建预览对象（自带MeshFilter + MeshRenderer + SkeletonAnimation）
                _skeletonAnimation = EditorInstantiation.InstantiateSkeletonAnimation(skeletonDataAsset, "", destroyInvalid: false, useObjectFactory: false);
                if (_skeletonAnimation == null || !_skeletonAnimation.valid)
                {
                    Cleanup();
                    return false;
                }

                _previewObject = _skeletonAnimation.gameObject;
                _previewObject.hideFlags = HideFlags.HideAndDontSave;
                _previewObject.layer = PREVIEW_LAYER;

                // 获取Renderer引用
                _renderer = _previewObject.GetComponent<Renderer>();

                // 初始渲染一帧以生成mesh和bounds
                _skeletonAnimation.LateUpdate();

                // 默认关闭renderer，仅在渲染时开启
                _renderer.enabled = false;

                // 创建相机
                _cameraObject = new GameObject("SpinePreviewCamera");
                _cameraObject.hideFlags = HideFlags.HideAndDontSave;

                _camera = _cameraObject.AddComponent<Camera>();
                _camera.orthographic = true;
                _camera.cullingMask = PREVIEW_CAMERA_CULLING_MASK;
                _camera.nearClipPlane = 0.01f;
                _camera.farClipPlane = 1000f;
                _camera.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
                _camera.clearFlags = CameraClearFlags.SolidColor;
                _camera.enabled = false; // 手动渲染

                // 根据mesh bounds自动调整相机
                AdjustCamera();

                // 收集动画名称
                _animationNames.Clear();
                foreach (var anim in skeletonData.Animations)
                {
                    _animationNames.Add(anim.Name);
                }

                // 创建RenderTexture
                _renderTexture = new RenderTexture(_width, _height, 16, RenderTextureFormat.ARGB32);
                _renderTexture.Create();

                _isInitialized = true;
                _lastEditorTime = EditorApplication.timeSinceStartup;

                // 渲染初始帧
                RenderFrame();

                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SpinePreviewRenderer] 初始化失败: {e.Message}");
                Cleanup();
                return false;
            }
        }

        /// <summary>
        /// 根据Renderer bounds自动调整相机位置和大小
        /// </summary>
        private void AdjustCamera()
        {
            if (_camera == null || _renderer == null) return;

            Bounds bounds = _renderer.bounds;
            _camera.orthographicSize = Mathf.Max(bounds.size.y, bounds.size.x * _height / _width) * 0.55f;
            _camera.transform.position = bounds.center + new Vector3(0, 0, -10f);
            _camera.transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// 设置动画
        /// </summary>
        public void SetAnimation(string animationName, bool loop)
        {
            if (!_isInitialized || _skeletonAnimation == null) return;

            var skeletonData = _skeletonAnimation.Skeleton.Data;
            var animation = skeletonData.FindAnimation(animationName);
            if (animation == null) return;

            _animationDuration = animation.Duration;
            _totalFrames = SkillEditorConstants.SecondsToFrames(_animationDuration);
            if (_totalFrames <= 0) _totalFrames = 1;

            // 重置到SetupPose后设置动画
            _skeletonAnimation.Skeleton.SetToSetupPose();
            _skeletonAnimation.AnimationState.SetAnimation(0, animationName, loop);

            _currentFrame = 0;
            _isPlaying = false;

            // 跳转到第0帧
            SeekToFrame(0);
        }

        /// <summary>
        /// 跳转到指定帧
        /// </summary>
        public void SeekToFrame(int frame)
        {
            if (!_isInitialized || _skeletonAnimation == null) return;

            _currentFrame = Mathf.Clamp(frame, 0, _totalFrames);
            float targetTime = SkillEditorConstants.FramesToSeconds(_currentFrame);

            // 重置骨骼到初始状态
            _skeletonAnimation.Skeleton.SetToSetupPose();

            // 获取当前TrackEntry并设置时间
            var trackEntry = _skeletonAnimation.AnimationState.GetCurrent(0);
            if (trackEntry != null)
            {
                trackEntry.TrackTime = targetTime;
                trackEntry.TimeScale = 0; // 冻结播放
            }

            // 必须先开启renderer，否则LateUpdate会跳过mesh生成
            _renderer.enabled = true;
            _skeletonAnimation.Update(0);
            _skeletonAnimation.LateUpdate();

            RenderFrame();
        }

        /// <summary>
        /// 切换播放/暂停
        /// </summary>
        public void TogglePlayPause()
        {
            if (!_isInitialized) return;

            _isPlaying = !_isPlaying;
            _lastEditorTime = EditorApplication.timeSinceStartup;

            var trackEntry = _skeletonAnimation.AnimationState.GetCurrent(0);
            if (trackEntry != null)
            {
                trackEntry.TimeScale = _isPlaying ? 1f : 0f;
            }
        }

        /// <summary>
        /// 编辑器更新（由EditorApplication.update驱动）
        /// </summary>
        public void EditorUpdate()
        {
            if (!_isInitialized || !_isPlaying || _skeletonAnimation == null) return;

            double currentTime = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(currentTime - _lastEditorTime);
            _lastEditorTime = currentTime;

            deltaTime = Mathf.Min(deltaTime, 0.1f);

            // 必须先开启renderer，否则LateUpdate会跳过mesh生成
            _renderer.enabled = true;
            _skeletonAnimation.Update(deltaTime);
            _skeletonAnimation.LateUpdate();

            // 更新当前帧
            var trackEntry = _skeletonAnimation.AnimationState.GetCurrent(0);
            if (trackEntry != null)
            {
                float animTime = trackEntry.AnimationTime;
                _currentFrame = SkillEditorConstants.SecondsToFrames(animTime);

                // 非循环动画播放完毕
                if (!trackEntry.Loop && animTime >= _animationDuration)
                {
                    _currentFrame = _totalFrames;
                    _isPlaying = false;
                    trackEntry.TimeScale = 0f;
                }
            }

            RenderFrame();
        }

        /// <summary>
        /// 渲染当前帧到RenderTexture
        /// </summary>
        public void RenderFrame()
        {
            if (!_isInitialized || _camera == null || _renderTexture == null || _renderer == null) return;

            // 渲染前开启renderer
            _renderer.enabled = true;

            var prevRT = RenderTexture.active;
            _camera.targetTexture = _renderTexture;
            _camera.Render();
            _camera.targetTexture = null;
            RenderTexture.active = prevRT;

            // 渲染后关闭renderer（避免出现在Scene视图中）
            _renderer.enabled = false;

            _needsRepaint = true;
        }

        /// <summary>
        /// 获取动画名称列表
        /// </summary>
        public List<string> GetAnimationNames()
        {
            return _animationNames;
        }

        /// <summary>
        /// 清理预览资源
        /// </summary>
        public void Cleanup()
        {
            _isPlaying = false;
            _isInitialized = false;
            _currentFrame = 0;
            _totalFrames = 0;
            _animationDuration = 0;
            _animationNames.Clear();

            _skeletonAnimation = null;
            _renderer = null;
            _camera = null;

            if (_previewObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_previewObject);
                _previewObject = null;
            }

            if (_cameraObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_cameraObject);
                _cameraObject = null;
            }

            if (_renderTexture != null)
            {
                _renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(_renderTexture);
                _renderTexture = null;
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}
