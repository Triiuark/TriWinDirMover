using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace TriWinDirMover
{
	class MainForm : Form
	{
		private Settings Settings;
		private SettingsForm SettingsForm;
		private ItemsDataGridView ItemsDataGridView;

		public MainForm()
		{
			Settings = Settings.Instance;
			InitForm();
			RunPreCommands();
			ItemsDataGridView.GetData();
		}

		private void InitForm()
		{
			ToolStripMenuItem settingsToolStripMenuItem = new ToolStripMenuItem();
			settingsToolStripMenuItem.Text = Properties.Strings.MainFormSettings;
			settingsToolStripMenuItem.Click += new EventHandler(this.SettingsToolStripMenuItem_Click);

			ToolStripMenuItem refreshToolStripMenuItem = new ToolStripMenuItem();
			refreshToolStripMenuItem.Text = Properties.Strings.MainFormRefresh;
			refreshToolStripMenuItem.Click += new EventHandler(this.RefreshToolStripMenuItem_Click);

			MainMenuStrip = new MenuStrip();
			MainMenuStrip.Items.Add(settingsToolStripMenuItem);
			MainMenuStrip.Items.Add(refreshToolStripMenuItem);

			ItemsDataGridView = new ItemsDataGridView(Settings);
			ItemsDataGridView.Columns["Target"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			ItemsDataGridView.RowHeadersVisible = false;

			Controls.Add(ItemsDataGridView);
			Controls.Add(MainMenuStrip);

			Text = Properties.Strings.MainForm;
			MinimumSize = new System.Drawing.Size(300, 100);
			Size = new System.Drawing.Size(1024, 768);
		}

		private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			((ItemsDataGridView)ItemsDataGridView).GetData();
		}

		private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SettingsForm == null)
			{
				SettingsForm = new SettingsForm(Settings);
			}

			SettingsForm.ShowDialog(this);
			if (SettingsForm.HasChanged)
			{
				RunPreCommands();
				((ItemsDataGridView)ItemsDataGridView).GetData();
			}
		}

		private void RunPreCommands()
		{
			if (Settings.PreCommands != null && Settings.PreCommands.Count > 0)
			{
				string[] cmds = new string[Settings.PreCommands.Count];
				Properties.Settings.Default.PreCommands.CopyTo(cmds, 0);
				RunCmds(this, cmds, Settings.KeepCmdOpen, Settings.RunPreCommandsAsAdmin, false);
			}
		}

		public static bool RunCmds(IWin32Window parent, string[] cmdList, bool keepCmdOpen, bool runAsAdmin, bool showDialog)
		{
			bool success = true;
			string[] cmds = Array.FindAll<string>(cmdList, x => x != null && x.Length > 0);
			string cmdDisplay = string.Join("\n\n", cmds);
			string cmdString = "/" + (keepCmdOpen ? "K" : "C")
				+ "(" + string.Join(") && (", cmds) + ")";

			DialogResult result = DialogResult.Yes;
			if (showDialog)
			{
				result = MessageBox.Show(parent,
					String.Format(Properties.Strings.MainFormRunProcessMessageConfirm, cmdDisplay),
					Properties.Strings.MainFormRunProcessMessageCaption,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);
			}

			if (result == DialogResult.Yes)
			{
				ProcessStartInfo processStartInfo = new ProcessStartInfo();
				processStartInfo.Arguments = cmdString;
				processStartInfo.FileName = "cmd.exe";
				if (runAsAdmin)
				{
					processStartInfo.Verb = "runas";
				}
				try
				{
					Process process = Process.Start(processStartInfo);
					process.WaitForExit();
					if (!keepCmdOpen && process.ExitCode != 0)
					{
						ShowErrorBox(parent, string.Format(
							Properties.Strings.MainFormCommandsFailed,
							cmdDisplay));
						success = false;
					}
					process.Close();
				}
				catch (Exception ex)
				{
					ShowErrorBox(parent,
						string.Format(Properties.Strings.MainFormProcessExecutionFailed, ex.Message));
					success = false;
				}
			}

			return success;
		}

		public static void ShowErrorBox(IWin32Window parent, string error)
		{
			MessageBox.Show(parent, error, Properties.Strings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}
