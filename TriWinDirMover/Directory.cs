using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
namespace TriWinDirMover
{
    // Note: can't derive from sealed type System.IO.DirectoryInfo
    public class Directory : INotifyPropertyChanged
    {
        public struct SizeValue
        {

            public const long NotCalculated = -1;
            public const long Calculating = -2;
            public const long Error = -3;
        }

        public bool IsSizeCalculated
        {
            get;
            protected set;
        }

        public long Size
        {
            get;
            protected set;
        }

        public string HumanReadableSize
        {
            get
            {
                return ToHumanReadableSize(Size);
            }
        }

        public static string ToHumanReadableSize(long size)
        {
            switch (size)
            {
                case SizeValue.NotCalculated:
                    return "";
                case SizeValue.Calculating:
                    return "...";
                case SizeValue.Error:
                    return Properties.Strings.Error;
            }

            int count = 0;
            double value = size;
            while (value > 1024.0 && ++count < 5)
            {
                value /= 1024.0;
            }

            string result = value.ToString("0.00");
            switch (count)
            {
                case 0:
                    result += " B  "; // keep spaces to right align
                    break;
                case 1:
                    result += " KiB";
                    break;
                case 2:
                    result += " MiB";
                    break;
                case 3:
                    result += " GiB";
                    break;
                case 4:
                default:
                    result += " TiB";
                    break;
            }

            return result;
        }

        public string Path
        {
            get
            {
                return Info.Parent.FullName;
            }
        }

        public string Name
        {
            get
            {
                return Info.Name;
            }
        }

        public string FullName
        {
            get
            {
                return Info.FullName;
            }
        }

        private string TargetValue;
        public string Target
        {
            get
            {
                return TargetValue;
            }
            set
            {
                if (!IsSymLink)
                {
                    TargetValue = value;
                    OnPropertyChanged("Target");
                }
            }
        }

        public bool IsSymLink
        {
            get;
            protected set;
        }

        public bool HasError
        {
            get;
            protected set;
        }

        public string Error
        {
            get;
            protected set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected DirectoryInfo Info;
        protected BackgroundWorker SizeBackgroundWorker;

        public Directory(string path) : this(new DirectoryInfo(path))
        {
        }

        public Directory(DirectoryInfo directoryInfo)
        {
            Init(directoryInfo);
        }

        protected void Init(DirectoryInfo directoryInfo)
        {
            Info = directoryInfo;
            Size = SizeValue.NotCalculated;
            IsSizeCalculated = false;
            HasError = false;
            Error = "";
            try
            {
                Info.GetFiles();
            }
            catch (Exception ex)
            {
                HasError = true;
                Error = ex.Message;
            }

            IsSymLink = false;
            TargetValue = Info.FullName;
            if ((Info.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
            {
                // TODO could also be something different
                IsSymLink = true;
                try
                {
                    TargetValue = SymLink.GetTarget(Info.FullName);
                }
                catch (Exception ex)
                {
                    HasError = true;
                    Error = ex.Message;
                }
                /*
                var AccessList = System.IO.Directory.GetAccessControl(Info.FullName);
                var AccessRules = AccessList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                System.Console.WriteLine(AccessRules);
                foreach (FileSystemAccessRule rule in AccessRules)
                {
                    if ((rule.FileSystemRights & FileSystemRights.Read) != FileSystemRights.Read)
                    {
                        continue;
                    }
                    if (rule.AccessControlType == AccessControlType.Allow)
                    {
                        HasError = true;
                        break;
                    }
                }
                */
            }
        }

        ~Directory()
        {
            StopCalculateSize();
        }

        public IEnumerable<DirectoryInfo> GetDirectories()
        {
            return Info.GetDirectories();
        }

        public void CalculateSize()
        {
            if (HasError)
            {
                IsSizeCalculated = true;
                Size = SizeValue.Error;
                OnPropertyChanged("Size");
                return;
            }

            IsSizeCalculated = false;
            Size = SizeValue.Calculating;
            if (SizeBackgroundWorker == null)
            {
                SizeBackgroundWorker = new BackgroundWorker();
                SizeBackgroundWorker.WorkerSupportsCancellation = true;
                SizeBackgroundWorker.DoWork += SizeBackgroundWorker_DoWork;
                SizeBackgroundWorker.RunWorkerCompleted += SizeBackgroundWorker_RunWorkerCompleted;
                SizeBackgroundWorker.RunWorkerAsync(this);
            }
        }

        public void StopCalculateSize()
        {
            SizeBackgroundWorker?.CancelAsync();
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void SizeBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Directory dir = (Directory)e.Argument;
            long result = 0;
            try
            {
                foreach (string f in System.IO.Directory.GetFiles(dir.FullName, "*", SearchOption.AllDirectories))
                {
                    if (((BackgroundWorker)sender).CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }
                    result += (new FileInfo(f)).Length;
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                result = SizeValue.Error;
            }
            e.Result = result;
        }

        private void SizeBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                Size = (long)e.Result;
                if (Size == SizeValue.Error)
                {
                    HasError = true;
                }
                IsSizeCalculated = true;
            }
            else
            {
                Size = SizeValue.NotCalculated;
                IsSizeCalculated = false;
            }

            OnPropertyChanged("Size");
        }
    }
}
