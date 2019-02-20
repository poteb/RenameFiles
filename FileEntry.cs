using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RenameFiles
{
    public class FileEntry : INotifyPropertyChanged
    {
        private string fullFilePath;
        private string fileName;
        private string output;

        public FileInfo FileInfo { get; private set; }
        public string Output { get { return output; } set { output = value; OnPropertyChanged(); } }
        public string Name { get { return fileName; } set { fileName = value; OnPropertyChanged(); } }
        public string FullPath { get { return fullFilePath; } set { fullFilePath = value; OnPropertyChanged(); } }
        public string Extension { get; set; }

        public FileEntry(FileInfo fi)
        {
            Reload(fi);
        }

        public void Reload(FileInfo fi)
        {
            FileInfo = fi;
            FullPath = fi.FullName;
            Name = fi.Name;
            var arr = fi.Name.Split('.');
            if (arr.Length > 1)
                Extension = $".{arr.Last()}";
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged

    }
}