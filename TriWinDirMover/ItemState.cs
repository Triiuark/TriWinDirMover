using System.Collections.Generic;

namespace TriWinDirMover
{
	internal class ItemState
	{
		private List<string> DirectoryList;
		private List<string> FileList;
		private object Lock;

		public ItemState()
		{
			Lock = new object();
			DirectoryList = new List<string>();
			FileList = new List<string>();
			Reset();
		}

		public long Directories
		{
			get;
			private set;
		}

		public long Files
		{
			get;
			private set;
		}

		public long Size
		{
			get;
			private set;
		}

		public int TotalDirectories
		{
			get;
			private set;
		}

		public int TotalFiles
		{
			get;
			private set;
		}

		public long TotalSize
		{
			get;
			private set;
		}

		public void AddDir(string directory)
		{
			lock (Lock)
			{
				DirectoryList.Add(directory);
				++TotalDirectories;
			}
		}

		public void AddFile(string file)
		{
			lock (Lock)
			{
				FileList.Add(file);
				++TotalFiles;
			}
		}

		public void AddSize(long size)
		{
			lock (Lock)
			{
				Size += size;
			}
		}

		public void AddToTotalSize(long size)
		{
			lock (Lock)
			{
				TotalSize += size;
			}
		}

		public void Clear()
		{
			lock (Lock)
			{
				DirectoryList.Clear();
				FileList.Clear();
			}
		}

		public void DirectoryCreated()
		{
			lock (Lock)
			{
				++Directories;
			}
		}

		public void FileCopied()
		{
			lock (Lock)
			{
				++Files;
			}
		}

		public List<string> getDirectoryList()
		{
			lock (Lock)
			{
				return DirectoryList;
			}
		}

		public List<string> GetFileList()
		{
			lock (Lock)
			{
				return FileList;
			}
		}

		public void Reset()
		{
			lock (Lock)
			{
				TotalSize = SizeValue.NotCalculated;
				TotalDirectories = 0;
				TotalFiles = 0;
				Size = 0;
				DirectoryList.Clear();
				FileList.Clear();
			}
		}

		public struct SizeValue
		{
			public const long Error = -2;
			public const long NotCalculated = -1;
		}
	}
}
