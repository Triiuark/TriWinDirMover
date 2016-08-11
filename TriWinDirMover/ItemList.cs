using System.Collections.Generic;
using System.ComponentModel;

namespace TriWinDirMover
{
	class ItemList : BindingList<Item>
	{
		protected override bool SupportsSortingCore
		{
			get
			{
				return true;
			}
		}

		private ListSortDirection SortDirectionCoreValue;
		protected override ListSortDirection SortDirectionCore
		{
			get
			{
				return SortDirectionCoreValue;
			}
		}

		private PropertyDescriptor SortPropertyCoreValue;
		protected override PropertyDescriptor SortPropertyCore
		{
			get
			{
				return SortPropertyCoreValue;
			}
		}

		private bool IsSortedCoreValue;
		protected override bool IsSortedCore
		{
			get
			{
				return IsSortedCoreValue;
			}
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

		public string SumSizes()
		{
			List<Item> items = Items as List<Item>;
			long sum = 0;
			bool isReady = true;
			if (items != null)
			{
				foreach (Item item in Items)
				{
					if (!item.IsDisabled)
					{
						if (item.IsSizeCalculated)
						{
							sum += item.Size;
						}
						else
						{
							isReady = false;
						}
					}
				}
			}
			return Directory.ToHumanReadableSize(sum) + (isReady ? "" : " ...");
		}
	}
}
