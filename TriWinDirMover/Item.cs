using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TriWinDirMover
{
	internal class Item : Directory, IComparable
	{
		private static int MAX_READ_BYTES = 10 * 1024 * 1024;
		private static int MAX_THREADS = 32;

		private BackgroundWorker CopyBackgroundWorker;
		private DirectorySet DirectorySet;
		private bool IsDisabledValue;
		private Settings Settings;
		private BackgroundWorker SizeBackgroundWorker;

		public Item(DirectoryInfo directoryInfo, Settings settings) : base(directoryInfo)
		{
			State = new ItemState();
			Settings = settings;
			IsSizeCalculating = true;
			IsDisabledValue = Settings.DisabledItems.Contains(Info.FullName);
			DirectorySet = Settings.GetDirectorySet(Path);
			if (DirectorySet == null)
			{
				HasError = true;
				Error = "Missing DirectorySet";
			}

			if (!IsSymLink)
			{
				Target = DirectorySet?.Target.FullName;
			}

			if (Target == null)
			{
				Target = "";
			}

			PropertyChanged += Item_PropertyChanged;
			CalculateSize();
		}

		public double AverageFileSize
		{
			get
			{
				double size = 0;
				if (State.TotalFiles > 0)
				{
					size = State.TotalSize / State.TotalFiles;
				}

				return size;
			}
		}

		public string HumanReadableAverageFileSize
		{
			get
			{
				if (IsDisabled)
				{
					return "";
				}
				return ToHumanReadableSize(AverageFileSize);
			}
		}

		public string HumanReadableSize
		{
			get
			{
				return ToHumanReadableSize(State.TotalSize);
			}
		}

		public bool IsDefaultTarget
		{
			get
			{
				bool result;
				if (IsSymLink)
				{
					result = Target.Equals(DirectorySet?.Target.FullName + "\\" + Name);
				}
				else
				{
					result = Target.Equals(DirectorySet?.Target.FullName);
				}
				return result;
			}
		}

		public bool IsDisabled
		{
			get
			{
				return IsDisabledValue;
			}
			set
			{
				IsDisabledValue = value;
				OnPropertyChanged("IsDisabled");
				if (IsDisabledValue)
				{
					//StopCalculateSize();
					Settings.DisabledItems.Add(Info.FullName);
				}
				else
				{
					CalculateSize();
					Settings.DisabledItems.Remove(Info.FullName);
				}
			}
		}

		public bool IsSizeCalculating
		{
			get;
			private set;
		}

		public int NumberOfDirectories
		{
			get
			{
				return State.TotalDirectories;
			}
		}

		public int NumberOfFiles
		{
			get
			{
				return State.TotalFiles;
			}
		}

		public long Size
		{
			get
			{
				return State.TotalSize;
			}
		}

		public ItemState State
		{
			get;
			private set;
		}

		public static string ToHumanReadableSize(double size)
		{
			string result = "";

			if (size == ItemState.SizeValue.Error)
			{
				result = Properties.Strings.Error;
			}
			else if (size >= 0)
			{
				int count = 0;
				string[] units = new string[] { "B  ", "KiB", "MiB", "GiB", "TiB" };
				while (size > 1024.0 && ++count < 5)
				{
					size /= 1024.0;
				}

				result = size.ToString("0.00") + " " + units[count];
			}

			return result;
		}

		public void CalculateSize()
		{
			if (!IsDisabled && Settings.CalculateSizes)
			{
				if (SizeBackgroundWorker == null)
				{
					SizeBackgroundWorker = new BackgroundWorker();
					SizeBackgroundWorker.WorkerSupportsCancellation = true;
					SizeBackgroundWorker.DoWork += SizeBackgroundWorker_DoWork;
					SizeBackgroundWorker.RunWorkerCompleted += SizeBackgroundWorker_RunWorkerCompleted;
				}
				IsSizeCalculating = true;
				SizeBackgroundWorker.RunWorkerAsync(this);
			}
		}

		public int CompareTo(object obj, string propertyName)
		{
			if (obj == null)
			{
				return 1;
			}

			Item item = obj as Item;
			if (item == null)
			{
				throw new ArgumentException("Object is not an Item");
			}

			int result = 0;
			switch (propertyName)
			{
				case "Path":
					result = Path.CompareTo(item.Path);
					break;

				case "Size":
				case "HumanReadableSize":
					result = Size.CompareTo(item.Size);
					break;

				case "AverageFileSize":
				case "HumanReadableAverageFileSize":
					result = AverageFileSize.CompareTo(item.AverageFileSize);
					break;

				case "NumberOfFiles":
					result = NumberOfFiles.CompareTo(item.NumberOfFiles);
					break;

				case "NumberOfDirectories":
					result = NumberOfDirectories.CompareTo(item.NumberOfDirectories);
					break;

				case "Name":
					result = Name.CompareTo(item.Name);
					break;

				case "Target":
					result = Target.CompareTo(item.Target);
					break;

				case "IsSymLink":
					result = IsSymLink.CompareTo(item.IsSymLink);
					break;

				case "IsDisabled":
					result = IsDisabled.CompareTo(item.IsDisabled);
					break;
			}

			if (result == 0)
			{
				if (propertyName == null || !propertyName.Equals("Path"))
				{
					result = Path.CompareTo(item.Path);
				}
				if (result == 0)
				{
					result = Info.FullName.CompareTo(item.FullName);
				}
			}

			return result;
		}

		public int CompareTo(object obj)
		{
			return CompareTo(obj, null);
		}

		public void CopyBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			string source;
			string target;

			if (IsSymLink)
			{
				source = Target;
				target = Regex.Replace(Path + "\\" + Name, "\\\\", "\\");
			}
			else
			{
				source = FullName;
				target = Regex.Replace(Target + "\\" + Name, "\\\\", "\\");
			}

			ParallelOptions parallelOptions = new ParallelOptions();
			parallelOptions.MaxDegreeOfParallelism = MAX_THREADS;

			State.Reset();
			CalculateMySize(new DirectoryInfo(source), parallelOptions);

			if (new DirectoryInfo(target).Exists)
			{
				if (IsSymLink)
				{
					target += ".bak";
				}
				else
				{
					return;
				}
			}

			Parallel.ForEach(State.getDirectoryList(), parallelOptions, (directory) =>
			{
				System.IO.Directory.CreateDirectory(
					directory.Replace(source, target));
				State.DirectoryCreated();
			});

			Parallel.ForEach(State.GetFileList(), parallelOptions, (file) =>
			{
				long size = new FileInfo(file).Length;
				string newFile = file.Replace(source, target);
				byte[] bytes = new byte[MAX_READ_BYTES];

				FileStream s = File.OpenRead(file);
				FileStream t = File.Create(newFile);

				int bytesRead = 0;
				while ((bytesRead = s.Read(bytes, 0, MAX_READ_BYTES)) > 0)
				{
					t.Write(bytes, 0, bytesRead);
					State.AddSize(bytesRead);
				}
				s.Close();
				t.Close();
				State.FileCopied();
			});

			State.Clear();
		}

		public void MoveItem()
		{
			if (CopyBackgroundWorker == null)
			{
				CopyBackgroundWorker = new BackgroundWorker();
				CopyBackgroundWorker.WorkerSupportsCancellation = true;
				CopyBackgroundWorker.DoWork +=
					CopyBackgroundWorker_DoWork;
				CopyBackgroundWorker.RunWorkerCompleted +=
					CopyBackgroundWorker_RunWorkerCompleted;
			}
			CopyBackgroundWorker.RunWorkerAsync(this);
		}

		private void CalculateMySize(DirectoryInfo source, ParallelOptions parallelOptions)
		{
			Parallel.ForEach(source.GetDirectories(), parallelOptions, (dir) =>
			{
				State.AddDir(dir.FullName);
				CalculateMySize(dir, parallelOptions);
			});

			Parallel.ForEach(source.GetFiles(), parallelOptions, (file) =>
			{
				State.AddFile(file.FullName);
				State.AddToTotalSize(new FileInfo(file.FullName).Length);
			});
		}

		private void CopyBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
		}

		private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Size >= 0)
			{
				DriveInfo driveInfo;
				if (IsSymLink)
				{
					driveInfo = new DriveInfo(DirectorySet.Source.Root);
				}
				else
				{
					driveInfo = new DriveInfo(Target);
				}

				if (driveInfo.AvailableFreeSpace < Size)
				{
					HasError = true;
					Error = "Not enough space to move directory.";
				}
				else if (Error.Equals("Not enough space to move directory."))
				{
					HasError = false;
					Error = "";
				}
			}
			if (e.PropertyName.Equals("Size"))
			{
				OnPropertyChanged("HumanReadableSize");
			};
		}

		private void SizeBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			ParallelOptions opt = new ParallelOptions();
			opt.MaxDegreeOfParallelism = MAX_THREADS;

			State.Reset();
			CalculateMySize(Info, opt);
			State.Clear();
		}

		private void SizeBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			State.Clear();
			IsSizeCalculating = false;
			OnPropertyChanged("Size");
		}

		/*
		public void Move(IWin32Window parent)
		{
			if (HasError)
			{
				return;
			}

			string source;
			string destination;
			string[] cmds;
			string silent = "";// "/Q";
			if (IsSymLink)
			{
				source = Target;
				destination = Regex.Replace(Path + "\\" + Name, "\\\\", "\\");
				cmds = new string[3];
				cmds[0] = "rd /Q \"" + destination + "\""; // remove existing symlink
				cmds[1] = "xcopy /E /H /I " + silent + " /V /Y \"" + source + "\" \"" + destination + "\""; // copy back to original folder
				cmds[2] = "rd /S /Q \"" + source + "\""; // remove old source
			}
			else
			{
				source = FullName;
				destination = Regex.Replace(Target + "\\" + Name, "\\\\", "\\");
				cmds = new string[3];
				cmds[0] = "xcopy /E /H /I " + silent + " /V /Y \"" + source + "\" \"" + destination + "\""; // copy to new folder
				cmds[1] = "rd /S /Q \"" + source + "\""; // delete original
				cmds[2] = "mklink /D \"" + source + "\" \"" + destination + "\""; // create symlink
			}

			if (!MainForm.RunCmds(parent, cmds, Settings.KeepCmdOpen, Settings.RunAsAdmin, true))
			{
				// TODO error msg?
			}

			Init(new DirectoryInfo(FullName));
			if (!IsSymLink)
			{
				Target = DirectorySet?.Target.FullName;
			}
			if (Target == null)
			{
				Target = "";
			}
			OnPropertyChanged("IsSymLink");
		}
		*/
	}
}
