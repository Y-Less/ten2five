/*
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
            TB_Add2.Focus();
            List_Exercises.ItemsSource = words;

            InputLanguageManager.SetInputLanguage(TB_Add2, CultureInfo.CreateSpecificCulture("en-GB"));
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
				s1 = TB_Add2.Text.Trim();
			if (s1.Length != 0)
			{
                var
                    todb = new ExerciseMap { Exercise = s1 };
                words_.InsertSorted(todb, x => x.Exercise);
            }
			TB_Add2.Text = "";
			TB_Add2.Focus();
		}
    }
}

