/*
 * Copyright (c) 2015 Alex "Y_Less" Cole
 *
 * This Source Code Form is subject to the terms of the Mozilla Public License,
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/.
 */

/* 
 * This file contains the main window, the majority of the view when running
 * ten2five.
 * 
 * It is a study in code repetition and missing comments, and this fact is
 * making me really antsy!  The code started out as a single file, just a quick
 * hack to render a circle and save some TODO items, then it grew slightly and
 * never recovered from its poor beginnings.  I opted not to rip everything out
 * and restart as ultimately it is now working quite well and performing major
 * refactorings on what is meant to be a functioning procrastination hack sort
 * of defeats the object!  I learned things from the experience, and I can apply
 * them to future projects, but while it is annoying me, there's no point in
 * retroactively applying them to the project, when I'm happy with the result.
 * 
 * This file also includes the Task and ProgSettings classes, both stored in a
 * local sqlite database (using the SQLite library from):
 * 
 * https://github.com/praeclarum/sqlite-net
 * 
 * The design is based on:
 * 
 * http://chads.website/tentwofive/
 * 
 * And the original technique comes from:
 * 
 * http://www.43folders.com/2005/10/11/procrastination-hack-1025
 * 
 * The minimise to system tray code comes from:
 * 
 * http://dlaa.me/blog/post/9889700
 * 
 * The single running instance with window switch was modified (to account for
 * a running instance potentially being in the system tray, since the advertised
 * method didn't work for me) from:
 * 
 * http://www.codeproject.com/Articles/32908/C-Single-Instance-App-With-the-Ability-To-Restore
 * 
 * The system tray icon generation code is from:
 * 
 * http://www.codeproject.com/Articles/7122/Dynamically-Generating-Icons-safely?msg=1304833
 * 
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;
using SQLite;
using Ten2Five.Drawing;
using Ten2Five.Plugins;
using System.Windows.Interop;

namespace Ten2Five
{
    public class Task : INotifyPropertyChanged
    {
        private bool done_;

		private int order_;

        public event PropertyChangedEventHandler PropertyChanged;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }

		public int Order
		{
			get { return order_; }
			set { order_ = value; Notify("Order"); }
		}

        public bool Done
        {
            get { return done_; }
            set { done_ = value; Notify("Done"); }
        }

        private void Notify(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public Task(string n, int order)
        {
            Name = n;
            done_ = false;
			order_ = order;
        }

        public Task()
        {
            Name = "";
            done_ = false;
			order_ = 0;
        }
    }

    public class ProgSettings
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private bool sound_ = true;
        private int workSeconds_ = 600;
        private int playSeconds_ = 120;
		private bool ballonShown_ = false;

        public bool Sound { get { return sound_; } set { sound_ = value; Update(); } }

        private void Update()
        {
            if (DB != null)
                DB.Update(this);
        }
        public int WorkSeconds { get { return workSeconds_; } set { workSeconds_ = value; Update(); } }
        public int PlaySeconds { get { return playSeconds_; } set { playSeconds_ = value; Update(); } }

        public ProgSettings()
        {
        }

        public ProgSettings(SQLiteConnection db)
        {
            Id = 0;
            DB = db;
            DB.Insert(this);
        }

        public SQLiteConnection DB = null;

		//private static ObservableCollection<Plugin> plugins_ = new ObservableCollection<Plugin>(new Plugin[] { new PluginWords() });

		[Ignore]
		public ObservableCollection<Plugin> Plugins { get; set; }

		public bool PauseMusic { get; set; }

		public bool BallonShown { get { return ballonShown_; } set { ballonShown_ = value; Update(); } }
	}

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		private SQLiteConnection db_;

		private DateTime endPoint_;
		private DateTime pausePoint_;
		private DateTime startPoint_;
		private bool working_ = true;
		private bool running_ = true;

		private ObservableCollection<Task> tasks_ = new ObservableCollection<Task>();
		private MediaPlayer mediaPlayer_ = new MediaPlayer();
		private ProgSettings settings_;
		private int cycle_ = 1;
		private MinimizeToTray minimise_;

		private DrawnCollection customRenders_ = new DrawnCollection();
		private DrawnPartCircle workArc_ = new DrawnPartCircle();
		private DrawnPartCircle playArc_ = new DrawnPartCircle();
		private DrawnPartCircle workGrey_ = new DrawnPartCircle();
		private DrawnPartCircle playGrey_ = new DrawnPartCircle();

		private DateTime lastTick_ = DateTime.Now;

		private Canvas clockParent_ = null;

		private Plugin currentPlugin_ = null;

		private Random rand_ = new Random();

		public MainWindow(string dbpath)
		{
			db_ = new SQLiteConnection(dbpath);
			db_.CreateTable<Task>();
			db_.CreateTable<ProgSettings>();
			tasks_ = new ObservableCollection<Task>(db_.Table<Task>().OrderBy(t => t.Order));
			settings_ = db_.Find<ProgSettings>(1);

			minimise_ = new MinimizeToTray(this, settings_.BallonShown);

			settings_.Plugins = new ObservableCollection<Plugin>(new Plugin[] { new PluginCore(settings_), new PluginWords(db_) });

			if (settings_ == null)
				settings_ = new ProgSettings(db_);
			else
				settings_.DB = db_;
			foreach (Task newItem in tasks_)
			{
				newItem.PropertyChanged += this.OnItemPropertyChanged;
			}

			InitializeComponent();
			List_Tasks.ItemsSource = tasks_;
			Text_Add_Item.Focus();

			workArc_.Right = true;
			playArc_.Right = false;
			workGrey_.Right = false;
			playGrey_.Right = true;

			customRenders_.Add(workArc_);
			customRenders_.Add(playArc_);
			customRenders_.Add(workGrey_);
			customRenders_.Add(playGrey_);

			this.ResetTimer(true, DateTime.Now);

			App.Tick += Tick;
			Closed += MainWindow_Closed;

			mediaPlayer_.Open(new Uri(Environment.CurrentDirectory + "\\bell.mp3"));
			mediaPlayer_.Volume = 1.0;

			tasks_.CollectionChanged += this.OnCollectionChanged;
		}

		void MainWindow_Closed(object sender, EventArgs e)
		{
			settings_.BallonShown = minimise_.BaloonShown();
		}

		private void MoveBy(Task t, int move)
		{
			int listpos = tasks_.IndexOf(t);
			int newpos = listpos + move;
			// Is this a valid new position?
			if (newpos < 0 || newpos >= tasks_.Count)
				return;
			int orderpos = t.Order;
			Task other = tasks_[newpos];
			t.Order = other.Order;
			other.Order = orderpos;
			tasks_.Move(listpos, newpos);
		}

		private void MoveUp(Task t)
		{
			MoveBy(t, -1);
		}

		private void MoveDown(Task t)
		{
			MoveBy(t, 1);
		}

		private double nextSecond_ = 0.0;
		private  bool windowDirty_;
		
		private void ResetTimer(bool working, DateTime last)
		{
			nextSecond_ = 0.0;
			startPoint_ = last;
			pausePoint_ = last;
			if (working_)
			{
				workArc_.Enabled  = true;
				playArc_.Enabled  = false;
				workGrey_.Enabled = true;
				playGrey_.Enabled = false;
				workArc_.Percentage  = 0.0;
				workGrey_.Percentage = 0.0;
				endPoint_ = last.AddSeconds(settings_.WorkSeconds);
				List_Tasks.Margin      = new Thickness( 15, 100,  15,  70);
				Text_Add_Item.Margin   = new Thickness(-35,  50,   0,   0);
				Button_Add_Item.Margin = new Thickness(160,  50,   0,   0);
				Label_Tasks.Margin     = new Thickness( 10,  10,   0,   0);
				clockParent_ = Canvas_Clock;
				Canvas_Small.Children.Clear();
				Text_Clock.SetValue(Grid.ColumnProperty, 0);
				Text_Cycle.SetValue(Grid.ColumnProperty, 0);
				//Button_Clock.SetValue(Grid.ColumnProperty, 0);
				Text_Clock.VerticalAlignment  = VerticalAlignment.Center;
				Text_Cycle.VerticalAlignment  = VerticalAlignment.Center;
				//Button_Clock.VerticalAlignment = VerticalAlignment.Center;
				Text_Clock.Margin   = new Thickness(  0,   0,   0,  30);
				Text_Cycle.Margin   = new Thickness(  0, 100,   0,   0);
				//Button_Clock.Margin = new Thickness(  0,   0,   0,  30);
				Text_Clock.Foreground = Brushes.DarkRed;
				Text_Cycle.Foreground = Brushes.DarkRed;
				//Button_Small.Cursor = Cursors.Arrow;
				//Button_Clock.Cursor = Cursors.Hand;
				Button_Small.Visibility = Visibility.Collapsed;
				Button_Clock.Visibility = Visibility.Visible;
				int
					minutes,
					seconds;
				minutes = Math.DivRem(settings_.WorkSeconds, 60, out seconds);
				Text_Clock.Content = String.Format("{0}:{1,2:D2}", minutes, seconds);
			}
			else
			{
				workArc_.Enabled  = false;
				playArc_.Enabled  = true;
				workGrey_.Enabled = false;
				playGrey_.Enabled = true;
				playArc_.Percentage  = 1.0;
				playGrey_.Percentage = 1.0;
				endPoint_ = last.AddSeconds(settings_.PlaySeconds);
				List_Tasks.Margin      = new Thickness( 15, 385,  15,  70);
				Text_Add_Item.Margin   = new Thickness(-35, 335,   0,   0);
				Button_Add_Item.Margin = new Thickness(160, 335,   0,   0);
				Label_Tasks.Margin     = new Thickness( 10, 295,   0,   0);
				clockParent_ = Canvas_Small;
				Canvas_Clock.Children.Clear();
				Text_Clock.SetValue(Grid.ColumnProperty, 1);
				Text_Cycle.SetValue(Grid.ColumnProperty, 1);
				//Button_Clock.SetValue(Grid.ColumnProperty, 1);
				Text_Clock.VerticalAlignment = VerticalAlignment.Top;
				Text_Cycle.VerticalAlignment = VerticalAlignment.Top;
				//Button_Clock.VerticalAlignment = VerticalAlignment.Top;
				Text_Clock.Margin = new Thickness(0, 35, 0, 0);
				Text_Cycle.Margin   = new Thickness(0, 165, 0, 0);
				//Button_Clock.Margin = new Thickness(0, 35, 0, 0);
				Text_Clock.Foreground = Brushes.DarkGreen;
				Text_Cycle.Foreground = Brushes.DarkGreen;
				Text_Clock.Content = "0:00";
				//Button_Small.Cursor = Cursors.Hand;
				//Button_Clock.Cursor = Cursors.Arrow;
				Button_Clock.Visibility = Visibility.Collapsed;
				Button_Small.Visibility = Visibility.Visible;
				currentPlugin_ = settings_.Plugins[rand_.Next(settings_.Plugins.Count)];
				currentPlugin_.Init(db_);
			}
			Window_Resize(null, null);
		}

		private void DrawClock(double secondsDone, double secondsTotal, bool type, int cycle)
		{
			int
				minutes,
				seconds;
			minutes = Math.DivRem(Convert.ToInt32(type ? secondsDone + 1000 : secondsTotal - secondsDone) / 1000, 60, out seconds);
			Text_Clock.Content = String.Format("{0}:{1,2:D2}", minutes, seconds);
			Text_Cycle.Content = "(Cycle " + cycle + ")";
		}

		private void DrawIcon(double secondsDone, double secondsTotal, bool type, int cycle)
		{
			if (working_)
			{
				double percent = secondsDone / secondsTotal;
				minimise_.SetIcon(IconGen.Generate(percent, Brushes.DarkRed, Brushes.White));
			}
			else
			{
				double percent = secondsDone / secondsTotal;
				minimise_.SetIcon(IconGen.Generate(1.0 - percent, Brushes.White, Brushes.DarkGreen));
			}
		}

		private void DrawCurrentClock()
		{
			if (this.WindowState == WindowState.Minimized)
				DrawIcon((DateTime.Now - startPoint_).TotalMilliseconds, (endPoint_ - startPoint_).TotalMilliseconds, !working_, cycle_);
			else
				DrawClock((DateTime.Now - startPoint_).TotalMilliseconds, (endPoint_ - startPoint_).TotalMilliseconds, !working_, cycle_);
		}

		private void NextCycle(DateTime last)
		{
			if ((working_ = !working_))
				++cycle_;
			this.ResetTimer(working_, last);
		}
		
		private int diffSeconds(TimeSpan ts)
		{
			return ts.Hours * 3600 + ts.Minutes * 60 + ts.Seconds;
		}

		private void ArcTo(double p)
		{
			if (working_)
			{
				workArc_.ArcTo(p, 500.0);
				workGrey_.ArcTo(p, 500.0);
			}
			else
			{
				playArc_.ArcTo(1.0 - p, 500.0);
				playGrey_.ArcTo(1.0 - p, 500.0);
			}
		}

		private void Tick(Object source, RenderingEventArgs e)
		{
			DateTime time = DateTime.Now;
			double tick = (time - lastTick_).TotalMilliseconds;
			if (running_)
			{
				TimeSpan elapsed = time - startPoint_;
				TimeSpan total = endPoint_ - startPoint_;
				if (this.WindowState == WindowState.Minimized)
					DrawIcon(elapsed.TotalMilliseconds, total.TotalMilliseconds, !working_, cycle_);
				else
				{
					if (elapsed.TotalMilliseconds >= nextSecond_)
					{
						nextSecond_ += 1000.0;
						ArcTo(nextSecond_ / (endPoint_ - startPoint_).TotalMilliseconds);
					}
					DrawCurrentClock();
				}
				// Tell the main thread to update the arc soonish.
				if (time >= endPoint_)
				{
					if (settings_.Sound)
					{
						mediaPlayer_.Play();
						mediaPlayer_.Position = new TimeSpan(0);
					}
					minimise_.ShowBaloon("Time's up!");
					this.NextCycle(endPoint_);
				}
			}
			else
			{
				Text_Cycle.Content = "(Paused)";
			}
			if (!working_ && currentPlugin_ != null)
			{
				if (currentPlugin_.Update(tick) || windowDirty_)
					currentPlugin_.Render(Canvas_Clock);
			}
			clockParent_.Children.Clear();
			customRenders_.Tick(clockParent_.Children, tick);
			lastTick_ = time;
			windowDirty_ = false;
		}

		private void Window_Resize(object sender, EventArgs e)
		{
			double
				cwidth = clockParent_.ActualWidth,
				cheight = clockParent_.ActualHeight,
				min = Math.Min(cwidth, cheight),
				radius = min * 0.45,
				x = cwidth * 0.5,
				y = cheight * 0.5,
				stroke = Math.Max(min * 0.01, 5.0);
			workArc_.X = x;
			workArc_.Y = y;
			workArc_.Radius = radius;
			workArc_.Shape = new Path { Stroke = Brushes.DarkRed, StrokeThickness = stroke };
			playArc_.X = x;
			playArc_.Y = y;
			playArc_.Radius = radius;
			playArc_.Shape = new Path { Stroke = Brushes.DarkGreen, StrokeThickness = stroke };
			workGrey_.X = x;
			workGrey_.Y = y;
			workGrey_.Radius = radius;
			workGrey_.Shape = new Path { Stroke = Brushes.LightGray, StrokeThickness = stroke };
			playGrey_.X = x;
			playGrey_.Y = y;
			playGrey_.Radius = radius;
			playGrey_.Shape = new Path { Stroke = Brushes.LightGray, StrokeThickness = stroke };
			windowDirty_ = true;
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IList<Task> oi = e.OldItems.ToIList<Task>();
			IList<Task> ni = new List<Task>();
			if (e.NewItems != null)
			{
				foreach (Task newItem in e.NewItems)
				{
					if (oi.Contains(newItem))
						oi.Remove(newItem);
					else
						ni.Add(newItem);
				}
			}
			if (ni.Count == 0 && oi.Count == 0)
				return;
			// Something moved.
			db_.BeginTransaction();
			foreach (Task newItem in ni)
			{
				db_.Insert(newItem);
				newItem.PropertyChanged += this.OnItemPropertyChanged;
			}
			foreach (Task oldItem in oi)
			{
				db_.Delete(oldItem);
				oldItem.PropertyChanged -= this.OnItemPropertyChanged;
			}
			db_.Commit();
		}

		void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Task item = sender as Task;
			if (item != null)
				db_.Update(item);
		}

		private int GetNewOrder()
		{
			if (tasks_.Count == 0)
				return 0;
			return tasks_[tasks_.Count - 1].Order + 1;
		}

		private void Button_Add_Item_Click(object sender, RoutedEventArgs e)
		{
			string task = Text_Add_Item.Text.Trim();
			if (task.Length != 0)
			{
				//Task t = new Task(task, 
				tasks_.Add(new Task(task, GetNewOrder()));
				Text_Add_Item.Text = "";
			}
			Text_Add_Item.Focus();
		}

		private void Remove_Task_Click(object sender, RoutedEventArgs e)
		{
			Task t = (Task)(sender as Button).Content;
			tasks_.Remove(t);
			Text_Add_Item.Focus();
		}

		private void Up_Task_Click(object sender, RoutedEventArgs e)
		{
			MoveBy((Task)(sender as Button).Content, -1);
		}

		private void Down_Task_Click(object sender, RoutedEventArgs e)
		{
			MoveBy((Task)(sender as Button).Content, 1);
		}

		private void DoPause()
		{
			if (running_)
			{
				//Button_Pause.Content = "Resume";
				//timer_.Enabled = false;
				//App.Tick -= OnTick;
				pausePoint_ = DateTime.Now;
				running_ = false;
				if (settings_.PauseMusic)
					MediaControl.Pause();
			}
			else
			{
				//Button_Pause.Content = "Pause";
				//timer_.Enabled = true;
				//App.Tick += OnTick;
				TimeSpan pf = DateTime.Now - pausePoint_;
				endPoint_ += pf;
				startPoint_ += pf;
				running_ = true;
				if (settings_.PauseMusic)
					MediaControl.Play();
			}
			Text_Add_Item.Focus();
		}

		private void Button_Reset_Click(object sender, RoutedEventArgs e)
		{
			//cycle_ = 1;
			//working_ = true;
			this.ResetTimer(true, DateTime.Now);
			Text_Add_Item.Focus();
		}

		private void Button_Skip_Click(object sender, RoutedEventArgs e)
		{
			this.NextCycle(DateTime.Now);
			Text_Add_Item.Focus();
		}

		private void Button_Pause_Click(object sender, RoutedEventArgs e)
		{
			if (sender == (working_ ? Button_Clock : Button_Small))
				DoPause();
			Text_Add_Item.Focus();
		}

		private void Button_More_Click(object sender, RoutedEventArgs e)
		{
			int
				pw = settings_.WorkSeconds,
				pp = settings_.PlaySeconds;
			if (new MoreWindow(settings_).ShowDialog() == true)
			{
				if (working_ ? pw != settings_.WorkSeconds : pp != settings_.PlaySeconds)
					this.ResetTimer(true, DateTime.Now);
			}
			Text_Add_Item.Focus();
		}

		private void Text_Add_Item_KeyUp(object sender, KeyEventArgs e)
		{
			// If it is an enter - return.
			if (e.Key == Key.Enter || e.Key == Key.Return)
				Button_Add_Item_Click(sender, null);
			else if (e.Key == Key.Escape)
				Text_Add_Item.Text = "";
		}

		private void Text_Add_Item_KeyDown(object sender, KeyEventArgs e)
		{
			// If it is an enter - return.
			if (e.Key == Key.Space && Text_Add_Item.Text.Length == 0)
			{
				DoPause();
				e.Handled = true;
			}
		}

		private void Window_Initialised(object sender, EventArgs e)
		{
			IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
			HwndSource src = HwndSource.FromHwnd(windowHandle);
			src.AddHook(new HwndSourceHook(WndProc));
		}

		private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (msg == SingleInstance.WM_SHOWFIRSTINSTANCE)
			{
				ShowWindow();
			}
			return IntPtr.Zero;
		}

		public void ShowWindow()
		{
			// Insert code here to make your form show itself.
			minimise_.Restore();
			WinApi.ShowToFront(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
		}
	}
}

