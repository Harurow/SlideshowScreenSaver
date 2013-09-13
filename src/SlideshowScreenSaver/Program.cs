using System;
using System.Windows.Forms;
using SlideshowScreenSaver.Lib;

namespace SlideshowScreenSaver
{
	static class Program
	{
		/// <summary>
		/// アプリケーションのメイン エントリ ポイントです。
		/// </summary>
		[STAThread]
		static void Main()
		{
			// 多重起動禁止
			if (Utility.CreateMutex(Application.ProductName))
			{
				// スクリーンセーバをコマンドラインによってい切り分けて起動
				Utility.RunScreenSaver<MainForm, OptionForm>();
			}
		}
	}
}
