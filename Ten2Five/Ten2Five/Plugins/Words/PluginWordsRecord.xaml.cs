/*
 * Copyright (c) 2015 Alex "Y_Less" Cole
 *
 * This Source Code Form is subject to the terms of the Mozilla Public License,
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using Ten2Five.Utils;

namespace Ten2Five.Plugins
{
    /// <summary>
    /// Interaction logic for PluginWordsRussian.xaml
    /// </summary>
    public partial class PluginWordsRecord : Window
	{
        private MP3Recorder mp3_ = null;
        private readonly string name_;

        public PluginWordsRecord(string lang, int id)
		{
            name_ = "./Raw/" + id.ToString() + "_" + lang + ".mp3";
            InitializeComponent();
            Title = "Learn Words: Record " + lang + " Clip";
            Retry_Click(null, null);
            Stop_Button.Focus();
        }

		private void OK_Click(object sender, RoutedEventArgs e)
		{
            mp3_.Dispose();
            this.Close(true);
        }

        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            if (mp3_ != null)
                mp3_.Dispose();
            mp3_ = new MP3Recorder(name_);
        }
    }
}

