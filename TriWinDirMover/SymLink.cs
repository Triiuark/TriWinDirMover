using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TriWinDirMover
{
	internal static class SymLink
	{
		// open existing
		private const uint CREATION_DISPOSITION = 3;

		// generic read
		private const uint DESIRED_ACCESS = 0x80000000;

		private const uint ERROR_NOT_A_REPARSE_POINT  = 4390;

		// backup semantics (needed if directory), open reparse point
		private const uint FLAGS_AND_ATTRIBUTES = 0x02000000 | 0x00200000;

		private const uint FSCTL_GET_REPARSE_POINT    = 0x000900A8;
		private const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
		private const uint IO_REPARSE_TAG_SYMLINK     = 0xA000000C;

		// read, write, delete
		private const uint SHARE_MODE = 1 | 2 | 4;

		public static string GetTarget(string fileName)
		{
			SafeFileHandle handle = new SafeFileHandle(CreateFile(fileName,
				DESIRED_ACCESS, SHARE_MODE, IntPtr.Zero,
				CREATION_DISPOSITION, FLAGS_AND_ATTRIBUTES, IntPtr.Zero),
				true);

			if (Marshal.GetLastWin32Error() != 0)
			{
				string message = new Win32Exception(Marshal.GetLastWin32Error()).Message;
				throw new IOException(message + " (" + fileName + ")",
					Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
			}

			int outBufferSize = Marshal.SizeOf(typeof(REPARSE_DATA_BUFFER));
			IntPtr outBuffer = Marshal.AllocHGlobal(outBufferSize);

			try
			{
				int bytesReturned;
				bool result = DeviceIoControl(handle.DangerousGetHandle(),
					FSCTL_GET_REPARSE_POINT, IntPtr.Zero, 0,
					outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero);

				if (!result)
				{
					if (Marshal.GetLastWin32Error() == ERROR_NOT_A_REPARSE_POINT)
					{
						return null;
					}
					string message = new Win32Exception(Marshal.GetLastWin32Error()).Message;
					throw new IOException(message + " (" + fileName + ")",
						Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));
				}

				REPARSE_DATA_BUFFER reparseDataBuffer = (REPARSE_DATA_BUFFER)
					Marshal.PtrToStructure(outBuffer, typeof(REPARSE_DATA_BUFFER));

				ushort offset = reparseDataBuffer.PrintNameOffset;
				if (reparseDataBuffer.ReparseTag == IO_REPARSE_TAG_SYMLINK)
				{
					// + sizeof(uint) for Flags
					// which I don't have in REPARSE_DATA_BUFFER
					offset += sizeof(uint);
				}
				else if (reparseDataBuffer.ReparseTag != IO_REPARSE_TAG_MOUNT_POINT)
				{
					// throw an error?
				}
				string targetDir = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer,
					offset, reparseDataBuffer.PrintNameLength);

				return targetDir;
			}
			finally
			{
				handle.Close();
				Marshal.FreeHGlobal(outBuffer);
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr CreateFile(string lpFileName,
			uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes,
			uint dwCreationDisposition, uint dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool DeviceIoControl(IntPtr hDevice,
			uint dwIoControlCode, IntPtr InBuffer, int nInBufferSize,
			IntPtr OutBuffer, int nOutBufferSize, out int pBytesReturned,
			IntPtr lpOverlapped);

		[StructLayout(LayoutKind.Sequential)]
		private struct REPARSE_DATA_BUFFER
		{
			public uint  ReparseTag;
			public ushort ReparseDataLength;
			public ushort Reserved;
			public ushort SubstituteNameOffset;
			public ushort SubstituteNameLength;
			public ushort PrintNameOffset;
			public ushort PrintNameLength;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x3FF0)]
			public byte[] PathBuffer;
		}
	}
}
