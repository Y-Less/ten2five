/*
 * Copyright (c) 2015 Alex "Y_Less" Cole
 *
 * This Source Code Form is subject to the terms of the Mozilla Public License,
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Ten2Five
{
	public class MediaControl
	{
		private const int APPCOMMAND_VOLUME_MUTE          = 0x080000;
		private const int APPCOMMAND_VOLUME_UP            = 0x0A0000;
		private const int APPCOMMAND_VOLUME_DOWN          = 0x090000;
		private const int APPCOMMAND_MEDIA_NEXTTRACK      = 0x0B0000;
		private const int APPCOMMAND_MEDIA_PREVIOUSTRACK  = 0x0C0000;
		private const int APPCOMMAND_MEDIA_PLAY           = 0x2E0000;
		private const int APPCOMMAND_MEDIA_PAUSE          = 0x2F0000;
		private const int APPCOMMAND_MEDIA_STOP           = 0x0D0000;
		private const int APPCOMMAND_MEDIA_PLAY_PAUSE     = 0x0E0000;
		private const int WM_APPCOMMAND = 0x319;

		private static IntPtr HANDLE = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

		[DllImport("user32.dll")]
		private static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		public static void PlayPause()
		{
			SendMessageW(HANDLE, WM_APPCOMMAND, HANDLE, (IntPtr)APPCOMMAND_MEDIA_PLAY_PAUSE);
		}

		public static void Play()
		{
			SendMessageW(HANDLE, WM_APPCOMMAND, HANDLE, (IntPtr)APPCOMMAND_MEDIA_PLAY);
		}

		public static void Pause()
		{
			SendMessageW(HANDLE, WM_APPCOMMAND, HANDLE, (IntPtr)APPCOMMAND_MEDIA_PAUSE);
		}

		public static void Stop()
		{
			SendMessageW(HANDLE, WM_APPCOMMAND, HANDLE, (IntPtr)APPCOMMAND_MEDIA_STOP);
		}

		public static void NextTrack()
		{
			SendMessageW(HANDLE, WM_APPCOMMAND, HANDLE, (IntPtr)APPCOMMAND_MEDIA_NEXTTRACK);
		}

		public static void PreviousTrack()
		{
			SendMessageW(HANDLE, WM_APPCOMMAND, HANDLE, (IntPtr)APPCOMMAND_MEDIA_PREVIOUSTRACK);
		}

		public static void Mute()
		{
			SendMessageW(HANDLE, WM_APPCOMMAND, HANDLE, (IntPtr)APPCOMMAND_VOLUME_MUTE);
		}

		public static void VolumeUp()
		{
			SendMessageW(HANDLE, WM_APPCOMMAND, HANDLE, (IntPtr)APPCOMMAND_VOLUME_DOWN);
		}

		public static void VolumeDown()
		{
			SendMessageW(HANDLE, WM_APPCOMMAND, HANDLE, (IntPtr)APPCOMMAND_VOLUME_UP);
		}
	}
}

