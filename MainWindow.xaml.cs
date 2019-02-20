using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace RenameFiles
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        private string format;
        private string extension;
        private bool typeRename = true;
        private bool typeReplace;
        private string regexFormat;
        private string regexReplaces;

        public bool TypeReplace { get => this.typeReplace; set { ResetTypeSelection(value); this.typeReplace = value; OnPropertyChanged(); } }
        public bool TypeRename { get => this.typeRename; set { ResetTypeSelection(value); this.typeRename = value; OnPropertyChanged(); } }
        public string Extension { get => this.extension; set { this.extension = value; OnPropertyChanged(); PreviewOutput(); } }
        public string Format { get => format; set { format = value; OnPropertyChanged(); PreviewOutput(); } }
        public string RegexFormat { get => this.regexFormat; set { this.regexFormat = value; OnPropertyChanged(); } }
        public string RegexReplaces { get => this.regexReplaces; set { this.regexReplaces = value; OnPropertyChanged(); } }
        public ObservableCollection<FileEntry> Files { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Files = new ObservableCollection<FileEntry>();
            DataContext = this;


        }

        private void ResetTypeSelection(bool value)
        {
            if (!value) return;
            TypeRename = false;
            TypeReplace = false;
        }

        private const string renamePattern = "(<[a-z]:[0-9]+>)";
        private const string replacePattern = "(<[a-z]:[^:]+:[^>]*>)";
        private void PreviewOutput()
        {
            if (this.tabSimple.IsSelected)
            {
                foreach (var file in Files)
                {
                    var output = this.format;
                    output += string.IsNullOrEmpty(this.extension) ? file.Extension : this.extension.PadLeft(this.extension.Length + 1, '.');
                    file.Output = output;
                }
                if (this.typeRename)
                {
                    var matches = Regex.Matches(this.format, renamePattern);
                    for (int i = matches.Count - 1; i >= 0; i--)
                    {
                        var match = matches[i];
                        var value = match.Value;
                        var type = value.TrimStart('<').TrimEnd('>');
                        var arr = type.Split(':');
                        if (arr.Length != 2) continue;
                        switch (arr[0])
                        {
                            case "i":
                                if (!this.typeRename) continue;
                                int val;
                                if (!int.TryParse(arr[1], out val)) continue;
                                var index = match.Index;
                                var length = match.Length;
                                foreach (var file in Files)
                                {
                                    file.Output = file.Output.Remove(index, length).Insert(index, val.ToString().PadLeft(arr[1].Length, '0'));
                                    val++;
                                }
                                break;
                        }
                    }
                }
                else if (this.typeReplace)
                {
                    var matches = Regex.Matches(this.format, replacePattern);
                    for (int i = matches.Count - 1; i >= 0; i--)
                    {
                        var match = matches[i];
                        var value = match.Value;
                        var type = value.TrimStart('<').TrimEnd('>');
                        var arr = type.Split(':');
                        if (arr.Length != 3) continue;
                        switch (arr[0])
                        {
                            case "r":
                                if (!this.typeReplace) continue;
                                string from = arr[1];
                                string to = arr[2];
                                foreach (var file in Files)
                                {
                                    file.Output = file.Name.Replace(from, to);
                                }
                                break;
                        }
                    }
                }
            }
            else if (this.tabRegex.IsSelected)
            {
                
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            try
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files == null) return;
                foreach (var file in files)
                {
                    if (Files.Any(x => x.FullPath == file)) continue;
                    var fi = new FileInfo(file);
                    Files.Add(new FileEntry(fi));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UIElement_OnDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void DgFiles_OnCopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {
            if (!e.ClipboardRowContent.Any()) return;
            var first = e.ClipboardRowContent[0];
            e.ClipboardRowContent.Clear();
            e.ClipboardRowContent.Add(first);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged

        private void bRename_Click(object sender, RoutedEventArgs e)
        {
            if (this.Files.Select(x => x.FileInfo.DirectoryName + "_" + x.Output).GroupBy(y => y).Any(z => z.Count() > 1))
            {
                MessageBox.Show("Unable to rename, there are duplicate filenames in the same directory.");
                return;
            }
            this.bRename.IsEnabled = false;
            try
            {
                foreach (var file in Files)
                {
                    if (file.FileInfo.DirectoryName == null) continue;
                    var newPath = Path.Combine(file.FileInfo.DirectoryName, file.Output);
                    File.Move(file.FullPath, newPath);
                    file.Reload(new FileInfo(newPath));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error");
            }
            this.bRename.IsEnabled = true;
        }
    }
}