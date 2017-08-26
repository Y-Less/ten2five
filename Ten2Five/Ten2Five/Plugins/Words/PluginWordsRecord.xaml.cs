/*
 * Copyright (c) 2015 Alex "Y_Less" Cole
 *
 * This Source Code Form is subject to the terms of the Mozilla Public License,
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/.
 */

using CSCore.SoundIn;
using System.IO;
using System.Windows;
using System.Windows.Input;
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
        private object p;
        private readonly ISoundIn iSoundIn_;
        private readonly bool iGiven_;

        public PluginWordsRecord(string lang, int id, ISoundIn p)
		{
            if (p == null)
            {
                iSoundIn_ = new WasapiCapture();
                iSoundIn_.Initialize();
                iGiven_ = false;
            }
            else
            {
                iSoundIn_ = p;
                iGiven_ = true;
            }

            name_ = "./Raw/" + id.ToString() + "_" + lang + ".mp3";
            InitializeComponent();
            Title = "Learn Words: Record " + lang + " Clip";
            Retry_Click(null, null);
            Stop_Button.Focus();
        }

        public ISoundIn Sound => iSoundIn_;
        public bool Disposed => mp3_.Disposed;

        private void OK_Click(object sender, RoutedEventArgs e)
		{
            mp3_.Clean(iGiven_);
            this.DialogResult = true;
		}

        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            if (mp3_ != null)
                mp3_.Clean(false);
            mp3_ = new MP3Recorder(iSoundIn_, name_);
        }
    }
}

