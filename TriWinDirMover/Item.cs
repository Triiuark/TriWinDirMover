using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TriWinDirMover
{
	internal class Item : Directory, IComparable
	{
		private DirectorySet DirectorySet;
		private bool IsDisabledValue;
		private Settings Settings;

		public Item(DirectoryInfo directoryInfo, Settings settings) : base(directoryInfo)
		{
			Settings = settings;
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

			CalculateSize();

			PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
			{
				if (Size > -1)
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
				}
			};
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
					StopCalculateSize();
					Settings.DisabledItems.Add(Info.FullName);
				}
				else
				{
					CalculateSize();
					Settings.DisabledItems.Remove(Info.FullName);
				}
			}
		}

		public new void CalculateSize()
		{
			if (Settings.CalculateSizes && !IsDisabled)
			{
				base.CalculateSize();
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
	}
}
