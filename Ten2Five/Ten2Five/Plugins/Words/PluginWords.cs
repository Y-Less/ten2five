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

		private double accuracy_ = 1.0;

		private string word_ = "";
		public string Word { get { return word_; } set { word_ = value; Notify("Word"); } }

		private string meaning_ = "";
		public string Meaning { get { return meaning_; } set { meaning_ = value; Notify("Meaning"); } }

		private int shown_ = 0;
		public int Shown { get { return shown_; } set { shown_ = value; Notify("Shown"); accuracy_ = GetAccuracy(shown_, wrong_);  } }

		private int wrong_ = 0;
		public int Wrong { get { return wrong_; } set { wrong_ = value; Notify("Wrong"); } }

		[Ignore]
		public bool Right { set { if (!value) ++Wrong; ++Shown; } }

		// Return the likelihood of this result being down to random chance.
		[Ignore]
		public double Accuracy { get { return accuracy_; } }

		public event PropertyChangedEventHandler PropertyChanged;

		private static double GetAccuracy(int n, int k)
		{
			return ChooseSum(n, k) / Math.Pow(2.0, n);
		}

		// This function does:
		// 
		//     k
		//    ---
		//    \     (n)
		//    /     (i)
		//    ---
		//   i = 0
		// 
		// I.e. a partial sum of combinations.
		private static long ChooseSum(int n, int k)
		{
			// Invalid.
			if (k > n)
				return 0;
			// Always 2 ** n.
			if (k == n)
				return (long)Math.Pow(2, n);
			if (k == 0)
				return 1;
			// We DO NOT want to mirror this, since we need the partial sum of
			// all combinations.
			long r = 1;
			long total = 1;
			// Slow, but won't overflow (much).
			for (long d = 1; d <= k; ++d)
			{
				r *= n;
				r /= d;
				--n;
				// Partial sum of combinations.
				total += r;
			}
			return total;
		}

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

