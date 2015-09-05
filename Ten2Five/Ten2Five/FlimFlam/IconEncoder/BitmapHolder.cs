// Created by Joshua Flanagan
// http://flimflan.com/blog
// May 2004
//
// You may freely use this code as you wish, I only ask that you retain my name in the source code

using System;
using System.IO;

namespace FlimFlan.IconEncoder
{
	/// <summary>
	/// Provides an in-memory representation of the device independent bitmap format
	/// </summary>
	/// <remarks>
	/// Based on documentation at http://www.whisqu.se/per/docs/graphics52.htm
	/// </remarks>
	public class BitmapHolder
	{
		public BITMAPFILEHEADER fileHeader;
		public BITMAPINFO info;
		public byte[] imageData;

		public void Open(string filename)
		{
			this.Open(File.OpenRead(filename));
		}

		public void Open(Stream stream)
		{
			using (BinaryReader br = new BinaryReader(stream))
			{
				fileHeader.Populate(br);
				info.Populate(br);
				if (info.infoHeader.biSizeImage > 0)
				{
					imageData = br.ReadBytes((int)info.infoHeader.biSizeImage);
				}
				else 
				{
					// can be 0 if the bitmap is in the BI_RGB format
					// in which case you just read all of the remaining data
					imageData = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
				}
			}
		}

		public void Save(string filename)
		{
			this.Save(File.OpenWrite(filename));
		}
		public void Save(Stream stream)
		{
			using (BinaryWriter bw = new BinaryWriter(stream))
			{
				fileHeader.Save(bw);
				info.Save(bw);
				bw.Write(imageData);
			}
		}
	}
}
