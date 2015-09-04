using System.Windows.Controls;
using System.Windows.Shapes;
using System;
using System.Windows.Media;
using System.Windows;

namespace Ten2Five.Drawing
{
	public class DrawnPartCircle : DrawnShape
	{
		private bool moving_ = false;
		private double target_ = 0.0;
		private double start_ = 0.0;
		private double diff_ = 0.0;
		private double time_ = 0.0;
		private double elapsed_ = 0.0;
		
		private double r_ = 0.0;
		
		private Path p0_ = null;
		private Path p1_ = null;
		
		private void clonePath(Path s)
		{
			p0_ = new Path();
			p1_ = new Path();
			foreach (System.Reflection.PropertyInfo pi in typeof(Ellipse).GetProperties())
			{
				if (pi.CanWrite && pi.CanRead)
				{
					pi.SetValue(p0_, pi.GetValue(s, null), null);
					pi.SetValue(p1_, pi.GetValue(s, null), null);
				}
			}
		}
		
		public override Path Shape
		{
			set { base.Shape = value; clonePath(value); }
			get { return base.Shape; }
		}
		
		public double Radius
		{
			get { return r_; }
			set { r_ = value; }
		}
		
		private double percent_ = 0.0;
		
		public double Percentage
		{
			get { return percent_; }
			set { percent_ = value; moving_ = false; }
		}
		
		private bool right_ = true;
		
		public bool Right
		{
			get { return right_; }
			set { right_ = value; }
		}

		public void ArcTo(double percent, double time)
		{
			/*if (this.Percentage >= percent)
			{
				this.Percentage = percent;
				return;
			}*/
			target_ = percent;
			start_ = this.Percentage;
			diff_ = target_ - start_;
			time_ = time;
			elapsed_ = 0.0;
			moving_ = true;
		}
		
		/**
			Draws an arc, and adds that arc to an element parent for rendering.
			
			x - The x co-ordinate of the arc centre.
			y - The y co-ordinate of the arc centre.
			r - The arc radius.
			p - Percentage of a circle to draw (0.0 - 1.0).
			right - Draw the arc to the right or the left (circle direction).
			shape - Style to draw the arc in.
			
			This is some HORRIBLE code, but gets the job done - I really want to
			improve it, but won't for now.
		**/
		private void DrawArc(UIElementCollection parent, double x, double y, double r, double p, bool right)
		{
			if (right)
			{
				{
					double
						angle = Math.PI - 2 * Math.PI * Math.Min(0.5, p),
						ex = x + r * Math.Sin(angle),
						ey = y + r * Math.Cos(angle);
					var g = new StreamGeometry();
					using (var gc = g.Open())
					{
						gc.BeginFigure(
							startPoint: new Point(x, y - r),
							isFilled: false,
							isClosed: false);
						gc.ArcTo(
							point: new Point(ex, ey),
							size: new Size(r, r),
							rotationAngle: 0.0,
							isLargeArc: false,
							sweepDirection: SweepDirection.Clockwise,
							isStroked: true,
							isSmoothJoin: false);
					}
					p0_.Data = g;
					parent.Add(p0_);
				}
				if (p > 0.5)
				{
					double
						angle = 2 * Math.PI * Math.Min(0.5, p - 0.5),
						ex = x - r * Math.Sin(angle),
						ey = y + r * Math.Cos(angle);
					var g = new StreamGeometry();
					using (var gc = g.Open())
					{
						gc.BeginFigure(
							startPoint: new Point(x, y + r),
							isFilled: false,
							isClosed: false);
						gc.ArcTo(
							point: new Point(ex, ey),
							size: new Size(r, r),
							rotationAngle: 0.0,
							isLargeArc: false,
							sweepDirection: SweepDirection.Clockwise,
							isStroked: true,
							isSmoothJoin: false);
					}
					p1_.Data = g;
					parent.Add(p1_);
				}
			}
			else
			{
				p = 1.0 - p;
				{
					double
						angle = Math.PI - 2 * Math.PI * Math.Min(0.5, p),
						ex = x - r * Math.Sin(angle),
						ey = y + r * Math.Cos(angle);
					var g = new StreamGeometry();
					using (var gc = g.Open())
					{
						gc.BeginFigure(
							startPoint: new Point(ex, ey),
							isFilled: false,
							isClosed: false);
						gc.ArcTo(
							point: new Point(x, y - r),
							size: new Size(r, r),
							rotationAngle: 0.0,
							isLargeArc: false,
							sweepDirection: SweepDirection.Clockwise,
							isStroked: true,
							isSmoothJoin: false);
					}
					p0_.Data = g;
					parent.Add(p0_);
				}
				if (p > 0.5)
				{
					double
						angle = 2 * Math.PI * Math.Min(0.5, p - 0.5),
						ex = x + r * Math.Sin(angle),
						ey = y + r * Math.Cos(angle);
					var g = new StreamGeometry();
					using (var gc = g.Open())
					{
						gc.BeginFigure(
							startPoint: new Point(ex, ey),
							isFilled: false,
							isClosed: false);
						gc.ArcTo(
							point: new Point(x, y + r),
							size: new Size(r, r),
							rotationAngle: 0.0,
							isLargeArc: false,
							sweepDirection: SweepDirection.Clockwise,
							isStroked: true,
							isSmoothJoin: false);
					}
					p1_.Data = g;
					parent.Add(p1_);
				}
			}
		}
		
		public override void Update(double ticks)
		{
			if (moving_)
			{
				// We are moving somehow (i.e. growing).
				elapsed_ += ticks;
				if (elapsed_ >= time_)
					this.Percentage = target_;
				else
					percent_ = start_ + elapsed_ / time_ * diff_;
			}
		}
		
		public override void Render(UIElementCollection parent)
		{
			DrawArc(parent, this.X, this.Y, this.Radius, this.Percentage, this.Right);
		}
	}
}

