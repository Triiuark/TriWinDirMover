using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace TriWinDirMover
{
	internal class MainForm : Form
	{
		private ProgressBar ElementsProgressBar;
		private TextBox ElementsTextBox;
		private ItemsDataGridView ItemsDataGridView;
		private long MaxNonPagedMemory;
		private long MaxPagedMemory;
		private long MaxPhysicalMemory;
		private int MaxThreads;
		private long MaxVirtualMemory;
		private Settings Settings;
		private SettingsForm SettingsForm;
		private ProgressBar SizeProgressBar;
		private TextBox SizeTextBox;
		private TextBox StatsTextBox;

		public MainForm()
		{
			Settings = Settings.Instance;
			InitForm();
			RunPreCommands();
			ItemsDataGridView.GetData();
		}

		private delegate void SetElementsCallback(int value, string text);

		private delegate void SetSizeCallback(int value, string text);

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

		public void SetElementProgress(int value, string text)
		{
			if (ElementsProgressBar.InvokeRequired)
			{
				Invoke(new SetElementsCallback(SetElementProgress),
					new object[] { value, text });
			}
			else
			{
				ElementsTextBox.Text = text;
				ElementsProgressBar.Value = value;
			}
		}

		public void SetSizeProgress(int value, string text)
		{
			if (SizeProgressBar.InvokeRequired)
			{
				Invoke(new SetSizeCallback(SetSizeProgress),
					new object[] { value, text });
			}
			else
			{
				SizeTextBox.Text = text;
				SizeProgressBar.Value = value;
			}
		}

		private void InitForm()
		{
			StatsTextBox = new TextBox();
			StatsTextBox.ReadOnly = true;
			StatsTextBox.BorderStyle = BorderStyle.None;
			StatsTextBox.Dock = DockStyle.Bottom;

			ElementsTextBox = new TextBox();
			ElementsTextBox.ReadOnly = true;
			ElementsTextBox.Dock = DockStyle.Top;

			ElementsProgressBar = new ProgressBar();
			ElementsProgressBar.Value = 0;
			ElementsProgressBar.Maximum = 1000;
			ElementsProgressBar.Height = ElementsTextBox.Height;
			ElementsProgressBar.Dock = DockStyle.Top;

			SizeTextBox = new TextBox();
			SizeTextBox.ReadOnly = true;
			SizeTextBox.Dock = DockStyle.Top;

			SizeProgressBar = new ProgressBar();
			SizeProgressBar.Value = 0;
			SizeProgressBar.Maximum = 1000;
			SizeProgressBar.Height = SizeTextBox.Height;
			SizeProgressBar.Dock = DockStyle.Top;

			TableLayoutPanel tableLayout = new TableLayoutPanel();
			tableLayout.RowCount = 2;
			//tableLayout.ColumnCount = 2;
			tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent));
			tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent));
			tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			tableLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			tableLayout.ColumnStyles[0].Width = 65;
			tableLayout.ColumnStyles[1].Width = 35;
			tableLayout.Dock = DockStyle.Bottom;
			tableLayout.Controls.Add(ElementsProgressBar, 0, 0);
			tableLayout.Controls.Add(ElementsTextBox, 1, 0);
			tableLayout.Controls.Add(SizeProgressBar, 0, 1);
			tableLayout.Controls.Add(SizeTextBox, 1, 1);
			//tableLayout.Height = (int)(tableLayout.RowStyles[0].Height + tableLayout.RowStyles[1].Height + 1);
			tableLayout.Height = 3 * SizeTextBox.Height;
			Console.WriteLine(tableLayout.Height);
			Console.WriteLine(tableLayout.RowStyles[0].Height);

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
			Controls.Add(tableLayout);
			Controls.Add(StatsTextBox);

			Controls.Add(MainMenuStrip);

			Text = Properties.Strings.MainForm;
			MinimumSize = new System.Drawing.Size(300, 100);
			Size = new System.Drawing.Size(1024, 768);

			Timer timer = new Timer();
			timer.Interval = 1000;
			timer.Tick += (timerSender, timerEventArgs) =>
			{
				ProcessStats();
			};
			timer.Start();
		}

		private void ProcessStats()
		{
			Process current = Process.GetCurrentProcess();
			int threadCount = current.Threads.Count;
			long physicalMemory = current.WorkingSet64;
			long virtualMemory = current.VirtualMemorySize64;
			long nonPagedMemory = current.NonpagedSystemMemorySize64;
			long pagedMemory = current.PagedMemorySize64;

			MaxThreads = Math.Max(MaxThreads, threadCount);
			MaxPhysicalMemory = Math.Max(MaxPhysicalMemory, physicalMemory);
			MaxVirtualMemory = Math.Max(MaxVirtualMemory, virtualMemory);
			MaxNonPagedMemory = Math.Max(MaxNonPagedMemory, nonPagedMemory);
			MaxPagedMemory = Math.Max(MaxPagedMemory, pagedMemory);

			StatsTextBox.Text = "Threads: " + MaxThreads.ToString() + "/" + threadCount
				+ " | Physical: " + Item.ToHumanReadableSize(MaxPhysicalMemory) + "/" + Item.ToHumanReadableSize(physicalMemory)
				+ " | Virtual: " + Item.ToHumanReadableSize(MaxVirtualMemory) + "/" + Item.ToHumanReadableSize(virtualMemory)
				+ " | NonPaged: " + Item.ToHumanReadableSize(MaxNonPagedMemory) + "/" + Item.ToHumanReadableSize(nonPagedMemory)
				+ " | Paged: " + Item.ToHumanReadableSize(MaxPagedMemory) + "/" + Item.ToHumanReadableSize(pagedMemory);
		}

		private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			((ItemsDataGridView)ItemsDataGridView).GetData();
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
	}
}
