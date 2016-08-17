using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TriWinDirMover
{
	internal class ItemList : BindingList<Item>
	{
		private bool IsSortedCoreValue;
		private ListSortDirection SortDirectionCoreValue;
		private PropertyDescriptor SortPropertyCoreValue;

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

		protected override bool IsSortedCore
		{
			get
			{
				return IsSortedCoreValue;
			}
		}

		protected override ListSortDirection SortDirectionCore
		{
			get
			{
				return SortDirectionCoreValue;
			}
		}

		protected override PropertyDescriptor SortPropertyCore
		{
			get
			{
				return SortPropertyCoreValue;
			}
		}

		protected override bool SupportsSortingCore
		{
			get
			{
				return true;
			}
		}

		public string SumSizes()
		{
			List<Item> items = Items as List<Item>;
			TotalDirectories = 0;
			TotalFiles = 0;
			TotalSize = 0;
			bool isReady = true;
			if (items != null)
			{
				foreach (Item item in Items)
				{
					if (!item.IsDisabled)
					{
						if (!item.IsSizeCalculating)
						{
							TotalSize += item.Size;
							TotalDirectories += item.NumberOfDirectories;
							TotalFiles += item.NumberOfFiles;
						}
						else
						{
							isReady = false;
						}
					}
				}
			}
			return Item.ToHumanReadableSize(TotalSize) + (isReady ? "" : " ...");
		}

		protected override void ApplySortCore(PropertyDescriptor property,
									ListSortDirection direction)
		{
			SortDirectionCoreValue = direction;
			SortPropertyCoreValue = property;
			List<Item> items = Items as List<Item>;
			if (items != null)
			{
				int dir = SortDirectionCoreValue == ListSortDirection.Ascending ? 1 : -1;
				items.Sort((x, y) => dir * x.CompareTo(y, SortPropertyCoreValue.Name));
				IsSortedCoreValue = true;
			}
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		protected override void RemoveSortCore()
		{
			IsSortedCoreValue = false;
		}
	}
}
