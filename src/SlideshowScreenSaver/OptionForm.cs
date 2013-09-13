using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SlideshowScreenSaver
{
	public partial class OptionForm : Form
	{
		public static readonly string RegAppRoot = @"Software\Harurow\SlideshowScreenSaver";

		/// <summary>
		/// 選択中のディレクトリ
		/// </summary>
		private string _photoDir;

		public OptionForm()
		{
			InitializeComponent();
		}

		private void OptionForm_Load( object sender, EventArgs e )
		{
			#region 設定を読み込む、設定がない場合は、マイピクチャー

			var regKey = Registry.CurrentUser.CreateSubKey(RegAppRoot + @"\PhotoDir");

			if (regKey == null)
				throw new InvalidOperationException();

			_photoDir = (string)regKey.GetValue("Path1",
				Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));

			#endregion

			#region 選択中のディレクトリ名を表示
			labelDirName.Text = Path.GetFileName(_photoDir);
			#endregion
		}

		/// <summary>
		/// 保存ボタンのクリックイベントハンドラ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonSave_Click( object sender, EventArgs e )
		{
			#region 設定したディレクトリを保存し画面を閉じる

			var regKey = Registry.CurrentUser.CreateSubKey(RegAppRoot + @"\PhotoDir");

			if (regKey == null)
				throw new InvalidOperationException();

			regKey.SetValue("Path1", _photoDir);

			Close();
			#endregion
		}

		/// <summary>
		/// 参照ボタンのクリックイベントハンドラ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonBrowseDir_Click( object sender, EventArgs e )
		{
			#region ディレクトリをユーザに選択させる
			using (var dlg = new FolderBrowserDialog())
			{
				dlg.Description = @"スクリーンセーバーに表示する画像があるフォルダーを選んでから[OK]をクリックしてください。";
				dlg.ShowNewFolderButton = false;
				dlg.SelectedPath = _photoDir;

				if (dlg.ShowDialog(this) == DialogResult.OK)
				{
					#region OKをクリックした場合はそれを保存し、画面に表示する
					_photoDir = dlg.SelectedPath;
					labelDirName.Text = Path.GetFileName(_photoDir);
					#endregion
				}
			}
			#endregion
		}

		/// <summary>
		/// キャンセルボタンのクリックイベントハンドラ
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonCancel_Click( object sender, EventArgs e )
		{
			#region キャンセルをクリックした場合は保存せずに閉じる
			Close();
			#endregion
		}
	}
}
