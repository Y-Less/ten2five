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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ten2Five.Plugins
{
	/// <summary>
	/// Interaction logic for PluginCoreAbout.xaml
	/// </summary>
	public partial class PluginCoreAbout : Window
	{
		public PluginCoreAbout()
		{
			InitializeComponent();
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
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
				if (sender == Link_Design)
					GoTo("http://chads.website/tentwofive/");
				else if (sender == Link_More_Info)
					GoTo("http://www.43folders.com/2005/10/11/procrastination-hack-1025");
				else if (sender == Link_MPL)
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

