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
using M = System.Windows.Media;
using System.Drawing;
using System.Drawing.Imaging;
using FlimFlan.IconEncoder;

namespace Ten2Five.Drawing
{
	public static class IconGen
	{
		private static Brush ConvertBrush(M.Brush b)
		{
			M.SolidColorBrush b2 = b as M.SolidColorBrush;
			if (b2 == null)
				return Brushes.White;
			return new SolidBrush(Color.FromArgb(b2.Color.A, b2.Color.R, b2.Color.G, b2.Color.B));
		}

		public static Icon Generate(double percent, M.Brush c0a, M.Brush c1a)
		{
			Bitmap bmp = new Bitmap(16, 16, PixelFormat.Format24bppRgb);
			using (Graphics g = Graphics.FromImage(bmp))
			{
				//g.FillEllipse(Brushes.Red, 0, 0, 16, 16);
				// Greenscreen effect - set everything to a transparent colour
				// determined by what is at 0,15.
				g.FillRectangle(Brushes.LawnGreen, 0, 0, 16, 16);
				g.FillPie(ConvertBrush(c1a), 1.0f, 1.0f, 14.0f, 14.0f, 0.0f, 360.0f);
				g.FillPie(ConvertBrush(c0a), 1.0f, 1.0f, 14.0f, 14.0f, -90.0f, (float)(360.0f * percent));
			}
			return Converter.BitmapToIcon(bmp);
		}
	}
}

