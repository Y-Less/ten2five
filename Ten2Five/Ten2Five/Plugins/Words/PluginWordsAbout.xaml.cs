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
	/// Interaction logic for PluginWordsAbout.xaml
	/// </summary>
	public partial class PluginWordsAbout : Window
	{
		public PluginWordsAbout()
		{
			InitializeComponent();
		}

		private void OK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private object sawDown_ = null;

		private void Click_MouseDown(object sender, MouseButtonEventArgs e)
		{
			sawDown_ = sender;
		}

		private void Reset_MouseUp(object sender, MouseButtonEventArgs e)
		{
			sawDown_ = null;
		}

		private void Click_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (sender == sawDown_)
			{
				// Finished a click.
				if (sender == Link_MPL)
					GoTo("https://www.mozilla.org/en-US/MPL/1.1/");
			}
			sawDown_ = null;
		}

		private void GoTo(string target)
		{
			System.Diagnostics.Process.Start(target);
		}
	}
}
