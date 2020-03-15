/*
 * Copyright (c) 2015 Alex "Y_Less" Cole
 *
 * This Source Code Form is subject to the terms of the Mozilla Public License,
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/.
 */

using System;
using SQLite;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Input;
using Ten2Five.Utils;

namespace Ten2Five.Plugins
{
    public class ExerciseMap : INotifyPropertyChanged
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		private string word_ = "";
		public string Exercise { get { return word_; } set { word_ = value; Notify("Exercise"); } }

        public DateTime Added { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		protected void Notify(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}

	public class PluginExercises : Plugin
	{
		private readonly ObservableCollection<ExerciseMap> exercises_;
		private readonly SQLiteConnection db_ = null;

		private ExerciseMap currentExercise_ = null;

		private static readonly ExerciseMap nullExercise_ = new ExerciseMap { Exercise = "No Exercises Found!" };

		private readonly Random rand_ = new Random();

		public PluginExercises(SQLiteConnection db)
		{
			db_ = db;
			db.CreateTable<ExerciseMap>();
			exercises_ = new ObservableCollection<ExerciseMap>(db.Table<ExerciseMap>().OrderBy(x => x.Exercise));
			foreach (ExerciseMap newItem in exercises_)
			{
				newItem.PropertyChanged += this.OnItemPropertyChanged;
			}
			exercises_.CollectionChanged += this.OnCollectionChanged;
		}

		public override Window ShowConfigure()
		{
			// Create a window to configure this plugin.
			return new PluginExercisesConfigure(db_, exercises_);
		}

		public override Window ShowAbout()
		{
			// Create a window to configure this plugin.
			return new PluginExercisesAbout();
		}

		public override void Init(SQLiteConnection db)
		{
			// Change the word.
			if (exercises_.Count == 0)
				currentExercise_ = nullExercise_;
			else
				currentExercise_ = exercises_[rand_.Next(exercises_.Count)];
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			db_.BeginTransaction();
			if (e.NewItems != null)
			{
				foreach (ExerciseMap newItem in e.NewItems)
				{
                    // Added now.
                    newItem.Added = DateTime.Now;
					db_.Insert(newItem);
					newItem.PropertyChanged += this.OnItemPropertyChanged;
				}
			}
			if (e.OldItems != null)
			{
				foreach (ExerciseMap oldItem in e.OldItems)
				{
					db_.Delete(oldItem);
					oldItem.PropertyChanged -= this.OnItemPropertyChanged;
				}
			}
			db_.Commit();
		}

		void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ExerciseMap item = sender as ExerciseMap;
			if (item != null)
				db_.Update(item);
		}

		public override bool Update(double tick)
		{
			return false;
		}

		public override void Render(Canvas parent)
		{
			parent.Children.Clear();
			double
				w = parent.ActualWidth,
				h = parent.ActualHeight,
				x = w / 2.0,
				y = h / 2.0;
			Label tb = new Label
			{
				Width = w,
				Height = h,
				Margin = new Thickness(0, 0, 0, 0),
				VerticalContentAlignment = VerticalAlignment.Center,
				HorizontalContentAlignment = HorizontalAlignment.Center,
				FontSize = 60,
				Foreground = Brushes.RoyalBlue,
				Content = currentExercise_.Exercise
			};
			parent.Children.Add(tb);
		}

		public override string Name { get { return "Learn Exercises"; } }
	}
}

