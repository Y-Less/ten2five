/*
 * Copyright (c) 2015 Alex "Y_Less" Cole
 *
 * This Source Code Form is subject to the terms of the Mozilla Public License,
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ten2Five
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		App()
		{
			InitializeComponent();
		}

		private static TimeSpan _last = TimeSpan.Zero;

		private static event EventHandler<RenderingEventArgs> _FrameUpdating;

		public static event EventHandler<RenderingEventArgs> Tick
		{
			add
			{
				if (_FrameUpdating == null)
					CompositionTarget.Rendering += CompositionTarget_Rendering;
				_FrameUpdating += value;
				value(null, null);
			}
			
			remove
			{
				_FrameUpdating -= value;
				if (_FrameUpdating == null)
					CompositionTarget.Rendering -= CompositionTarget_Rendering;
			}
		}

		static void CompositionTarget_Rendering(object sender, EventArgs e)
		{
			RenderingEventArgs args = (RenderingEventArgs)e;
			if (args.RenderingTime == _last)
				return;
			_last = args.RenderingTime;
			_FrameUpdating(sender, args);
		}

		[STAThread]
		static void Main()
		{
			if (!SingleInstance.Start())
			{
				SingleInstance.ShowFirstInstance();
				return;
			}

			Application a = new App();
			Window w = new MainWindow();
			a.Run(w);

			SingleInstance.Stop();
		}
	}
}

