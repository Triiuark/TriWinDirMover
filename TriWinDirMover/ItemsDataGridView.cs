using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TriWinDirMover
{
	internal class ItemsDataGridView : DataGridView
	{
		private ItemList ItemList;
		private Settings Settings;

		public ItemsDataGridView(Settings settings)
		{
			Settings = settings;
			AddColumns();
			AllowUserToAddRows = false;
			AllowUserToDeleteRows = false;
			AutoGenerateColumns = false;
			CellClick += ItemsDataGridView_CellClick;
			CellDoubleClick += ItemsDataGridView_CellDoubleClick;
			CurrentCellDirtyStateChanged += ItemsDataGridView_CurrentCellDirtyStateChanged;
			DataBindingComplete += ItemsDataGridView_DataBindingComplete;
			CellMouseEnter += ItemsDataGridView_CellMouseEnter;

			ItemList = new ItemList();
			DataSource = new BindingSource();
			((BindingSource)DataSource).DataSource = ItemList;
			((BindingSource)DataSource).CurrentItemChanged += ItemsDataGridView_CurrentItemChanged;
		}

		public void GetData()
		{
			ItemList.Clear();

			foreach (DirectorySet dirSet in Settings.DirectorySets)
			{
				if (dirSet.Source.HasError)
				{
					MainForm.ShowErrorBox(this, dirSet.Source.Error);
					continue;
				}
				foreach (DirectoryInfo dir in dirSet.Source.GetDirectories())
				{
					ItemList.Add(new Item(dir, Settings));
				}
			}

			if (Settings.CalculateSizes)
			{
				Columns["Size"].Visible = true;
			}
			else
			{
				Columns["Size"].Visible = false;
			}

			if (Settings.ShowIsDisabled)
			{
				Columns["IsDisabled"].Visible = true;
			}
			else
			{
				Columns["IsDisabled"].Visible = false;
			}

		   ((BindingSource)DataSource).Sort = "Path asc";
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		private void AddColumns()
		{
			DataGridViewColumn col;

			col = new DataGridViewCheckBoxColumn();
			col.Name = "IsSymLink";
			col.DataPropertyName = "IsSymLink";
			col.HeaderText = Properties.Strings.MainFormItemsIsSymLink;
			col.SortMode = DataGridViewColumnSortMode.Automatic;
			Columns.Add(col);

			col = new DataGridViewTextBoxColumn();
			col.Name = "Path";
			col.DataPropertyName = "Path";
			col.HeaderText = Properties.Strings.MainFormItemsPath;
			Columns.Add(col);

			col = new DataGridViewTextBoxColumn();
			col.Name = "Name";
			col.DataPropertyName = "Name";
			col.HeaderText = Properties.Strings.MainFormItemsName;
			Columns.Add(col);

			col = new DataGridViewTextBoxColumn();
			col.Name = "Target";
			col.DataPropertyName = "Target";
			col.HeaderText = Properties.Strings.MainFormItemsTarget;
			col.MinimumWidth = 60;
			col.ReadOnly = true;
			Columns.Add(col);

			col = new DataGridViewTextBoxColumn();
			col.Name = "Size";
			col.DataPropertyName = "HumanReadableSize";
			col.HeaderText = Properties.Strings.MainFormItemsSize;
			col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
			Columns.Add(col);

			col = new DataGridViewCheckBoxColumn();
			col.Name = "IsDisabled";
			col.DataPropertyName = "IsDisabled";
			col.HeaderText = Properties.Strings.MainFormItemsIsDisabled;
			col.SortMode = DataGridViewColumnSortMode.Automatic;
			Columns.Add(col);
		}

		private void ApplyRowStyles(DataGridViewRow row)
		{
			Item item = (Item)row.DataBoundItem;

			Color foreColor = Color.Black;
			Color backColor = Color.White;

			if (item.IsDisabled)
			{
				foreColor = Color.Black;
				backColor = Color.LightGray;
			}
			else
			{
				foreColor = Color.Black;
				backColor = Color.White;
			}

			foreach (DataGridViewCell cell in row.Cells)
			{
				cell.Style.ForeColor = foreColor;
				cell.Style.BackColor = backColor;
			}

			if (!item.IsDisabled)
			{
				if (item.IsSymLink)
				{
					row.Cells["IsSymlink"].Style.BackColor = Color.LightGreen;

					if (item.IsDefaultTarget)
					{
						row.Cells["Target"].Style.BackColor = Color.LightGreen;
					}
					else
					{
						row.Cells["Target"].Style.BackColor = Color.LightYellow;
					}
				}
				else
				{
					if (!item.IsDefaultTarget)
					{
						row.Cells["Target"].Style.ForeColor = Color.DarkRed;
					}
				}
				Columns["Size"].ToolTipText = ItemList.SumSizes();
			}

			if (item.Size.Equals(Directory.SizeValue.Error))
			{
				row.Cells["Size"].Style.ForeColor = Color.Red;
			}

			if (item.HasError)
			{
				row.Cells["IsSymLink"].Style.BackColor = Color.LightPink;
				row.Cells["IsSymLink"].ToolTipText = item.Error;
			}

			Color selectionBackColor;
			int amount = 40;
			int r;
			int g;
			int b;
			foreach (DataGridViewCell cell in row.Cells)
			{
				r = cell.Style.BackColor.R < amount ? 0 : cell.Style.BackColor.R - amount;
				g = cell.Style.BackColor.G < amount ? 0 : cell.Style.BackColor.G - amount;
				b = cell.Style.BackColor.B;// < amount ? 0 : cell.Style.BackColor.G - amount;
				selectionBackColor = Color.FromArgb(255, r, g, b);
				cell.Style.SelectionForeColor = cell.Style.ForeColor;
				cell.Style.SelectionBackColor = selectionBackColor;
			}
		}

		private void ItemsDataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 0 && e.RowIndex > -1)
			{
				Item item = (Item)Rows[e.RowIndex].DataBoundItem;
				if (!item.IsDisabled)
				{
					item.Move(Parent);
				}
			}
		}

		private void ItemsDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex > -1 && e.RowIndex > -1)
			{
				Item item = (Item)Rows[e.RowIndex].DataBoundItem;
				switch (Columns[e.ColumnIndex].Name)
				{
					case "Target":
						if (item.IsSymLink)
						{
							ShowExplorer(item.Target);
						}
						else
						{
							ShowFolderBrowser(e.RowIndex, e.ColumnIndex);
						}
						break;

					case "Path":
						ShowExplorer(item.Path);
						break;

					case "Name":
						ShowExplorer(item.FullName);
						break;
				}
			}
		}

		private void ItemsDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex > -1)
			{
				ClearSelection();
				foreach (DataGridViewCell cell in Rows[e.RowIndex].Cells)
				{
					cell.Selected = true;
				}
			}
		}

		private void ItemsDataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			// to immediately submit changes to the Item objects
			CommitEdit(DataGridViewDataErrorContexts.Commit);
		}

		private void ItemsDataGridView_CurrentItemChanged(object sender, EventArgs e)
		{
			int index = ((BindingSource)sender).Position;
			if (index >= 0 && index < Rows.Count)
			{
				ApplyRowStyles(Rows[index]);
			}
		}

		private void ItemsDataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
		{
			// only way to get this correct working is this - iterate always over all the rows
			// RowsAdded does not work after BindingSource.Clear();
			foreach (DataGridViewRow row in Rows)
			{
				ApplyRowStyles(row);
			}
		}

		private void ShowExplorer(string target)
		{
			Process[] processes = Process.GetProcessesByName("explorer");
			foreach (Process p in processes)
			{
				if (p.MainWindowTitle.Equals(target, StringComparison.InvariantCultureIgnoreCase))
				{
					SetForegroundWindow(p.MainWindowHandle);
					return;
				}
			}

			Process process = new Process();
			process.StartInfo.FileName = "explorer";
			process.StartInfo.Arguments = target;
			process.Start();
		}
	}
}
