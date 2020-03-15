/*
 * Copyright (c) 2015 Alex "Y_Less" Cole
 *
 * This Source Code Form is subject to the terms of the Mozilla Public License,
 * v. 2.0. If a copy of the MPL was not distributed with this file, You can
 * obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Media;

namespace Ten2Five.Drawing
{
	public abstract class DrawnBase
	{
		public abstract bool Enabled { get; set; }
		public abstract double X { get; set; }
		public abstract double Y { get; set; }
		public abstract void Tick(UIElementCollection parent, double ticks);
	}
	
	public class DrawnCollection : DrawnBase
	{
		private double x_ = 0.0;
		private double y_ = 0.0;
		private bool enabled_ = true;
		
		private List<DrawnBase> children_ = new List<DrawnBase>();
		
		public override bool Enabled
		{
			set
			{
				foreach (DrawnBase c in children_)
					c.Enabled = value;
				enabled_ = value;
			}
			get { return enabled_; }
		}
		
		public override double X
		{
			set
			{
				double diff = value - x_;
				foreach (DrawnBase c in children_)
					c.X += diff;
				x_ = value;
			}
			get { return x_; }
		}
		
		public override double Y
		{
			set
			{
				double diff = value - y_;
				foreach (DrawnBase c in children_)
					c.Y += diff;
				y_ = value;
			}
			get { return y_; }
		}
		
		public void Add(DrawnBase s)
		{
			children_.Add(s);
		}
		
		public void Remove(DrawnBase s)
		{
			children_.Remove(s);
		}
		
		public override void Tick(UIElementCollection parent, double ticks)
		{
			foreach (DrawnBase c in children_)
				c.Tick(parent, ticks);
		}
	}
	
	public abstract class DrawnShape : DrawnBase
	{
		private double x_ = 0.0;
		private double y_ = 0.0;
		private Path s_ = null;
		private bool enabled_ = true;
		
		public override bool Enabled
		{
			set { enabled_ = value; }
			get { return enabled_; }
		}

		public override double X
		{
			set { x_ = value; }
			get { return x_; }
		}

		public override double Y
		{
			set { y_ = value; }
			get { return y_; }
		}
		
		public virtual Path Shape
		{
			set { s_ = value; }
			get { return s_; }
		}
		
		public override void Tick(UIElementCollection parent, double ticks)
		{
			this.Update(ticks);
			// If there is no details on how this arc should look, then there is
			// no point in rendering it (forget "no point", it is impossible).
			if (this.Enabled && s_ != null)
				this.Render(parent);
		}
		
		public abstract void Update(double ticks);
		public abstract void Render(UIElementCollection parent);
	}
}

