﻿/*
 * Copyright (c) 2015 Alex "Y_Less" Cole
 *
 * This Source Code Form is subject to the terms of the Mozilla Public License,
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.ObjectModel;
using SQLite;
using Ten2Five.Utils;
using System.Globalization;

namespace Ten2Five.Plugins
{
    /// <summary>
    /// Interaction logic for PluginExercisesConfigure.xaml
    /// </summary>
    public partial class PluginExercisesConfigure : Window
	{
		ObservableCollection<ExerciseMap> words_;
        private SQLiteConnection db_;

        public PluginExercisesConfigure(SQLiteConnection db, ObservableCollection<ExerciseMap> words)
		{
            db_ = db;
			words_ = words;
			InitializeComponent();
            TB_Add1.Focus();
            List_Exercises.ItemsSource = words;

            InputLanguageManager.SetInputLanguage(TB_Add2, CultureInfo.CreateSpecificCulture("en-GB"));
            InputLanguageManager.SetInputLanguage(TB_Add1, CultureInfo.CreateSpecificCulture("ru-RU"));
        }

        private void OK_Click(object sender, RoutedEventArgs e)
		{
			this.Close(true);
		}
        
		private void Remove_Exercise_Click(object sender, RoutedEventArgs e)
		{
			words_.Remove((ExerciseMap)(sender as Button).Content);
		}

		private void Add_Exercise_Click(object sender, RoutedEventArgs e)
		{
			string
				s0 = TB_Add1.Text.Trim(),
				s1 = TB_Add2.Text.Trim();
			if (s0.Length != 0 && s1.Length != 0)
			{
                var
                    todb = new ExerciseMap { Exercise = s0, Meaning = s1 };
                words_.InsertSorted(todb, x => x.Exercise);
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
                x = new M3UHandler(db_, "test", "./Playlists");
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

