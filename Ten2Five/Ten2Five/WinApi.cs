using System;
using System.Collections.Generic;
using System.Linq;
// http://www.codeproject.com/Articles/32908/C-Single-Instance-App-With-the-Ability-To-Restore

using System.Text;
using System.Runtime.InteropServices;

namespace Ten2Five
{
	static public class WinApi
	{
		[DllImport("user32.dll")]
		public static extern int RegisterWindowMessage(string message);

		[DllImport("user32.dll")]
		public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

		[DllImport("user32.dll")]
		public static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		public const int HWND_BROADCAST = 0xffff;
		public const int SW_HIDE = 0;
		public const int SW_SHOWNORMAL = 1;
		public const int SW_SHOWMINIMIZED = 2;
		public const int SW_SHOWMAXIMIZED = 3;
		public const int SW_SHOWNOACTIVATE = 4;
		public const int SW_SHOW = 5;
		public const int SW_MINIMIZE = 6;
		public const int SW_SHOWMINNOACTIVE = 7;
		public const int SW_SHOWNA = 8;
		public const int SW_RESTORE = 9;
		public const int SW_SHOWDEFAULT = 10;
		public const int SW_FORCEMINIMIZE = 11;

		public static int RegisterWindowMessage(string format, params object[] args)
		{
			return RegisterWindowMessage(String.Format(format, args));
		}

		public static void ShowToFront(IntPtr window)
		{
			//ShowWindow(window, SW_MINIMIZE);
			//ShowWindow(window, SW_SHOW);
			//ShowWindow(window, SW_RESTORE);
			ShowWindow(window, SW_SHOWNORMAL);
			SetForegroundWindow(window);
		}
	}
}

