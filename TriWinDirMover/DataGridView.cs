using System.Drawing;
using System.Windows.Forms;

namespace TriWinDirMover
{
    class DataGridView : System.Windows.Forms.DataGridView
    {
        public DataGridView()
        {
            DefaultCellStyle.Font = new Font(FontFamily.GenericMonospace, 9);
            BorderStyle = BorderStyle.None;
            BackgroundColor = Color.White;
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            Dock = DockStyle.Fill;

            //CellPainting += DataGridView_CellPainting;
        }

        public void ShowFolderBrowser(int rowIndex, int columnIndex)
        {
            object value = Rows[rowIndex].Cells[columnIndex].Value;
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (value != null)
            {
                folderBrowserDialog.SelectedPath = value.ToString();
            }

            DialogResult result = folderBrowserDialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                Rows[rowIndex].Cells[columnIndex].Value = folderBrowserDialog.SelectedPath;
            }
        }

        private void DataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            e.Handled = true;
            Color Color = e.CellStyle.BackColor;
            if ((e.State & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
            {
                Color = e.CellStyle.SelectionBackColor;
            }

            using (Brush brush = new SolidBrush(Color))
            {
                e.Graphics.FillRectangle(brush, e.CellBounds);
            }
            using (Pen pen = new Pen(Brushes.DarkGray))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
                e.Graphics.DrawRectangle(pen, e.CellBounds);
            }
            e.PaintContent(e.ClipBounds);
        }
    }
}
