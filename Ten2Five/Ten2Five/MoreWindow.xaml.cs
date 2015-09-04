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

namespace Ten2Five
{
    /// <summary>
    /// Interaction logic for TimeSettings.xaml
    /// </summary>
    public partial class MoreWindow : Window
    {
        public MoreWindow()
        {
            InitializeComponent();
        }

        public MoreWindow(ProgSettings settings)
        {
            InitializeComponent();
			List_Plugins.ItemsSource = settings.Plugins;
        }

		private void Configure_Plugin_Click(object sender, RoutedEventArgs e)
		{
			Plugin p = ((sender as Button).Content as Plugin);
			Window w = p.ShowConfigure();
			if (w != null)
				w.ShowDialog();
		}

		private void OK_Button_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private object downOn_ = null;

		private void About_Plugin_Down(object sender, MouseButtonEventArgs e)
		{
			downOn_ = sender;
		}

		private void About_Plugin_Reset(object sender, MouseButtonEventArgs e)
		{
			downOn_ = null;
		}

		private void About_Plugin_Click(object sender, MouseButtonEventArgs e)
		{
			// TextBlock doesn't have a "Click" event, so this fakes one.
			if (downOn_ == sender)
			{
				Plugin p = ((sender as TextBlock).Tag as Plugin);
				Window w = p.ShowAbout();
				if (w != null)
					w.ShowDialog();
			}
			downOn_ = null;
		}
    }
}

