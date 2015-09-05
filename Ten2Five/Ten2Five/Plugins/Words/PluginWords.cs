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
		private bool dirty_;

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
		}

		public override Window ShowConfigure()
		{
			// Create a window to configure this plugin.
			return new PluginWordsConfigure(words_);
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
				if ((now - startTime_).TotalSeconds > ANSWER_SECONDS)
				{
					// Change the word.
					if (words_.Count == 0)
						currentWord_ = nullWord_;
					else
						currentWord_ = words_[rand_.Next(words_.Count)];
					startTime_ = now;
					showMeaning_ = rand_.Next(2) == 1;
					showAnswer_ = false;
					dirty_ = false;
					return true;
				}
			}
			else
			{
				if ((now - startTime_).TotalSeconds > DISPLAY_SECONDS)
				{
					// Change the word.
					showAnswer_ = true;
					showMeaning_ = !showMeaning_;
					dirty_ = false;
					return true;
				}
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
			if (!showAnswer_)
			{
				Button cb = new Button();
				cb.Content = "Check";
				cb.Width = 100;
				cb.Height = 50;
				cb.Margin = new Thickness(x - 50.0, y + 70.0, 0.0, 0.0);
				cb.Click += OnKnow;
				cb.FontSize = 20;
				parent.Children.Add(cb);
			}
		}

		public override string Name { get { return "Learn Words"; } }
	}
}

