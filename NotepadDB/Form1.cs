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
using FastColoredTextBoxNS;
using System.Text.RegularExpressions;

namespace NotepadDB
{
    public partial class Form_NotepadDB : Form
    {
        private string currentFilename;
        private string currentExtension;

        public Form_NotepadDB()
        {
            InitializeComponent();

            comboBox_Extension.Items.AddRange(new string[] { ".txt", ".xml", ".json" } );
            comboBox_Extension.SelectedIndex = 0;

            UpdateStatusLabel("Open a file from a disk or a database.");
        }

        private void UpdateStatusLabel(string text)
        {
            toolStripStatusLabel_Status.Text = text;
        }

        private void UpdateFileNameInput()
        {
            textBox_FileName.Text = currentFilename;
        }

        private void UpdateExtension()
        {
            switch (currentExtension)
            {
                case ".xml":
                    textBox.Language = FastColoredTextBoxNS.Language.XML;
                    comboBox_Extension.SelectedIndex = 1;
                    break;
                case ".json":
                    textBox.Language = FastColoredTextBoxNS.Language.JS;
                    comboBox_Extension.SelectedIndex = 2;
                    break;
                case ".txt":
                    textBox.Language = FastColoredTextBoxNS.Language.Custom;
                    comboBox_Extension.SelectedIndex = 0;
                    break;
                default:
                    return;
            }
        }

        private bool CheckIfFilenameInputEmpty()
        {
            if (String.IsNullOrWhiteSpace(currentFilename))
            {
                MessageBox.Show("Please enter a file name.", "Error");
                return true;
            }
            return false;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = openFileDialog.ShowDialog();
            
            if (dialogResult == DialogResult.OK)
            {
                currentFilename = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                currentExtension = Path.GetExtension(openFileDialog.FileName);
                textBox_FileName.Text = currentFilename;

                try
                {
                    OpenFile(openFileDialog.FileName);
                    UpdateStatusLabel("File opened.");
                }
                catch
                {
                    MessageBox.Show("Failed to load the file.", "Error");
                }
            }

            void OpenFile(string fileName)
            {
                textBox.Text = File.ReadAllText(fileName);

                UpdateExtension();
            }
        }

        private async void saveToDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckIfFilenameInputEmpty())
                return;

            try
            {
                await Task.Run(() =>
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

                    UpdateStatusLabel("Saving file to DB...");

                    documentContext.Documents.AddOrUpdate(document);
                    documentContext.SaveChanges();

                    UpdateStatusLabel("File saved to DB.");
                        
                    }
                });
            }
            catch
            {
                MessageBox.Show("Failed to save to the database.", "Error");
                UpdateStatusLabel("Failed to save to the database.");
            }
        }

        private async void openFromDBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() =>
                {
                    using (FormFileList formFileList = new FormFileList())
                    {
                        formFileList.PopulateListBoxAsync();
                        if (formFileList.ShowDialog() == DialogResult.Cancel)
                            return;

                        currentFilename = Path.GetFileNameWithoutExtension(formFileList.SelectedFile);
                        currentExtension = Path.GetExtension(formFileList.SelectedFile);
                    }
                });
            }
            catch
            {
                MessageBox.Show("Error opening the file from the database.", "Error");
                UpdateStatusLabel("Error opening the file from the database.");
                return;
            }

            UpdateFileNameInput();
            UpdateExtension();

            UpdateStatusLabel("Opening file from DB...");
            List<string> documents = new List<string>();

            try
            {
                await Task.Run(() =>
                {
                    using (DocumentContext documentContext = new DocumentContext())
                    {
                        documents =
                            documentContext.Documents
                            .Where(d => d.Name == currentFilename && d.Extension == currentExtension)
                            .Select(d => d.Contents)
                            .ToList();
                    }
                });
            }
            catch
            {
                MessageBox.Show("Error opening the file from the database.", "Error");
                UpdateStatusLabel("Error opening the file from the database.");
                currentFilename = String.Empty;
                UpdateFileNameInput();
                return;
            }

            if (documents.Count > 0)
            {
                textBox.Text = documents[0];
                UpdateStatusLabel("File opened from DB.");
            }
            else
            {
                UpdateStatusLabel("The database has no files to open.");
            }
        }

        private void ComboBox_Extension_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox_Extension.SelectedIndex)
            {
                case 1:
                    textBox.Language = FastColoredTextBoxNS.Language.XML;
                    ChangeHighlighting(comboBox_Extension, textBox);
                    break;
                case 2:
                    textBox.Language = FastColoredTextBoxNS.Language.JS;
                    ChangeHighlighting(comboBox_Extension, textBox);
                    break;
                case 0:
                    textBox.Language = FastColoredTextBoxNS.Language.Custom;
                    ChangeHighlighting(comboBox_Extension, textBox);
                    break;
                default:
                    return;
            }

            void ChangeHighlighting(ComboBox comboBox, FastColoredTextBox textBox)
            {
                currentExtension = comboBox.Text;

                string text = textBox.Text;
                textBox.Clear();
                textBox.Text = text;
            }
        }

        private void TextBox_FileName_TextChanged(object sender, EventArgs e)
        {
            if (Regex.Match(textBox_FileName.Text, @"(<|>|:|""|/|\\|\||\?|\*)?").Success)
            {
                textBox_FileName.Text = Regex.Replace(textBox_FileName.Text, @"(<|>|:|""|/|\\|\||\?|\*)", "");
                textBox_FileName.SelectionStart = textBox_FileName.Text.Length;
            }

            currentFilename = textBox_FileName.Text.Trim();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|XML Files (*.xml)|*.xml|JSON Files (*.json)|*.json";
            saveFileDialog.InitialDirectory = Environment.CurrentDirectory;
            saveFileDialog.FilterIndex = comboBox_Extension.SelectedIndex + 1;
            saveFileDialog.FileName = currentFilename;
            DialogResult dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, textBox.Text);
                    UpdateStatusLabel("File saved.");
                }
                catch
                {
                    MessageBox.Show("Error saving the file.", "Error");
                    UpdateStatusLabel("Error saving the file.");
                }
            }
        }
    }
}
