﻿namespace SlideshowScreenSaver.Lib
{
	/// <summary>
	/// スクリーンセーバの動作
	/// </summary>
	public enum ScreenSaverBehavior
	{
		/// <summary>
		/// スクリーンセーバを起動
		/// </summary>
		RunScreenSaver,

		/// <summary>
		/// プレビュー表示
		/// </summary>
		ShowPreview,

		/// <summary>
		/// オプションを表示
		/// </summary>
		ShowOption,

		/// <summary>
		/// 無効
		/// </summary>
		Invalid = -1,
	}
}