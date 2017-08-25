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
using System.Collections.ObjectModel;
using SQLite;
using Ten2Five.Utils;

namespace Ten2Five.Plugins
{
	/// <summary>
	/// Interaction logic for PluginWordsConfigure.xaml
	/// </summary>
	public partial class PluginWordsConfigure : Window
	{
		ObservableCollection<WordMap> words_;
        private SQLiteConnection db_;

        public PluginWordsConfigure(SQLiteConnection db, ObservableCollection<WordMap> words)
		{
            db_ = db;
			words_ = words;
			InitializeComponent();
			List_Words.ItemsSource = words;
		}
        
        private void OK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void Remove_Word_Click(object sender, RoutedEventArgs e)
		{
			words_.Remove((WordMap)(sender as Button).Content);
		}

		private void Add_Word_Click(object sender, RoutedEventArgs e)
		{
			string
				s0 = TB_Add1.Text.Trim(),
				s1 = TB_Add2.Text.Trim();
			if (s0.Length != 0 && s1.Length != 0)
			{
				words_.InsertSorted(new WordMap { Word = s0, Meaning = s1 }, x => x.Word);
			}
			TB_Add1.Text = "";
			TB_Add2.Text = "";
			TB_Add1.Focus();
		}

		private void MaybeAdd1(object sender, TextCompositionEventArgs e)
		{
			if (e.Text == "" && e.SystemText != "" && Keyboard.IsKeyDown(Key.RightAlt))
			{
				TextBox tb = sender as TextBox;
				string str = tb.Text;
				int tp = tb.SelectionStart;
				int tl = tb.SelectionLength;
				tb.Text = str.Substring(0, tp) + e.SystemText + "\x0301" + str.Substring(tp + tl);
				tb.SelectionStart = tp + e.SystemText.Length + 1;
				e.Handled = true;
			}
		}

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var
                x = new M3Uhandler(db_, "test", "./Playlists");
            x.AddFile("../Clips/hello1");
            x.AddFile("../Clips/hello2");
            x.AddFile("../Clips/hello3");
            x.AddFile("../Clips/hello4");
            x.AddFile("../Clips/hello5");
            x.AddFile("../Clips/hello6");
            x.AddFile("../Clips/hello7");
        }
    }
}

