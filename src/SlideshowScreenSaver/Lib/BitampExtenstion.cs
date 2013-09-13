using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SlideshowScreenSaver.Lib
{
	/// <summary>
	/// Bitmapクラスの拡張メソッド
	/// </summary>
	public static class BitampExtenstion
	{
		#region dll import

		[DllImport("kernel32.dll", EntryPoint = "RtlCopyMemory")]
		private static extern unsafe void RtlCopyMemory( byte* dest, byte* src, uint len );

		#endregion

		/// <summary>
		/// 背景を単一の色で塗りつぶす
		/// </summary>
		/// <param name="bmp">対象のビットマップ</param>
		/// <param name="color">塗りつぶす色</param>
		public static void Clear(this Bitmap bmp, Color color)
		{
			using (var g = Graphics.FromImage(bmp))
			{
				g.Clear(color);
			}
		}

		/// <summary>
		/// 高速にビットマップをスクロールする
		/// 垂直のみ対応
		/// </summary>
		/// <param name="bmp"></param>
		public static void ScrollFast(this Bitmap bmp)
		{
			#region ビットマップを直接操作して高速にスクロール

			unsafe
			{
				var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
				BitmapData bd = null;
				try
				{
					// ビットマップの内部データにアクセスするためにロックする
					bd = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

					int scrollY = 1; // 垂直の移動距離
					for (int y = 0; y < bd.Height - scrollY; y++)
					{
						var dst = (byte*) bd.Scan0 + bd.Stride*y;
						var src = (byte*) bd.Scan0 + bd.Stride*(y + scrollY);

						// 一行毎のコピー
						RtlCopyMemory(dst, src, (uint)bd.Stride);
					}

					#region 新しい行の塗りつぶし

					{
						var dst = (byte*) bd.Scan0 + bd.Stride*(bd.Height - scrollY);
						var trm = (byte*) bd.Scan0 + bd.Stride*(bd.Height);
						while (dst < trm)
						{
							*dst++ = 0;
						}
					}

					#endregion
				}
				finally
				{
					// ロックの解除
					if (bd != null)
					{
						bmp.UnlockBits(bd);
					}
				}
			}

			#endregion
		}

		/// <summary>
		/// ビットマップをスクロールさせる
		/// </summary>
		/// <param name="bmp">対象のビットマップ</param>
		/// <param name="x">水平方向の移動距離</param>
		/// <param name="y">垂直方向の移動距離</param>
		/// <param name="backColor">移動後、抜けた部分の色</param>
		public static void Scroll(this Bitmap bmp, float x, float y, Color backColor)
		{
			#region 移動距離が0の場合は何もしない
			if (x == 0f && y == 0f)
			{
				return;
			}
			#endregion

			#region スクロール描画
			using (var tmpBmp = new Bitmap(bmp, bmp.Size))
			{
				#region 一旦別のビットマップへスクロールを描画する
				using (var tmpGrap = Graphics.FromImage(tmpBmp))
				{
					tmpGrap.Clear(backColor);
					tmpGrap.DrawImage(bmp, x, y, bmp.Width, bmp.Height);
				}
				#endregion

				#region スクロールしたビットマップを書き戻す
				using (var tmpGrap = Graphics.FromImage(bmp))
				{
					tmpGrap.DrawImage(tmpBmp, 0, 0, bmp.Width, bmp.Height);
				}
				#endregion
			}
			#endregion
		}
	}
}