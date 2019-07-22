using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Migrations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Storage;

namespace NotepadDB
{
    public partial class Form_NotepadDB : Form
    {
        private string currentFilename;
        private string currentExtension;

        public Form_NotepadDB()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = openFileDialog.ShowDialog();
            
            if (dialogResult == DialogResult.OK)
            {
                currentFilename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                currentExtension = Path.GetExtension(openFileDialog.FileName);
                OpenFile(openFileDialog.FileName);
            }
        }

        private void OpenFile(string fileName)
        {
            switch (currentExtension)
            {
                case ".xml":
                    textBox.Language = FastColoredTextBoxNS.Language.XML;
                    break;
                case ".json":
                    textBox.Language = FastColoredTextBoxNS.Language.JS;
                    break;
                case ".txt":
                    textBox.Language = FastColoredTextBoxNS.Language.Custom;
                    break;
                default:
                    return;
            }

            textBox.Clear();
            textBox.Text = File.ReadAllText(fileName);
        }

        private void saveToDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (DocumentContext documentContext = new DocumentContext())
            {
                if (documentContext.Documents.Any(d => d.Name == currentFilename && d.Extension == currentExtension))
                {
                    DialogResult dialogResult =
                        MessageBox.Show(
                            "The file with the same name has already been added to a database. Do you want to overwrite it?",
                            "Record exists",
                            MessageBoxButtons.YesNo);

                    if (dialogResult == DialogResult.No)
                        return;
                }

                Document document = new Document()
                {
                    Name = currentFilename,
                    Extension = currentExtension,
                    Contents = textBox.Text
                };

                documentContext.Documents.AddOrUpdate(document);
                documentContext.SaveChanges();
            }
        }

        private void openFromDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (DocumentContext documentContext = new DocumentContext())
            {
                var documents = documentContext.Documents.Where(d => d.Name == currentFilename && d.Extension == currentExtension).Select(d => d.Contents).ToList();
                if (documents.Count > 0)
                    textBox.Text = documents[0];
            }
        }
    }
}
