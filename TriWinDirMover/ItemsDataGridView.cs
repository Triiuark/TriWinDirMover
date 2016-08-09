using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TriWinDirMover
{
    class ItemsDataGridView : DataGridView
    {
        private Settings Settings;
        private DataGridViewColumn CurrentSortedColumn;
        private ListSortDirection CurrentSortDirection = ListSortDirection.Ascending;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

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
            ColumnHeaderMouseClick += ItemsDataGridView_ColumnHeaderMouseClick;
            CellMouseEnter += ItemsDataGridView_CellMouseEnter;

            DataSource = new BindingSource();
            ((BindingSource)DataSource).DataSource = typeof(Item);
            ((BindingSource)DataSource).CurrentItemChanged += ItemsDataGridView_CurrentItemChanged;
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

        public override void Sort(DataGridViewColumn dataGridViewColumn, ListSortDirection direction)
        {
            BindingList<Item> items = (BindingList<Item>)((BindingSource)DataSource).List;
            List<Item> sorted = new List<Item>();
            foreach (Item item in items)
            {
                sorted.Add(item);
            }

            int dir = direction == ListSortDirection.Ascending ? 1 : -1;
            sorted.Sort((x, y) => dir * x.CompareTo(y, dataGridViewColumn.Name));

            ((BindingSource)DataSource).Clear();
            foreach (Item item in sorted)
            {
                ((BindingSource)DataSource).Add(item);
            }

            CurrentSortedColumn = dataGridViewColumn;
            CurrentSortDirection = direction;
            dataGridViewColumn.HeaderCell.SortGlyphDirection =
                dir == 1 ? SortOrder.Ascending : SortOrder.Descending;
        }

        private void ItemsDataGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridViewColumn col = Columns[e.ColumnIndex];
            ListSortDirection direction = ListSortDirection.Ascending;
            if (col.Equals(CurrentSortedColumn))
            {
                if (CurrentSortDirection == ListSortDirection.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }
            }
            Sort(col, direction);
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

        private void ItemsDataGridView_CurrentItemChanged(object sender, EventArgs e)
        {
            int index = ((BindingSource)sender).Position;
            if (index >= 0 && index < Rows.Count)
            {
                ApplyRowStyles(Rows[index]);
            }
        }

        private void ApplyRowStyles(DataGridViewRow row)
        {
            Item item = (Item)row.DataBoundItem;
            if (item.IsDisabled)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.ForeColor = Color.Black;
                    cell.Style.BackColor = Color.LightGray;
                }
            }
            else
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Style.ForeColor = Color.Black;
                    cell.Style.BackColor = Color.White;
                }

                if (item.IsSymLink)
                {
                    row.Cells["IsSymlink"].Style.BackColor = Color.LightGreen;

                    if (item.IsDefaultTarget)
                    {
                        row.Cells["Target"].Style.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        row.Cells["Target"].Style.BackColor = Color.LightPink;
                    }

                }
                else
                {
                    if (!item.IsDefaultTarget)
                    {
                        row.Cells["Target"].Style.ForeColor = Color.Red;
                    }
                }
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
            int amount = 40;
            foreach (DataGridViewCell cell in row.Cells)
            {
                int r = cell.Style.BackColor.R < amount ? 0 : cell.Style.BackColor.R - amount;
                int g = cell.Style.BackColor.G < amount ? 0 : cell.Style.BackColor.G - amount;
                int b = cell.Style.BackColor.B;// < amount ? 0 : cell.Style.BackColor.G - amount;
                Color backColor = Color.FromArgb(255, r, g, b);
                cell.Style.SelectionForeColor = cell.Style.ForeColor;
                cell.Style.SelectionBackColor = backColor;
            }
        }

        public void GetData()
        {
            ((BindingSource)DataSource).Clear();

            List<Item> items = new List<Item>();
            foreach (DirectorySet dirSet in Settings.DirectorySets)
            {
                if (dirSet.Source.HasError)
                {
                    MainForm.ShowErrorBox(this, dirSet.Source.Error);
                    continue;
                }
                foreach (DirectoryInfo dir in dirSet.Source.GetDirectories())
                {
                    items.Add(new Item(dir, Settings));
                }
            }
            items.Sort();
            foreach (Item item in items)
            {
                ((BindingSource)DataSource).Add(item);
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
            GC.Collect();
        }

        private void AddColumns()
        {
            DataGridViewColumn col;

            col = new DataGridViewCheckBoxColumn();
            col.Name = "IsSymLink";
            col.DataPropertyName = "IsSymLink";
            col.HeaderText = Properties.Strings.MainFormItemsIsSymLink;
            col.SortMode = DataGridViewColumnSortMode.Programmatic;
            Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.Name = "Path";
            col.DataPropertyName = "Path";
            col.HeaderText = Properties.Strings.MainFormItemsPath;
            col.SortMode = DataGridViewColumnSortMode.Programmatic;
            Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.Name = "Name";
            col.DataPropertyName = "Name";
            col.HeaderText = Properties.Strings.MainFormItemsName;
            col.SortMode = DataGridViewColumnSortMode.Programmatic;
            Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.Name = "Target";
            col.DataPropertyName = "Target";
            col.HeaderText = Properties.Strings.MainFormItemsTarget;
            col.MinimumWidth = 60;
            col.SortMode = DataGridViewColumnSortMode.Programmatic;
            col.ReadOnly = true;
            Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.Name = "Size";
            col.DataPropertyName = "HumanReadableSize";
            col.HeaderText = Properties.Strings.MainFormItemsSize;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.SortMode = DataGridViewColumnSortMode.Programmatic;
            Columns.Add(col);

            col = new DataGridViewCheckBoxColumn();
            col.Name = "IsDisabled";
            col.DataPropertyName = "IsDisabled";
            col.HeaderText = Properties.Strings.MainFormItemsIsDisabled;
            col.SortMode = DataGridViewColumnSortMode.Programmatic;
            Columns.Add(col);
        }

        private void ItemsDataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // to immediately submit changes to the Item objects
            CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }
}
