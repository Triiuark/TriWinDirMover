using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace TriWinDirMover
{
    class Settings
    {
        private static char DIRECTORY_SET_SPLIT_CHARACTER = '|';

        private static Settings InstanceValue;
        public static Settings Instance
        {
            get
            {
                if (InstanceValue == null)
                {
                    InstanceValue = new Settings();
                }
                return InstanceValue;
            }
        }

        public bool CalculateSizes;
        public bool ShowIsDisabled;
        public bool KeepCmdOpen;
        public bool RunAsAdmin;
        public bool RunPreCommandsAsAdmin;

        public StringCollection PreCommands
        {
            get;
            private set;
        }

        public HashSet<DirectorySet> DirectorySets
        {
            get;
            private set;
        }

        public HashSet<string> DisabledItems
        {
            get;
            private set;
        }

        private Settings()
        {
            Load();
        }

        ~Settings()
        {
            SetDisabledItems();
            Properties.Settings.Default.Save();
        }

        public void Load()
        {
            CalculateSizes = Properties.Settings.Default.CalculateSizes;
            ShowIsDisabled = Properties.Settings.Default.ShowIsDisabled;
            KeepCmdOpen = Properties.Settings.Default.KeepCmdOpen;
            RunAsAdmin = Properties.Settings.Default.RunAsAdmin;
            RunPreCommandsAsAdmin = Properties.Settings.Default.RunPreCommandsAsAdmin;
            PreCommands = Properties.Settings.Default.PreCommands != null ? Properties.Settings.Default.PreCommands : new StringCollection();
            DisabledItems = new HashSet<string>();
            if (Properties.Settings.Default.DisabledItems != null)
            {
                foreach (string s in Properties.Settings.Default.DisabledItems)
                {
                    DisabledItems.Add(s);
                }
            }

            DirectorySets = new HashSet<DirectorySet>();
            if (Properties.Settings.Default.DirectorySets != null)
            {
                foreach (string s in Properties.Settings.Default.DirectorySets)
                {
                    string[] dirSet = s.Split(DIRECTORY_SET_SPLIT_CHARACTER);
                    if (dirSet.Length > 1)
                    {
                        DirectorySets.Add(new DirectorySet(dirSet[0], dirSet[1]));
                    }
                }
            }
        }

        public void Save()
        {
            Properties.Settings.Default.ShowIsDisabled = ShowIsDisabled;
            Properties.Settings.Default.CalculateSizes = CalculateSizes;
            Properties.Settings.Default.KeepCmdOpen = KeepCmdOpen;
            Properties.Settings.Default.RunAsAdmin = RunAsAdmin;
            Properties.Settings.Default.RunPreCommandsAsAdmin = RunPreCommandsAsAdmin;
            Properties.Settings.Default.PreCommands = PreCommands;

            Properties.Settings.Default.DirectorySets = new StringCollection();
            foreach (DirectorySet dirSet in DirectorySets)
            {
                Properties.Settings.Default.DirectorySets.Add(dirSet.Source.FullName + DIRECTORY_SET_SPLIT_CHARACTER + dirSet.Target.FullName);
            }

            Properties.Settings.Default.Save();
        }

        private void SetDisabledItems()
        {
            Properties.Settings.Default.DisabledItems = new StringCollection();
            foreach (string s in DisabledItems)
            {
                Properties.Settings.Default.DisabledItems.Add(s);
            }
        }

        public DirectorySet GetDirectorySet(string source)
        {
            DirectorySet result = null;
            foreach (DirectorySet dirSet in DirectorySets)
            {
                if (dirSet.Source.FullName.Equals(source))
                {
                    result = dirSet;
                    break;
                }
            }

            return result;
        }
    }
}
