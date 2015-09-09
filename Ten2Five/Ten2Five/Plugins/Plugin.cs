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

