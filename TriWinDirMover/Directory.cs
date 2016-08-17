using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace TriWinDirMover
{
	// Note: can't derive from sealed type System.IO.DirectoryInfo
	internal class Directory : INotifyPropertyChanged
	{
		protected DirectoryInfo Info;
		private string TargetValue;

		public Directory(string path) : this(new DirectoryInfo(path))
		{
		}

		public Directory(DirectoryInfo directoryInfo)
		{
			Init(directoryInfo);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public string Error
		{
			get;
			protected set;
		}

		public string FullName
		{
			get
			{
				return Info.FullName;
			}
		}

		public bool HasError
		{
			get;
			protected set;
		}

		public bool IsSymLink
		{
			get;
			protected set;
		}

		public string Name
		{
			get
			{
				return Info.Name;
			}
		}

		public string Path
		{
			get
			{
				return Info.Parent.FullName;
			}
		}

		public string Root
		{
			get
			{
				return Info.Root.FullName;
			}
		}

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

		public IEnumerable<DirectoryInfo> GetDirectories()
		{
			return Info.GetDirectories();
		}

		protected void Init(DirectoryInfo directoryInfo)
		{
			Info = directoryInfo;
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
			if ((Info.Attributes & FileAttributes.ReparsePoint) ==
				FileAttributes.ReparsePoint)
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

		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}
}
