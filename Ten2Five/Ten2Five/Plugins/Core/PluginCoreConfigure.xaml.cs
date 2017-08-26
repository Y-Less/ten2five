/*
 * Copyright (c) 2015 Alex "Y_Less" Cole
 *
 * This Source Code Form is subject to the terms of the Mozilla Public License,
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ten2Five.Utils;

namespace Ten2Five.Plugins
{
    /// <summary>
    /// Interaction logic for PluginCoreConfigure.xaml
    /// </summary>
    public partial class PluginCoreConfigure : Window
	{
		private ProgSettings settings_;

		public PluginCoreConfigure(ProgSettings settings)
		{
			InitializeComponent();
			settings_ = settings;
			TB_Work.Text = "" + settings.WorkSeconds;
			TB_Play.Text = "" + settings.PlaySeconds;
			CB_PlaySounds.IsChecked = settings.Sound;
			CB_PauseMusic.IsChecked = settings.PauseMusic;
			TB_Work.Focus();
		}

		private void OK_Button_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				int
                    ws = Int32.Parse(TB_Work.Text),
                    ps = Int32.Parse(TB_Play.Text);
				if (ws < 1 || ps < 1)
					MessageBox.Show("Both times must be positive integers.", "Time Settings");
				else
				{
					settings_.PlaySeconds = ps;
					settings_.WorkSeconds = ws;
					settings_.Sound = CB_PlaySounds.IsChecked == true;
					settings_.PauseMusic = CB_PauseMusic.IsChecked == true;
                    this.Close(true);
                    return;
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Both times must be positive integers.", "Time Settings");
			}
            this.Close(false);
        }

		private void Cancel_Button_Click(object sender, RoutedEventArgs e)
		{
            this.Close(false);
        }

		private void TB_GotFocus(object sender, RoutedEventArgs e)
		{
			(sender as TextBox).SelectAll();
		}

		private void TB_KeyUp(object sender, KeyEventArgs e)
		{
			// If it is an enter - return.
			if (e.Key == Key.Enter || e.Key == Key.Return)
				OK_Button_Click(sender, null);
			else if (e.Key == Key.Escape)
				Cancel_Button_Click(sender, null);
		}

	}
}

