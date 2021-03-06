﻿/*
 * Copyright (c) 2015 Alex "Y_Less" Cole
 *
 * This Source Code Form is subject to the terms of the Mozilla Public License,
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Input;
using Ten2Five.Utils;

namespace Ten2Five.Plugins
{
    /// <summary>
    /// Interaction logic for PluginExercisesAbout.xaml
    /// </summary>
    public partial class PluginExercisesAbout : Window
	{
		public PluginExercisesAbout()
		{
			InitializeComponent();
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
            this.Close(true);
        }

		private object sawDown_ = null;

		private void Click_MouseDown(object sender, MouseButtonEventArgs e)
		{
			sawDown_ = sender;
		}

		private void Reset_MouseUp(object sender, MouseButtonEventArgs e)
		{
			sawDown_ = null;
		}

		private void Click_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (sender == sawDown_)
			{
				// Finished a click.
				if (sender == Link_MPL)
					GoTo("https://www.mozilla.org/en-US/MPL/2.0/");
			}
			sawDown_ = null;
		}

		private void GoTo(string target)
		{
			System.Diagnostics.Process.Start(target);
		}
	}
}
