using System;
using FairyGUI;
using System.Collections.Generic;
using UnityEngine;


namespace XiHUI
{
	/// <summary>
	/// UI窗口栈管理
	/// </summary>
	public class UIStack
	{
		private GComponent _layer;
		private GGraph _modal;
		private GLoader _blurBG;
		private UIStack _baseStack;
		private bool _topOnly;

		private readonly List<UIDialog> _stack = new List<UIDialog>();

		private Action OnLayerChange;
		private RenderTexture _blurRT;

		private float _blurScale;
		private float _blurSize;

		public bool HasFullScreenDialog { get; private set; } = false;


		public UIStack(Mode mode)
		{
			_layer = new GComponent();
			_layer.MakeFullScreen();
			_layer.AddRelation(GRoot.inst, RelationType.Size);
			_layer.gameObjectName = mode.ToString();
			_layer.SetVisible(false);
			GRoot.inst.AddChildAt(_layer, (int)mode);

			if (mode == Mode.Modal)
			{
				_modal = new GGraph();
                //_modal.DrawRect(GRoot.inst.width, GRoot.inst.height, 0, Color.white, new Color(0f, 0.047f, 0.094f, 0.8f));
                _modal.DrawRect(GRoot.inst.width, GRoot.inst.height, 0, Color.white, new Color(0f, 0f, 0f, 0f));
#if UNITY_EDITOR
                Debug.LogWarning($"UIStack Modal 遮罩太黑，调整为0");
#endif
                _modal.AddRelation(GRoot.inst, RelationType.Size);
				_modal.name = _modal.gameObjectName = "Modal";
				_modal.SetHome(_layer);
				_layer.AddChild(_modal);
			}

			_topOnly = mode == Mode.Stack;
			CreateBlurBg();
			OnLayerChange += RefreshHasFullScreenDialog;
		}

		private void CreateBlurBg()
		{
			_blurBG = new GLoader();
			_blurBG.MakeFullScreen();
			_blurBG.AddRelation(GRoot.inst, RelationType.Size);
			_blurBG.name = _blurBG.gameObjectName = "BlurBG";
			_blurBG.SetHome(_layer);
			_blurBG.fill = FillType.Scale;
			_layer.AddChild(_blurBG);
		}

		public void SetBlurParams(float blurBGScale, float blursize)
		{
			_blurScale = blurBGScale;
			_blurSize = blursize;
		}

		public void SetBase(UIStack stack)
		{
			if (stack == null)
				return;

			_baseStack = stack;
			_baseStack.OnLayerChange += SetDialogStateByBase;
		}

		private void SetDialogStateByBase()
		{
			foreach (var d in _stack)
			{
				if (d == null || d.State == State.Loading)
					continue;

				d.BaseDialogs.RemoveAll(x => x == null || x.State == State.Close);
				if (d.BaseDialogs.Count == 0)
					d.Dispose();
				else
				{
					var hide = true;
					foreach (var parent in d.BaseDialogs)
					{
						if (parent.State == State.Open)
						{
							d.Open();
							hide = false;
							break;
						}
					}

					if (hide)
						d.Hide();
				}
			}

			_stack?.RemoveAll(x => x == null || x.State == State.Close);
			SortDepth();
		}

		public UIDialog Get(string name) => _stack?.Find(x => x != null && x.dialogName == name);


		/// <summary>
		/// 打开对话框并置于栈顶，并关闭原栈顶对话框
		/// </summary>
		public void Push(UIDialog dialog)
		{
			if (dialog == null || dialog.State == State.Close)
				return;

			if (dialog.State == State.Loading)
			{
				_stack.Add(dialog);
				return;
			}

			// 重新添加到栈顶
			if (_stack.IndexOf(dialog) != _stack.Count - 1)
			{
				_stack.Remove(dialog);
				_stack.Add(dialog);
			}

			dialog.SetBase(_baseStack?.GetTop());

			SortDepth();
		}

		/// <summary>
		/// 从栈内弹出对话框，如果是栈顶则自动打开新的栈顶窗口
		/// </summary>
		public void Pop(UIDialog dialog)
		{
			if (dialog == null || !_stack.Contains(dialog) || dialog.State == State.Close)
				return;

			dialog.Dispose();
			_stack.Remove(dialog);

			SortDepth();
		}

		/// <summary>
		/// 关闭全部UI
		/// </summary>
		/// <param name="exceptNames">关闭全部UI时的排除列表</param>
		private List<DialogOpenParams> _temp = new List<DialogOpenParams>();
        readonly static HashSet<string> defaultExceptDialogs=new HashSet<string>() { };//不会随场景跳转而关闭的UI
        public List<DialogOpenParams> Clear(HashSet<string> exceptNames = null)
		{
			if (exceptNames == null) exceptNames = defaultExceptDialogs;
            _temp.Clear();
			_stack?.RemoveAll((d) => {
				if (d == null)
					return true;

				if (exceptNames.Contains(d.dialogName) || defaultExceptDialogs.Contains(d.dialogName))
					return false;
				else
				{
					_temp.Add(d.OpenParams);
					d.Dispose();
					return true;
				}
			});

			SortDepth();
			return _temp;
		}

		/// <summary>
		/// 整理UI显示层级
		/// </summary>
		private void SortDepth()
		{
			var top = GetTop();

			if (_topOnly && top != null)
			{
				_stack.ForEach((x) => {
					if (x == top && x.State != State.Open)
						x.Open();
					else if (x != top && x.State == State.Open)
						x.Hide();
				});
			}

			var show = false;
			var idx = 0;
			for (int i = 0; i < _stack.Count; i++)
			{
				var dialog = _stack[i];
				if (dialog == null || dialog.State != State.Open)
					continue;

				var go = dialog.Content;

				if (go.parent == _layer)
					_layer.SetChildIndex(go, idx);
				else
				{
					_layer.AddChildAt(go, idx);
					go.MakeFullScreen();
					go.SetXY(0, 0);
					go.AddRelation(_layer, RelationType.Size);
				}

				show = true;
				idx++;
			}

			_layer?.SetVisible(show);

			if (_modal != null && show)
			{
				_layer.SetChildIndex(_modal, _stack.Count - 1);
				_layer.SetChildIndex(_stack[_stack.Count - 1].Content, _stack.Count);
			}

			if (_blurBG != null && show)
			{
				_blurBG.SetVisible(false);
				if (top != null && top.IsBlurBG)
				{
					top?.Content?.SetVisible(false);
					Camera camera = Camera.main;
					Rect rect = camera.pixelRect;

					_blurRT = new RenderTexture((int)(_blurBG.width * _blurScale), (int)(_blurBG.height * _blurScale), 0);
					_blurBG.texture = new NTexture(_blurRT);

					var b = new BlurFilter();
					b.blurSize = _blurSize;
					_blurBG.filter = b;
				//	b.target.captureOnce = true;


					//截图前准备
					if (!UIDialogManager.Instance.CameraRendering)
					{
						UIDialogManager.SetCameraRendering(true);
					}
					//var blit = BlitToFeature.IsEnabled;
					//BlitToFeature.IsEnabled = false;

					//camera.targetTexture = _blurRT;
					//camera.Render();
				//	camera.targetTexture = null;

					//截图后恢复
					//BlitToFeature.IsEnabled = blit;
					if (!UIDialogManager.Instance.CameraRendering)
					{
						UIDialogManager.SetCameraRendering(false);
					}

					_layer.SetChildIndex(_blurBG, _stack.Count - 1);
					_layer.SetChildIndex(_stack[_stack.Count - 1].Content, _stack.Count);
					top?.Content?.SetVisible(true);
					_blurBG.SetVisible(true);
				}
				else
				{
					GameObject.Destroy(_blurRT);
				}
			}

			OnLayerChange?.Invoke();
		}

		private void RefreshHasFullScreenDialog()
		{
			HasFullScreenDialog = false;
			foreach (var dialog in _stack)
			{
				if (dialog.State == State.Open && (dialog.OpenParams.IsFull || dialog.OpenParams.IsBlur))
				{
					HasFullScreenDialog = true;
					break;
				}
			}

			UIDialogManager.Instance.RefreshCameraRendering?.Invoke();
		}

		public UIDialog GetTop()
		{
			if (_stack.Count <= 0)
				return null;

			for (int i = _stack.Count - 1; i >= 0; i--)
			{
				var dialog = _stack[i];
				if (dialog != null && dialog.State != State.Loading)
					return dialog;
			}

			return null;
		}

		public void GetAll(ref IList<UIDialog> result)
		{
			if (result == null)
			{
				result = new List<UIDialog>(_stack);
			}
			else
			{
				result.Clear();
				foreach (var d in _stack) result.Add(d);
			}
		}

		public void Poll()
		{
			for (int i = 0; i < _stack.Count; i++)
			{
				var ui = _stack[i];
				if (ui.State == State.Open)
					ui.Update();
			}
		}

	}
}
