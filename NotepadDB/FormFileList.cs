using Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NotepadDB
{
    public partial class FormFileList : Form
    {
        public string SelectedFile { get; private set; }

        public FormFileList()
        {
            InitializeComponent();

            PopulateListBoxAsync();
        }

        private async void PopulateListBoxAsync()
        {
            using (DocumentContext documentContext = new DocumentContext())
            {
                List<string> files = new List<string>();

                await Task.Run(() =>
                {
                    files = documentContext.Documents.Select(d => d.Name + d.Extension).ToList();
                });

                label_SelectFile.Text = "Select the file to open:";
                listBox_Files.Items.AddRange(files.ToArray());
            }
        }

        private void Button_OK_Click(object sender, EventArgs e)
        {
            SelectedFile = listBox_Files.Items[listBox_Files.SelectedIndex].ToString();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
