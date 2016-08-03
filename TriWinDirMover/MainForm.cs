using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TriWinDirMover
{
	public partial class MainForm : Form
	{
		private Form settingsForm;
		private DataTable mDataTable;

		[Flags]
		private enum RunAs
		{
			None = 0x0,
			User = 0x1,
			Admin = 0x2
		}

		public MainForm()
		{
			InitializeComponent();
		
			mDataTable = new DataTable();
			mDataTable.Columns.Add("State", typeof(string));
			mDataTable.Columns.Add("Path", typeof(string));
			mDataTable.Columns.Add("Name", typeof(string));
			mDataTable.Columns.Add("Target", typeof(string));
			mDataTable.Columns.Add("Data", typeof(string));
			
			dataGridView1.DataSource = mDataTable;
			dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;
			dataGridView1.Columns["Data"].Visible = false;
			dataGridView1.DefaultCellStyle.Font = new Font(FontFamily.GenericMonospace, 11);
			dataGridView1.Columns[dataGridView1.Columns.Count - 2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			dataGridView1.Columns[0].DefaultCellStyle.Font = new Font(FontFamily.GenericMonospace, 12);
			dataGridView1.Columns[0].DefaultCellStyle.ForeColor = Color.White;
			dataGridView1.Columns[0].DefaultCellStyle.BackColor = Color.Green;
			dataGridView1.Columns[0].DefaultCellStyle.SelectionBackColor = Color.DarkGreen;

			RunPreCommands();
			RefreshAll();
		}

		private void RefreshAll()
		{
			GetData();
		}

		private void RunPreCommands()
		{
			/*
			StringCollection commands = new StringCollection();
			commands.Add("(cd /D R:/ 2>nul) || (net use R: \\\\<ip|name>\\raid /user:<username> <password> /persistent:yes >nul && cd /D R:/)");
			commands.Add("(cd /D S:/ 2>nul) || (net use R: \\\\<ip|name\\ssd /user:<username> <password> /persistent:yes >nul && cd /D S:/)");
			*/
			if (Properties.Settings.Default.preRunComands != null)
			{
				string[] cmds = new string[Properties.Settings.Default.preRunComands.Count];
				Properties.Settings.Default.preRunComands.CopyTo(cmds, 0);
				RunCmds(cmds, RunAs.User | RunAs.Admin, false);
			}
		}

		private void ShowSettings()
		{
			if (settingsForm == null)
			{
				settingsForm = new SettingsForm();
			}
			settingsForm.ShowDialog(this);
			RunPreCommands();
			RefreshAll();
		}

		private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
		{
			RefreshDataGridView();
		}

		private void GetData()
		{
			mDataTable.Clear();

			if (Properties.Settings.Default.directorySets == null)
			{
				return;
			}

			List<string[]> dirSets = SettingsForm.DirSetsFromJson(Properties.Settings.Default.directorySets);
			foreach (string[] dirSet in dirSets)
			{
				if (dirSet.Length < 1)
				{
					continue;
				}

				DirectoryInfo directoryInfo = new DirectoryInfo(dirSet[0]);
				DirectoryInfo[] dirs = directoryInfo.GetDirectories("*");
				foreach (DirectoryInfo dir in dirs)
				{
					object[] row = new object[]
					{
						"→",
						dirSet[0],
						dir.Name,
						dirSet[1],
						"default"
					};

					if ((dir.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
					{
						row[0] = "←";
						try
						{
							string path = SymLink.GetTarget(dir.FullName);
							DirectoryInfo pathInfo = new DirectoryInfo(path);

							if (pathInfo.Exists)
							{
								row[3] = path;
								row[4] = "symlink";
							}
							else
							{
								row[3] = "ERROR: " + dir.FullName;
								row[4] = "symlink error";
							}

							
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.StackTrace);
							row[3] = "ERROR: " + dir.FullName + ex.ToString();
							row[4] = "symlink error";
						}
					}
					mDataTable.Rows.Add(row);
				}
			}

			/*
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo info in drives)
			{
				table.Rows.Add(info.Name, info.DriveType, info.DriveFormat, info.VolumeLabel);
			}
			*/
		}

		private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ShowSettings();
		}

		private static string createPath(string path, string name)
		{
			string pattern = "\\$";
			string result = path;
			Regex.Replace(result, pattern, String.Empty);
			result += "\\" + name;

			return result;
		}

		private bool RunCmds(string[] cmdList, RunAs runAs, bool showDialog)
		{
			bool success = true;
			string[] cmds = Array.FindAll<string>(cmdList, x => x != null && x.Length > 0);
			string cmdString = "/C (" + string.Join(") && (", cmds) + ")";


			DialogResult result = DialogResult.Yes;
			if (showDialog)
			{
				string cmdDisplay = string.Join("'\n\n", cmds);
				result = MessageBox.Show(this,
				"Are you sure you want to run the following commands:\n\n" + cmdDisplay, "Caption",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			}

			if (result == DialogResult.Yes)
			{
				ProcessStartInfo myCmd = new ProcessStartInfo();
				myCmd.UseShellExecute = true;
				myCmd.FileName = "cmd.exe";
				if ((runAs & RunAs.Admin) == RunAs.Admin)
				{
					myCmd.Verb = "runas";
				}
				myCmd.Arguments = cmdString;
				try
				{
					Process cmd = Process.Start(myCmd);
					cmd.WaitForExit(1000);
					if (cmd.ExitCode != 0)
					{
						MessageBox.Show(this, "Commands failed.", "Game Links");
						success = false;
					}
				}
				catch
				{
					success = false;
				}


				if (success != false 
					&& (runAs & RunAs.Admin) == RunAs.Admin 
					&& (runAs & RunAs.User) == RunAs.User)
				{
					try
					{
						ProcessStartInfo myCmd2 = new ProcessStartInfo();
						myCmd2.UseShellExecute = true;
						myCmd2.FileName = "cmd.exe";
						myCmd2.Arguments = cmdString;
						Process cmd2 = Process.Start(myCmd2);
						cmd2.WaitForExit(1000);
						if (cmd2.ExitCode != 0)
						{
							MessageBox.Show(this, "Commands failed.", "Game Links");
							success = false;
						}
					}
					catch
					{
						success = false;
					}
				}
			}

			return success;
		}

		private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex > -1 && (e.ColumnIndex == 0 || e.ColumnIndex == 3))
			{
				DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

				if (e.ColumnIndex == 0 && !row.Cells["Data"].Value.ToString().Contains("error"))
				{
					string source;
					string destination;
					string[] cmds;
					if (row.Cells["Data"].Value.Equals("default"))
					{
						source = createPath(row.Cells["Path"].Value.ToString(), row.Cells["Name"].Value.ToString());
						destination = createPath(row.Cells["Target"].Value.ToString(), row.Cells["Name"].Value.ToString());
						cmds = new string[3];
						cmds[0] = "xcopy /E /H /I /Q /V /Y \"" + source + "\" \"" + destination + "\""; // copy to new folder
						cmds[1] = "rd /S /Q \"" + source + "\""; // delete original
						cmds[2] = "mklink /D \"" + source + "\" \"" + destination + "\""; // create symlink
					}
					else
					{
						source = row.Cells["Target"].Value.ToString();
						destination = createPath(row.Cells["Path"].Value.ToString(), row.Cells["Name"].Value.ToString());
						cmds = new string[3];
						cmds[0] = "rd /Q \"" + destination + "\""; // remove existing symlink
						cmds[1] = "xcopy /E /H /I /Q /V /Y \"" + source + "\" \"" + destination + "\""; // copy back to original folder
						cmds[2] = "rd /S /Q \"" + source + "\""; // remove old source
					}

					if (RunCmds(cmds, RunAs.Admin, true))
					{
						RefreshAll();
					}
					
				}
				else if (row.Cells["Data"].Value.Equals("default"))
				{
					object value = row.Cells[e.ColumnIndex].Value;
					FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
					folderBrowserDialog1.SelectedPath = value.ToString();
					folderBrowserDialog1.ShowDialog(this);
					row.Cells[e.ColumnIndex].Value = folderBrowserDialog1.SelectedPath;
					row.Cells[e.ColumnIndex].Selected = false;

					RefreshDataGridView();
				}
			}
		}

		private void RefreshDataGridView()
		{
			List<string[]> dirSets = SettingsForm.GetDirSets();

			foreach (DataGridViewRow row in dataGridView1.Rows)
			{
				if (row.Cells["Data"].Value.Equals("default"))
				{
					row.Cells["Target"].Style.ForeColor = Color.Blue;

					string target = createPath(row.Cells["Target"].Value.ToString(), row.Cells["Name"].Value.ToString());
					DirectoryInfo info = new DirectoryInfo(target);
					if (info.Exists)
					{
						row.Cells["State"].Style.BackColor = Color.Red;
						row.Cells["State"].Style.SelectionBackColor = Color.DarkRed;
					}
				}
				else if (row.Cells["Data"].Value.Equals("symlink"))
				{
					row.Cells["State"].Style.BackColor = Color.Blue;
					row.Cells["State"].Style.SelectionBackColor = Color.DarkBlue;
				}
				else if (row.Cells["Data"].Value.Equals("symlink error"))
				{
					row.Cells["State"].Style.BackColor = Color.Red;
					row.Cells["State"].Style.SelectionBackColor = Color.DarkRed;
				}
				if (row.Cells["Data"].Value.Equals("default"))
				{
					foreach (string[] dirSet in dirSets)
					{
						if (dirSet.Length > 0 && dirSet[0].Equals(row.Cells["Path"].Value))
						{
							if (dirSet.Length > 1 && dirSet[1].Equals(row.Cells["Target"].Value))
							{
								row.Cells["Target"].Style.ForeColor = Color.Blue;
							}
							else
							{
								row.Cells["Target"].Style.ForeColor = Color.Red;
							}
						}
					}
				}
				else if (row.Cells["Data"].Value.Equals("symlink"))
				{
					foreach (string[] dirSet in dirSets)
					{
						if (dirSet.Length > 0 && dirSet[0].Equals(row.Cells["Path"].Value))
						{
							if (dirSet.Length > 1 && row.Cells["Target"].Value.Equals(dirSet[1] + "\\" + row.Cells["Name"].Value))
							{
								row.Cells["Target"].Style.BackColor = Color.LightGreen;
								row.Cells["Target"].Style.SelectionBackColor = Color.Green;
							}
							else
							{
								row.Cells["Target"].Style.BackColor = Color.LightPink;
								row.Cells["Target"].Style.SelectionBackColor = Color.Red;
							}
						}
					}
				}
			}
		}

		private void dataGridView1_CellStyleChanged(object sender, DataGridViewCellEventArgs e)
		{
			Console.WriteLine("BNOWR");
			dataGridView1.Refresh();
		}

		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RefreshAll();
		}
	}
}
