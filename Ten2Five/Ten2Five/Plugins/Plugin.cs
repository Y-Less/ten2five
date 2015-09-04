using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLite;
using System.Windows.Controls;
using System.Windows;

namespace Ten2Five
{
	public abstract class Plugin
	{
		public abstract Window ShowConfigure();
		public abstract void Init(SQLiteConnection db);
		public abstract bool Update(double tick);
		public abstract void Render(Canvas parent);
		public abstract string Name { get; }
		public abstract Window ShowAbout();
	}
}

