using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace SlideshowScreenSaver.Lib
{
	public static class Utility
	{
		/// <summary>
		/// 矩形内に内接する最大の固定比率の矩形を求める
		/// </summary>
		/// <param name="frame">最大サイズ</param>
		/// <param name="aspectRatio">比率</param>
		/// <returns></returns>
		public static SizeF CalcFitRectangle( SizeF frame, SizeF aspectRatio )
		{
			return aspectRatio.Height * frame.Width / aspectRatio.Width > frame.Height
				? new SizeF(aspectRatio.Width * frame.Height / aspectRatio.Height, frame.Height)
				: new SizeF(frame.Width, aspectRatio.Height * frame.Width / aspectRatio.Width);
		}

		/// <summary>
		/// ミューテックスを作成する
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static bool CreateMutex(string name)
		{
			var m = new Mutex(false, name);
			if (!m.WaitOne(0, false))
			{
				return false;
			}

			GC.KeepAlive(m);

			m.Close();
			return true;
		}

		/// <summary>
		/// コマンドラインの引数から指定されたスクリーンセーバの動作を取得する
		/// </summary>
		/// <returns></returns>
		public static ScreenSaverBehavior GetScreenSaverBehavior()
		{
			var args = Environment.GetCommandLineArgs();
			if (args.Length <= 1)
			{
				return ScreenSaverBehavior.RunScreenSaver;
			}
			var arg = args[1].ToLower();
			if (arg.StartsWith("/c"))
			{
				return ScreenSaverBehavior.ShowOption;
			}
			if (arg == "/p" && args.Length >= 3)
			{
				return ScreenSaverBehavior.ShowPreview;
			}
			if (arg == "/s")
			{
				return ScreenSaverBehavior.RunScreenSaver;
			}
			return ScreenSaverBehavior.Invalid;
		}

		/// <summary>
		/// スクリーンセーバを表示する
		/// </summary>
		/// <typeparam name="T">スクリーンセーバを表示するフォーム
		///  ScreenSaverBaseFormを継承している必要がある</typeparam>
		public static void ShowScreenSaver<T>()
			where T : ScreenSaverBaseForm, new()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// すべてのディスプレイで表示する
			foreach (var screen in Screen.AllScreens)
			{
				 new T {Bounds = screen.Bounds}.Show();
			}

			Application.Run();
		}

		/// <summary>
		/// スクリーンセーバのプレビューを表示する
		/// </summary>
		/// <typeparam name="T">スクリーンセーバを表示するフォーム
		///  ScreenSaverBaseFormを継承している必要がある</typeparam>
		public static void ShowPreview<T>()
			where T : ScreenSaverBaseForm, new()
		{
			#region コマンドラインから 親となるウィンドウのハンドルを取得
			var args = Environment.GetCommandLineArgs();
			if (args.Length < 3)
				throw new InvalidOperationException();

			var handle = new IntPtr(long.Parse(args[2]));
			#endregion

			#region 引数(IntPtr)を持つコンストラクタを取得
			var t = typeof(T);

			var ctor = t.GetConstructor(new []{typeof(IntPtr)});
			if (ctor == null)
			{
				throw new InvalidOperationException();
			}
			#endregion

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run((T) ctor.Invoke(new object[] {handle}));
		}

		/// <summary>
		/// スクリーンセーバの設定画面を表示する
		/// </summary>
		/// <typeparam name="T">スクリーンセーバの設定画面</typeparam>
		public static void ShowOption<T>()
			where T : Form, new()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new T());
		}

		/// <summary>
		/// コマンドラインの支持に従いスクリーンセーバを起動する
		/// </summary>
		/// <typeparam name="TScreenSaver">スクリーンセーバを表示するフォーム
		///  ScreenSaverBaseFormを継承している必要がある</typeparam>
		public static void RunScreenSaver<TScreenSaver>()
			where TScreenSaver : ScreenSaverBaseForm, new()
		{
			switch (GetScreenSaverBehavior())
			{
				case ScreenSaverBehavior.ShowPreview:
					ShowPreview<TScreenSaver>();
					break;

				case ScreenSaverBehavior.RunScreenSaver:
					ShowScreenSaver<TScreenSaver>();
					break;

				case ScreenSaverBehavior.ShowOption:
					MessageBox.Show(@"設定オプションはありません", Application.ProductName,
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);
					break;
				default:
					ShowScreenSaver<TScreenSaver>();
					break;
			}
		}

		/// <summary>
		/// コマンドラインの支持に従いスクリーンセーバを起動する
		/// </summary>
		/// <typeparam name="TScreenSaver">スクリーンセーバを表示するフォーム
		///  ScreenSaverBaseFormを継承している必要がある</typeparam>
		/// <typeparam name="TOption">スクリーンセーバの設定画面</typeparam>
		public static void RunScreenSaver<TScreenSaver, TOption>()
			where TScreenSaver : ScreenSaverBaseForm, new()
			where TOption : Form, new()
		{
			switch (GetScreenSaverBehavior())
			{
                case ScreenSaverBehavior.ShowPreview:
					ShowPreview<TScreenSaver>();
					break;

				case ScreenSaverBehavior.RunScreenSaver:
					ShowScreenSaver<TScreenSaver>();
					break;

				case ScreenSaverBehavior.ShowOption:
					ShowOption<TOption>();
					break;
				default:
					ShowScreenSaver<TScreenSaver>();
					break;
			}
		}
	}
}
