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
using SQLite;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Input;

namespace Ten2Five.Plugins
{
	public class WordMap : INotifyPropertyChanged
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		private string word_ = "";
		public string Word { get { return word_; } set { word_ = value; Notify("Word"); } }

		private string meaning_ = "";
		public string Meaning { get { return meaning_; } set { meaning_ = value; Notify("Meaning"); } }

		private int shown_ = 0;
		public int Shown { get { return shown_; } set { shown_ = value; Notify("Shown"); } }

		private int wrong_ = 0;
		public int Wrong { get { return wrong_; } set { wrong_ = value; Notify("Wrong"); } }

        public DateTime Added { get; set; }

		[Ignore]
		public bool Right { set { if (!value) ++wrong_; ++shown_; Notify("Right"); } }

		public event PropertyChangedEventHandler PropertyChanged;

		protected void Notify(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}

	public class PluginWords : Plugin
	{
		private ObservableCollection<WordMap> words_;
		private  SQLiteConnection db_ = null;

		private DateTime startTime_;
		private WordMap currentWord_ = null;

		private static WordMap nullWord_ = new WordMap { Meaning = "No Words Found!", Word = "No Words Found!" };

		public static double DISPLAY_SECONDS = 15.0;
		public static double ANSWER_SECONDS = DISPLAY_SECONDS + 5.0;

		private bool showMeaning_;
		private bool showAnswer_;

		private Random rand_ = new Random();
		private bool dirty_ = false;
		private Style yesStyle_;
		private  WordMap toSave_ = null;
		private  bool right_ = false;

		public PluginWords(SQLiteConnection db)
		{
			db_ = db;
			db.CreateTable<WordMap>();
			words_ = new ObservableCollection<WordMap>(db.Table<WordMap>().OrderBy(x => x.Word));
			foreach (WordMap newItem in words_)
			{
				newItem.PropertyChanged += this.OnItemPropertyChanged;
			}
			words_.CollectionChanged += this.OnCollectionChanged;
			yesStyle_ = new Style();
			Trigger omo;
			omo = new Trigger() { Property = UIElement.IsMouseOverProperty, Value = true };
			omo.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Gainsboro));
			yesStyle_.Triggers.Add(omo);
			omo = new Trigger() { Property = UIElement.IsMouseOverProperty, Value = false };
			omo.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
			yesStyle_.Triggers.Add(omo);
            // TODO: Generate the playlist for today.
		}

		public override Window ShowConfigure()
		{
			// Create a window to configure this plugin.
			return new PluginWordsConfigure(db_, words_);
		}

		public override Window ShowAbout()
		{
			// Create a window to configure this plugin.
			return new PluginWordsAbout();
		}

		public override void Init(SQLiteConnection db)
		{
			startTime_ = DateTime.MinValue;
			currentWord_ = null;
			showAnswer_ = true;
			dirty_ = true;
		}
		
		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			db_.BeginTransaction();
			if (e.NewItems != null)
			{
				foreach (WordMap newItem in e.NewItems)
				{
                    // Added now.
                    newItem.Added = DateTime.Now;
					db_.Insert(newItem);
					newItem.PropertyChanged += this.OnItemPropertyChanged;
				}
			}
			if (e.OldItems != null)
			{
				foreach (WordMap oldItem in e.OldItems)
				{
					db_.Delete(oldItem);
					oldItem.PropertyChanged -= this.OnItemPropertyChanged;
				}
			}
			db_.Commit();
		}

		void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			WordMap item = sender as WordMap;
			if (item != null)
				db_.Update(item);
		}

		public override bool Update(double tick)
		{
			DateTime now = DateTime.Now;
			if (showAnswer_)
			{
				if ((now - startTime_).TotalSeconds >= ANSWER_SECONDS)
				{
					// Change the word.
					if (words_.Count == 0)
						currentWord_ = nullWord_;
					else
						currentWord_ = WordSelection.Select(words_);
					startTime_ = now;
					showMeaning_ = rand_.Next(2) == 1;
					showAnswer_ = false;
					dirty_ = false;
					return true;
				}
			}
			else
			{
				if ((now - startTime_).TotalSeconds >= DISPLAY_SECONDS)
				{
					// Change the word.
					showAnswer_ = true;
					showMeaning_ = !showMeaning_;
					dirty_ = false;
					return true;
				}
			}
			if (toSave_ != null)
			{
				toSave_.Right = right_;
				toSave_ = null;
				right_ = false;
			}
			if (dirty_)
			{
				dirty_ = false;
				return true;
			}
			return false;
		}

		private void OnKnow(object sender, EventArgs e)
		{
			dirty_ = true;
			showAnswer_ = true;
			showMeaning_ = !showMeaning_;
			startTime_ = DateTime.Now.AddSeconds(-DISPLAY_SECONDS);
		}

		private void OnYes(object sender, EventArgs e)
		{
			startTime_ = DateTime.Now.AddSeconds(-ANSWER_SECONDS);
			toSave_ = currentWord_;
			right_ = true;
		}

		private void OnNo(object sender, EventArgs e)
		{
			startTime_ = DateTime.Now.AddSeconds(-ANSWER_SECONDS);
			toSave_ = currentWord_;
		}

		public override void Render(Canvas parent)
		{
			parent.Children.Clear();
			double
				w = parent.ActualWidth,
				h = parent.ActualHeight,
				x = w / 2.0,
				y = h / 2.0;
			Label tb = new Label();
			tb.Width = w;
			tb.Height = h;
			tb.Margin = new Thickness(0, 0, 0, 0);
			tb.VerticalContentAlignment = VerticalAlignment.Center;
			tb.HorizontalContentAlignment = HorizontalAlignment.Center;
			tb.FontSize = 60;
			tb.Foreground = Brushes.RoyalBlue;
			string
				str = (showMeaning_ ? currentWord_.Meaning : currentWord_.Word); //.Replace('`', '\x0301');
			tb.Content = str;
			parent.Children.Add(tb);
			if (showAnswer_)
			{
				Button yes = new Button();
				yes.Content = "✔";
				yes.Style = yesStyle_;
				yes.Cursor = Cursors.Hand;
				yes.BorderBrush = Brushes.Transparent;
				yes.Foreground = Brushes.LimeGreen;
				yes.Width = 100;
				yes.Height = 100;
				yes.Margin = new Thickness(x + 10.0, y + 70.0, 0.0, 0.0);
				yes.Click += OnYes;
				yes.FontSize = 72.0;
				parent.Children.Add(yes);
				Button no = new Button();
				no.Content = "✖";
				no.Style = yesStyle_;
				no.Cursor = Cursors.Hand;
				no.BorderBrush = Brushes.Transparent;
				no.Foreground = Brushes.DarkRed;
				no.Width = 100;
				no.Height = 100;
				no.Margin = new Thickness(x - 110.0, y + 70.0, 0.0, 0.0);
				no.Click += OnNo;
				no.FontSize = 72.0;
				parent.Children.Add(no);
			}
			else
			{
				Button cb = new Button();
				cb.Content = "Check";
				cb.Width = 100;
				cb.Height = 50;
				cb.Margin = new Thickness(x - 50.0, y + 70.0, 0.0, 0.0);
				cb.Click += OnKnow;
				cb.FontSize = 20.0;
				parent.Children.Add(cb);
			}
		}

		public override string Name { get { return "Learn Words"; } }
	}
}

