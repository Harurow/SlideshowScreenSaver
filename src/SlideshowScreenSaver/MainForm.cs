using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Win32;
using SlideshowScreenSaver.Lib;

namespace SlideshowScreenSaver
{
	/// <summary>
	/// スクリーンセーバを描画するフォーム
	/// </summary>
	public partial class MainForm : ScreenSaverBaseForm
	{
		/// <summary>
		/// 写真の最大傾き角度
		/// </summary>
		private const int Angle = 200;

		/// <summary>
		/// スクロールのタイマー間隔(ms)
		/// </summary>
		private const int ScrollInterval = 32;

		/// <summary>
		/// 写真を追加する間隔(ms)
		/// </summary>
		public static int PhotoInterval = 3000;

		/// <summary>
		/// 表示する写真のパスのリスト
		/// </summary>
		private readonly List<string> _photosList = new List<string>();

		#region 背景のビットマップキャッシュとその更新用タイマー
		private Bitmap _baseBitmap;
		private TimerEx _baseTimer;
		#endregion

		#region 新たに表示する写真のビットマップとその読み込み用タイマー
		private Bitmap _photoLayer;
		private TimerEx _photoTimer;
		#endregion

		/// <summary>
		/// 指定された写真のディレクトリにある写真を列挙しリスト化して保存する
		/// </summary>
		private void CreatePhotoList()
		{
			#region 設定を読み込む。設定がない場合は、デフォルトとしてマイピクチャーを指定

			var regKey = Registry.CurrentUser.CreateSubKey(OptionForm.RegAppRoot + @"\PhotoDir");

			if (regKey == null)
				throw new InvalidOperationException();

			var dir = (string)regKey.GetValue("Path1",
				Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));

			#endregion

			#region 写真を列挙し、リスト化する

			if (Directory.Exists(dir))
			{
				var files = Directory.GetFiles(dir);
				var photos = new List<string>(files.Length);

				#region 有効な拡張子のみ登録

				foreach (string file in files)
				{
					string ext = Path.GetExtension(file);
					if (ext != null)
					{
						ext = ext.ToLower();
						switch (ext)
						{
							case ".bmp":
							case ".png":
							case ".jpg":
							case ".jpeg":
							case ".jpe":
							case ".gif":
							case ".tif":
							case ".tiff":
								photos.Add(file);
								break;
						}
					}
				}

				#endregion

				#region リストへ登録

				_photosList.Clear();
				_photosList.AddRange(photos);

				#endregion
			}

			#endregion
		}

		#region ctor

		/// <summary>
		/// スクリーンセーバ用のコンストラクタ
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
		}


		/// <summary>
		/// プレビュー用のコンストラクタ
		/// </summary>
		/// <param name="handle"></param>
		public MainForm(IntPtr handle)
			: base(handle)
		{
			InitializeComponent();

			// プレビュー用のコンストラクタで必ず呼び出す
//			AttachPreview();
		}

		#endregion

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			#region 写真をリスト化する

			CreatePhotoList();

			#endregion

			#region 背景用のビットマップを作成、背景は黒で初期化

			_baseBitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
			_baseBitmap.Clear(Color.Black);

			#endregion

			#region 背景のスクロール用のタイマーを作成

			_baseTimer = new TimerEx { Interval = ScrollInterval };
			_baseTimer.Tick += BaseTimerOnTick;

			#endregion

			#region 写真を追加するタイマーを作成

			_photoTimer = new TimerEx { Interval = PhotoInterval };
			_photoTimer.Tick += PhotoTimerOnTick;

			#endregion

			#region 最初の写真を登録

			// 写真を追加するタイマーの間隔が長いため、
			// 先にイベントを呼び出し写真を登録しておく
			PhotoTimerOnTick(this, EventArgs.Empty);

			#endregion

			#region 背景のスクロール用のタイマーと、写真読み込み用のタイマーを起動する

			_baseTimer.Start();
			_photoTimer.Start();

			#endregion
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			#region タイマーの停止

			_baseTimer.Stop();
			_photoTimer.Stop();

			#endregion
		}

		/// <summary>
		/// 写真を読み込むタイマーイベントハンドラ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void PhotoTimerOnTick(object sender, EventArgs eventArgs)
		{
			#region 写真が見当たらない場合、まだ写真を描画していない場合は処理を終了

			if (_photosList.Count <= 0 || _photoLayer != null)
			{
				return;
			}

			#endregion

			#region ランダムで読み込む写真を決める

			var index = GetNext(_photosList.Count - 1);
			var path = _photosList[index];

			#endregion

			#region 写真を読み込み、キャッシュへ描画

			// 傾きの描画は描画速度が遅いため、別レイヤーとして間接的に描画する

			using (var photo = Image.FromFile(path))
			{
				// キャッシュ用のビットマップを作成
				var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

				using (var g = Graphics.FromImage(bmp))
				{
					#region 背景色を透明で塗りつぶす
					g.Clear(Color.Transparent);
					#endregion

					#region 写真描画の中央を求める
					float cx = Width/2f;
					float cy = Height*2/3f;

					// 横のブレを追加
					float offsetX = cx / 6f * GetNext(-5, 5);
					cx += offsetX;
					#endregion

					#region 最大サイズに収まるように写真サイズを計算

					var maxSize = new SizeF(Width*2f/4f, Height*2/6f);
					var size = Utility.CalcFitRectangle(maxSize, photo.Size);

					#endregion

					#region 描画モードを設定
					g.SmoothingMode = SmoothingMode.HighQuality;
					g.InterpolationMode = InterpolationMode.HighQualityBilinear;
					g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
					g.CompositingQuality = CompositingQuality.HighQuality;
					#endregion

					#region 傾きを設定

					// ランダムで傾きを求める
					float angle = GetNext(-Angle, Angle) / 10f;

					g.TranslateTransform(-cx, -cy);
					g.RotateTransform(angle, MatrixOrder.Append);
					g.TranslateTransform(cx, cy, MatrixOrder.Append);
					#endregion

					#region 写真の周りの枠の太さを求め描画

					var photoFrameWidth = (int) (Math.Min(size.Width, size.Height)/12);

					var frameSize = size;
					frameSize.Width += photoFrameWidth;
					frameSize.Height += photoFrameWidth;

					g.FillRectangle(Brushes.WhiteSmoke,
						cx - frameSize.Width/2, cy - frameSize.Height/2,
						frameSize.Width, frameSize.Height);

					g.DrawRectangle(Pens.Gainsboro,
						cx - frameSize.Width / 2, cy - frameSize.Height / 2,
						frameSize.Width, frameSize.Height);


					#endregion

					#region 写真を描画

					g.DrawImage(photo,
						cx - size.Width/2, cy - size.Height/2,
						size.Width, size.Height);

					#endregion
				}

				// 描画した写真を登録する
				_photoLayer = bmp;
			}

			#endregion
		}

		/// <summary>
		/// 背景をスクロールするタイマーイベントハンドラ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void BaseTimerOnTick(object sender, EventArgs eventArgs)
		{
			#region スクロール
			//_baseBitmap.Scroll(0, -1, Color.Black);
			_baseBitmap.ScrollFast();
			#endregion

			#region 写真があれば描画する
			if (_photoLayer != null)
			{
				using (var g = Graphics.FromImage(_baseBitmap))
				{
					g.DrawImage(_photoLayer, 0, 0, Width, Height);
				}

				_photoLayer.Dispose();
				_photoLayer = null;
			}
			#endregion

			#region スクリーンセーバのキャッシュへ描画する

			lock (SyncCacheBitmap)
			{
				using (var g = Graphics.FromImage(CacheBitmap))
				{
					g.DrawImage(_baseBitmap, 0, 0, _baseBitmap.Width, _baseBitmap.Height);
				}
			}

			#endregion

			#region 再描画を引き起こす
			Invalidate();
			#endregion
		}
	}
}
