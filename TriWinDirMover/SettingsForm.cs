using System;
using System.Windows.Forms;

namespace TriWinDirMover
{
	internal class SettingsForm : Form
	{
		private CheckBox CalculateSizesCheckBox;
		private DataGridView DirectorySetsDataGridView;
		private CheckBox KeepCmdOpenCheckBox;
		private DataGridView PreCommandsDataGridView;
		private CheckBox RunAsAdminCheckBox;
		private CheckBox RunPreCommandsAsAdminCheckBox;
		private Settings Settings;
		private CheckBox ShowIsDisabledCheckBox;

		public SettingsForm(Settings settings)
		{
			Settings = settings;
		}

		public bool HasChanged
		{
			get;
			private set;
		}

		public new DialogResult ShowDialog(IWin32Window owner)
		{
			if (DirectorySetsDataGridView == null)
			{
				InitForm();
				AddColumns();
				GetData();
				MainMenuStrip.Enabled = false;
			}
			HasChanged = false;

			DialogResult result = base.ShowDialog(owner);

			return result;
		}

		private void AddColumns()
		{
			DirectorySetsDataGridView.Columns.Add("Source", Properties.Strings.SettingsFormDirectorySetsSource);
			DirectorySetsDataGridView.Columns.Add("Target", Properties.Strings.SettingsFormDirectorySetsTarget);
			PreCommandsDataGridView.Columns.Add("Command", Properties.Strings.SettingsFormPreCommandsCommand);

			DirectorySetsDataGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			PreCommandsDataGridView.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
		}

		private void DirectorySetsDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex > -1 && e.RowIndex > -1)
			{
				DirectorySetsDataGridView.ShowFolderBrowser(e.RowIndex, e.ColumnIndex);
			}
		}

		private void GetData()
		{
			DirectorySetsDataGridView.Rows.Clear();
			foreach (DirectorySet dirSet in Settings.DirectorySets)
			{
				DirectorySetsDataGridView.Rows.Add(new object[] { dirSet.Source.FullName, dirSet.Target.FullName });
			}
			PreCommandsDataGridView.Rows.Clear();
			foreach (string cmd in Settings.PreCommands)
			{
				PreCommandsDataGridView.Rows.Add(new object[] { cmd });
			}
			CalculateSizesCheckBox.Checked = Settings.CalculateSizes;
			ShowIsDisabledCheckBox.Checked = Settings.ShowIsDisabled;
			KeepCmdOpenCheckBox.Checked = Settings.KeepCmdOpen;
			RunAsAdminCheckBox.Checked = Settings.RunAsAdmin;
			RunPreCommandsAsAdminCheckBox.Checked = Settings.RunPreCommandsAsAdmin;
		}

		private void InitForm()
		{
			ToolStripMenuItem saveToolStripMenuItem = new ToolStripMenuItem(Properties.Strings.SettingsFormSave);
			saveToolStripMenuItem.Click += SaveToolStripMenuItem_Click;

			MainMenuStrip = new MenuStrip();
			MainMenuStrip.Items.Add(saveToolStripMenuItem);

			ShowIsDisabledCheckBox = new CheckBox();
			ShowIsDisabledCheckBox.Text = Properties.Strings.SettingsFormShowIsDisabled;
			ShowIsDisabledCheckBox.Dock = DockStyle.Top;

			CalculateSizesCheckBox = new CheckBox();
			CalculateSizesCheckBox.Text = Properties.Strings.SettingsFormSizes;
			CalculateSizesCheckBox.Dock = DockStyle.Top;

			KeepCmdOpenCheckBox = new CheckBox();
			KeepCmdOpenCheckBox.Text = Properties.Strings.SettingsFormKeepCmdOpen;
			KeepCmdOpenCheckBox.Dock = DockStyle.Top;

			RunAsAdminCheckBox = new CheckBox();
			RunAsAdminCheckBox.Text = Properties.Strings.SettingsFormRunAsAdmin;
			RunAsAdminCheckBox.Dock = DockStyle.Top;

			RunPreCommandsAsAdminCheckBox = new CheckBox();
			RunPreCommandsAsAdminCheckBox.Text = Properties.Strings.SettingsFormRunPreCommandsAsAdmin;
			RunPreCommandsAsAdminCheckBox.Dock = DockStyle.Top;

			TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
			tableLayoutPanel.RowCount = 3;
			tableLayoutPanel.ColumnCount = 1;
			tableLayoutPanel.Dock = DockStyle.Fill;
			tableLayoutPanel.Controls.Add(CalculateSizesCheckBox);
			tableLayoutPanel.Controls.Add(ShowIsDisabledCheckBox);
			tableLayoutPanel.Controls.Add(KeepCmdOpenCheckBox);
			tableLayoutPanel.Controls.Add(RunAsAdminCheckBox);
			tableLayoutPanel.Controls.Add(RunPreCommandsAsAdminCheckBox);

			PreCommandsDataGridView = new DataGridView();
			PreCommandsDataGridView.EditMode = DataGridViewEditMode.EditOnEnter;

			DirectorySetsDataGridView = new DataGridView();
			DirectorySetsDataGridView.CellClick += DirectorySetsDataGridView_CellClick;

			TabPage directorySetsTabPage = new TabPage();
			directorySetsTabPage.Controls.Add(DirectorySetsDataGridView);
			directorySetsTabPage.Text = Properties.Strings.SettingsFormDirectorySets;

			TabPage preCommandsTabPage = new TabPage();
			preCommandsTabPage.Text = Properties.Strings.SettingsFormPreCommands;
			preCommandsTabPage.Controls.Add(PreCommandsDataGridView);

			TabPage flagsTabPage = new TabPage();
			flagsTabPage.Text = Properties.Strings.SettingsFormFlags;
			flagsTabPage.Controls.Add(tableLayoutPanel);

			TabControl tabControl = new TabControl();
			tabControl.Dock = DockStyle.Fill;
			tabControl.Controls.Add(directorySetsTabPage);
			tabControl.Controls.Add(preCommandsTabPage);
			tabControl.Controls.Add(flagsTabPage);

			DirectorySetsDataGridView.CellValueChanged += SettingChanged;
			DirectorySetsDataGridView.RowsAdded += SettingChanged;
			DirectorySetsDataGridView.RowsRemoved += SettingChanged;
			PreCommandsDataGridView.CellValueChanged += SettingChanged;
			PreCommandsDataGridView.RowsAdded += SettingChanged;
			PreCommandsDataGridView.RowsRemoved += SettingChanged;

			ShowIsDisabledCheckBox.CheckedChanged += SettingChanged;
			CalculateSizesCheckBox.CheckedChanged += SettingChanged;
			KeepCmdOpenCheckBox.CheckedChanged += SettingChanged;
			RunAsAdminCheckBox.CheckedChanged += SettingChanged;
			RunPreCommandsAsAdminCheckBox.CheckedChanged += SettingChanged;

			Controls.Add(tabControl);
			Controls.Add(MainMenuStrip);

			Text = Properties.Strings.SettingsForm;
			MinimumSize = new System.Drawing.Size(300, 100);
			Size = new System.Drawing.Size(768, 300);
		}

		private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (MainMenuStrip.Enabled)
			{
				return;
			}

			DirectorySetsDataGridView.EndEdit();
			Settings.PreCommands.Clear();
			foreach (DataGridViewRow row in PreCommandsDataGridView.Rows)
			{
				string s = (string)row.Cells[0].Value;
				if (s != null && s.Length > 0)
				{
					Settings.PreCommands.Add(s);
				}
			}

			PreCommandsDataGridView.EndEdit();
			Settings.DirectorySets.Clear();
			foreach (DataGridViewRow row in DirectorySetsDataGridView.Rows)
			{
				string s = (string)row.Cells[0].Value;
				string t = (string)row.Cells[1].Value;
				if (s != null && s.Length > 0 && t != null && t.Length > 0)
				{
					Settings.DirectorySets.Add(new DirectorySet(s, t));
				}
			}

			Settings.CalculateSizes = CalculateSizesCheckBox.Checked;
			Settings.ShowIsDisabled = ShowIsDisabledCheckBox.Checked;
			Settings.KeepCmdOpen = KeepCmdOpenCheckBox.Checked;
			Settings.RunAsAdmin = RunAsAdminCheckBox.Checked;
			Settings.RunPreCommandsAsAdmin = RunPreCommandsAsAdminCheckBox.Checked;

			Settings.Save();

			GetData();

			MainMenuStrip.Enabled = false;
			HasChanged = true;
		}

		private void SettingChanged(object sender, EventArgs e)
		{
			MainMenuStrip.Enabled = true;
		}
	}
}
