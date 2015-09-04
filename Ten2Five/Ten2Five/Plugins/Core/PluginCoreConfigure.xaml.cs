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

namespace Ten2Five.Plugins
{
	/// <summary>
	/// Interaction logic for PluginCoreConfigure.xaml
	/// </summary>
	public partial class PluginCoreConfigure : Window
	{
		private ProgSettings settings_;

		public PluginCoreConfigure(ProgSettings settings)
		{
			InitializeComponent();
			settings_ = settings;
			TB_Work.Text = "" + settings.WorkSeconds;
			TB_Play.Text = "" + settings.PlaySeconds;
			CB_PlaySounds.IsChecked = settings.Sound;
			CB_PauseMusic.IsChecked = settings.PauseMusic;
			TB_Work.Focus();
		}

		private void OK_Button_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				int
                    ws = Int32.Parse(TB_Work.Text),
                    ps = Int32.Parse(TB_Play.Text);
				if (ws < 1 || ps < 1)
					MessageBox.Show("Both times must be positive integers.", "Time Settings");
				else
				{
					settings_.PlaySeconds = ps;
					settings_.WorkSeconds = ws;
					settings_.Sound = CB_PlaySounds.IsChecked == true;
					settings_.PauseMusic = CB_PauseMusic.IsChecked == true;
					this.DialogResult = true;
					return;
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Both times must be positive integers.", "Time Settings");
			}
			this.DialogResult = false;
		}

		private void Cancel_Button_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
		}

		private void TB_GotFocus(object sender, RoutedEventArgs e)
		{
			(sender as TextBox).SelectAll();
		}

		private void TB_KeyUp(object sender, KeyEventArgs e)
		{
			// If it is an enter - return.
			if (e.Key == Key.Enter || e.Key == Key.Return)
				OK_Button_Click(sender, null);
			else if (e.Key == Key.Escape)
				Cancel_Button_Click(sender, null);
		}

	}
}
