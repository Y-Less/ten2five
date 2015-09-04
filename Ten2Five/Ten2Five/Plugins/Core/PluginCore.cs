using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Ten2Five.Plugins
{
	public class PluginCore : Plugin
	{
		private ProgSettings settings_ = null;

		public PluginCore(ProgSettings s)
		{
			settings_ = s;
		}

		public override Window ShowConfigure()
		{
			return new PluginCoreConfigure(settings_);
		}

		public override Window ShowAbout()
		{
			// Create a window to configure this plugin.
			return new PluginCoreAbout();
		}

		public override void Init(SQLite.SQLiteConnection db)
		{
			//throw new NotImplementedException("Cannot re-init the main window.");
		}

		public override bool Update(double tick)
		{
			//throw new NotImplementedException("Cannot re-update the main window.");
			return false;
		}

		public override void Render(System.Windows.Controls.Canvas parent)
		{
			//throw new NotImplementedException("Cannot re-render the main window.");
			parent.Children.Clear();
		}

		public override string Name { get { return "Ten2Five"; } }
	}
}

