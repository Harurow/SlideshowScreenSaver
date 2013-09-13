using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SlideshowScreenSaver.Lib
{
	public abstract partial class ScreenSaverBaseForm : Form
	{
		#region Windows API

		[DllImport("user32.dll")]
		static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

		[DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

		[DllImport("user32.dll", SetLastError = true)]
		static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

		#endregion

		#region プロパティおよびフィールド

		#region プレビュー用

		private bool IsPreviewMode { get; set; }

		private readonly IntPtr _previewHandle = IntPtr.Zero;

		#endregion

		#region 乱数

		private static readonly object SyncRandom = new object();

		private static Random _random;

		#endregion

		#region マウスの移動距離を測る

		private Point _startLocation;

		private bool _hasStartLocation;

		#endregion

		#region ビットマップのキャッシュ

		/// <summary>
		/// CacheBitmapを利用する場合は、このオブジェクトをlockすること
		/// </summary>
		protected readonly object SyncCacheBitmap = new object();

		private Bitmap _cacheBitmap;

		/// <summary>
		/// ビットマップキャッシュ
		/// このオブジェクトを利用する場合は、SyncCacheBitmapをlockすること
		/// </summary>
		protected Bitmap CacheBitmap
		{
			get
			{
				return _cacheBitmap;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}

				var lastCache = _cacheBitmap;
				lock (SyncCacheBitmap)
				{
					_cacheBitmap = value;
				}

				if (lastCache != null)
				{
					lastCache.Dispose();
				}
			}
		}

		#endregion

		#endregion

		#region コンストラクタとそのヘルパ

		private void CommonInit()
		{
			DoubleBuffered = true;
			BackColor = Color.Black;
			ControlBox = false;
			FormBorderStyle = FormBorderStyle.None;
			ShowInTaskbar = false;
			SizeGripStyle = SizeGripStyle.Hide;
			Cursor.Hide();
		}

		/// <summary>
		/// デフォルトコンストラクタ
		/// スクリーンセーバで利用する
		/// </summary>
		protected ScreenSaverBaseForm()
		{
			InitializeComponent();

			CommonInit();

#if !DEBUG
			TopMost = true;
#endif
			StartPosition = FormStartPosition.Manual;
			WindowState = FormWindowState.Maximized;
		}

		/// <summary>
		/// プレビューモード用のコンストラクタ
		/// 必ず継承先でも実装すること
		/// </summary>
		/// <param name="previewHandle"></param>
		protected ScreenSaverBaseForm(IntPtr previewHandle)
		{
			_previewHandle = previewHandle;

			InitializeComponent();

			CommonInit();

			IsPreviewMode = true;
		}

		#endregion

		#region method

		/// <summary>
		/// 乱数を取得する
		/// </summary>
		/// <param name="maxValue">取得する乱数の最大値</param>
		/// <returns></returns>
		protected int GetNext(int maxValue)
		{
			lock (SyncRandom)
			{
				if (_random == null)
				{
					_random = new Random(Environment.TickCount);
				}
				return _random.Next(maxValue);
			}
		}

		/// <summary>
		/// 乱数を取得する
		/// </summary>
		/// <param name="minValue">取得する乱数の最小値</param>
		/// <param name="maxValue">取得する乱数の最大値</param>
		/// <returns></returns>
		protected int GetNext(int minValue, int maxValue )
		{
			lock (SyncRandom)
			{
				if (_random == null)
				{
					_random = new Random(Environment.TickCount);
				}
				return _random.Next(minValue, maxValue);
			}
		}

		#endregion

		#region override method

		#region keyboard / mouse events

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (!IsPreviewMode)
			{
				Application.Exit();
			}
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);

			if (!IsPreviewMode)
			{
				Application.Exit();
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (!IsPreviewMode)
			{
				if (!_hasStartLocation)
				{
					_startLocation = e.Location;
					_hasStartLocation = true;
				}
				else
				{
					if (Math.Abs(_startLocation.X - e.X) > SystemInformation.DragSize.Width
					    || Math.Abs(_startLocation.Y - e.Y) > SystemInformation.DragSize.Height)
					{
						Application.Exit();
					}
				}
			}
		}

		#endregion

		protected override void OnCreateControl()
		{
			#region プレビューモードの場合は、親のウィンドウにアタッチする
			if (IsPreviewMode)
			{
				const int GWL_STYLE = -16;
				const int WS_CHILD = 0x40000000;

				SetParent(Handle, _previewHandle);

				SetWindowLong(Handle, GWL_STYLE, new IntPtr(GetWindowLong(Handle, GWL_STYLE) | WS_CHILD));

				Rectangle rect;
				GetClientRect(_previewHandle, out rect);
				Size = rect.Size;
				Location = new Point(0, 0);
			}
			#endregion

			base.OnCreateControl();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			// ビットマップキャッシュを作成
			_cacheBitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
			_cacheBitmap.Clear(Color.Black);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			// 描画はビットマップキャッシュを利用する
			lock (SyncCacheBitmap)
			{
				e.Graphics.DrawImage(_cacheBitmap, 0, 0, Width, Height);
			}
		}

		#endregion
	}
}
