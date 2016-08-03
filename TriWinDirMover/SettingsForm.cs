using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TriWinDirMover
{
	public partial class SettingsForm : Form
	{
		private DataTable mPreRunCommandsTable;
		private DataTable mSourcesTable;

		public static string ToJson(string[] stringList)
		{
			return JsonConvert.SerializeObject(stringList);
		}

		public static string[] FromJson(string json)
		{
			return JsonConvert.DeserializeObject<string[]>(json);
		}

		public static List<string[]> DirSetsFromJson(StringCollection jsonStrings)
		{

			List<string[]> dirSets = new List<string[]>();
			foreach (string json in jsonStrings)
			{
				string[] fromJson = FromJson(json);
				dirSets.Add(fromJson);
			}

			return dirSets;
		}

		public static List<string[]> GetDirSets()
		{
			if (Properties.Settings.Default.directorySets != null)
			{
				return DirSetsFromJson(Properties.Settings.Default.directorySets);
			}

			return null;
		}


		public SettingsForm()
		{
			InitializeComponent();

			this.dataGridView1.DefaultCellStyle.Font = new Font(FontFamily.GenericMonospace, 11);
			this.dataGridView2.DefaultCellStyle.Font = new Font(FontFamily.GenericMonospace, 11);

			GetData();
		}

		public void GetData()
		{
			mPreRunCommandsTable = new DataTable();
			mPreRunCommandsTable.Columns.Add("CMD", typeof(string));
			FillPreCommandsTable();
			
			dataGridView1.DataSource = mPreRunCommandsTable;
			dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			mPreRunCommandsTable.RowChanged += new DataRowChangeEventHandler(PreRunCommandChanged);

			mSourcesTable = new DataTable();
			mSourcesTable.Columns.Add("Source", typeof(string));
			mSourcesTable.Columns.Add("Target", typeof(string));
			FillSourcesTable();

			dataGridView2.DataSource = mSourcesTable;
			dataGridView2.Columns[dataGridView2.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			mSourcesTable.RowChanged += new DataRowChangeEventHandler(PreRunCommandChanged);
		}

		private void FillPreCommandsTable()
		{
			mPreRunCommandsTable.Rows.Clear();
			if (Properties.Settings.Default.preRunComands != null)
			{
				foreach (string cmd in Properties.Settings.Default.preRunComands)
				{
					mPreRunCommandsTable.Rows.Add(cmd);
				}
			}
		}

		private void FillSourcesTable()
		{
			mSourcesTable.Rows.Clear();
			if (Properties.Settings.Default.directorySets != null)
			{
				List<string[]> dirSets = DirSetsFromJson(Properties.Settings.Default.directorySets);

				foreach (string[] dirSet in dirSets)
				{
					if (dirSet.Length == 2)
					{
						mSourcesTable.Rows.Add(dirSet[0], dirSet[1]);
					}
					else if (dirSet.Length > 0)
					{
						mSourcesTable.Rows.Add(dirSet[0]);
					}
				}
			}
		}

		private void PreRunCommandChanged(object sender, DataRowChangeEventArgs args)
		{
			Console.WriteLine("Row_Changed Event: name={0}; action={1}", args.Row[0], args.Action);
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			StringCollection preRunCommands = new StringCollection();
			StringCollection directorySets  = new StringCollection();
			foreach (DataRow row in mPreRunCommandsTable.Rows)
			{
				try
				{
					string s = (string)row[0];
					if (s != null && s.Length > 0)
					{
						preRunCommands.Add(s);
					}
				}
				catch
				{
				}
			}
			foreach (DataRow row in mSourcesTable.Rows)
			{
				try
				{

					string s = (string)row[0];
					string t = (string)row[1];
					if (s != null && s.Length > 0)
					{
						string[] r = new string[] { s, t };

						string jsonString = ToJson(r);
						directorySets.Add(jsonString);
					}
				}
				catch
				{
				}
			}
			Properties.Settings.Default.preRunComands = preRunCommands;
			Properties.Settings.Default.directorySets = directorySets;
			Properties.Settings.Default.Save();
			FillPreCommandsTable();
		}

		private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex > -1 && e.RowIndex > -1)
			{
				object value = dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
				FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
				folderBrowserDialog1.SelectedPath = value.ToString();
				DialogResult result = folderBrowserDialog1.ShowDialog(this);
				if (result == DialogResult.OK)
				{
					dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = folderBrowserDialog1.SelectedPath;
				}

			}
		}
	}
}
