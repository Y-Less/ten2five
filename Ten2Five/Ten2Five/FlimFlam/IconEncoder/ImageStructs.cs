// Created by Joshua Flanagan
// http://flimflan.com/blog
// May 2004
//
// You may freely use this code as you wish, I only ask that you retain my name in the source code

// Modified by Pavel Janda
// - added support for 32bpp bitmaps
// November 2006

using System;
using System.IO;

using BYTE = System.Byte;
using WORD = System.UInt16;
using DWORD = System.UInt32;
using LONG = System.Int32;

namespace FlimFlan.IconEncoder
{
	public struct ICONIMAGE
	{
		/// <summary>
		/// icHeader: DIB format header
		/// </summary>
		public BITMAPINFOHEADER   Header;
		/// <summary>
		/// icColors: Color table
		/// </summary>
		public RGBQUAD[]         Colors;
		/// <summary>
		/// icXOR: DIB bits for XOR mask
		/// </summary>
		public BYTE[]            XOR;
		/// <summary>
		/// icAND: DIB bits for AND mask
		/// </summary>
		public BYTE[]            AND;

		public void Populate(BinaryReader br)
		{
			// read in the header
			this.Header.Populate(br);
			this.Colors = new RGBQUAD[Header.biClrUsed];
			// read in the color table
			for(int i=0; i<this.Header.biClrUsed; ++i)
			{
				this.Colors[i].Populate(br);
			}
			// read in the XOR mask
			this.XOR = br.ReadBytes(numBytesInXor());
			// read in the AND mask
			this.AND = br.ReadBytes(numBytesInAnd());
		}

		public void Save(BinaryWriter bw)
		{
			Header.Save(bw);
			for(int i=0; i<Colors.Length; i++)
				Colors[i].Save(bw);
			bw.Write(XOR);
			bw.Write(AND);
		}

		#region byte count calculation functions
		public int numBytesInXor()
		{
			// number of bytes per pixel depends on bitcount
			int bytesPerLine = Convert.ToInt32(Math.Ceiling((Header.biWidth * Header.biBitCount) / 8.0));
			// If necessary, a scan line must be zero-padded to end on a 32-bit boundary.			
			// so there will be some padding, if the icon is less than 32 pixels wide
			int padding = (bytesPerLine % 4);
			if (padding > 0)
				bytesPerLine += (4 - padding);
			return bytesPerLine * (Header.biHeight >> 1);

		}
		public int numBytesInAnd()
		{
			// each byte can hold 8 pixels (1bpp)
			// check for a remainder
			int bytesPerLine = Convert.ToInt32(Math.Ceiling(Header.biWidth / 8.0));
			// If necessary, a scan line must be zero-padded to end on a 32-bit boundary.			
			// so there will be some padding, if the icon is less than 32 pixels wide
			int padding = (bytesPerLine % 4);
			if (padding > 0)
				bytesPerLine += (4 - padding);
			return bytesPerLine * (Header.biHeight >> 1);
		}
		#endregion
	}

	public struct ICONDIR
	{
		/// <summary>
		/// idReserved: Always 0
		/// </summary>
		public WORD			Reserved;   // Reserved
		/// <summary>
		/// idType: Resource type (Always 1 for icons)
		/// </summary>
		public WORD			ResourceType;
		/// <summary>
		/// idCount: Number of images in directory
		/// </summary>
		public WORD			EntryCount;
		/// <summary>
		/// idEntries: Directory entries for each image
		/// </summary>
		public ICONDIRENTRY[]	Entries;

		public void Save(BinaryWriter bw)
		{
			bw.Write(Reserved);
			bw.Write(ResourceType);
			bw.Write(EntryCount);
			for (int i=0; i<Entries.Length; ++i)
				Entries[i].Save(bw);
		}

		public void Populate(BinaryReader br)
		{
			Reserved = br.ReadUInt16();
			ResourceType = br.ReadUInt16();
			EntryCount = br.ReadUInt16();
			Entries = new ICONDIRENTRY[this.EntryCount];
			for (int i=0; i < Entries.Length; i++)
			{
				Entries[i].Populate(br);
			}
		}
	}

	public struct ICONDIRENTRY
	{
		/// <summary>
		/// bWidth: In pixels.  Must be 16, 32, or 64
		/// </summary>
		public BYTE	Width;
		/// <summary>
		/// bHeight: In pixels.  Must be 16, 32, or 64
		/// </summary>
		public BYTE	Height;
		/// <summary>
		/// bColorCount: Number of colors in image (0 if >=8bpp)
		/// </summary>
		public BYTE	ColorCount;
		/// <summary>
		/// bReserved: Must be zero
		/// </summary>
		public BYTE	Reserved;
		/// <summary>
		/// wPlanes: Number of color planes in the icon bitmap
		/// </summary>
		public WORD	Planes;
		/// <summary>
		/// wBitCount: Number of bits in each pixel of the icon.  Must be 1,4,8, or 24
		/// </summary>
		public WORD	BitCount;
		/// <summary>
		/// dwBytesInRes: Number of bytes in the resource
		/// </summary>
		public DWORD BytesInRes;
		/// <summary>
		/// dwImageOffset: Number of bytes from the beginning of the file to the image
		/// </summary>
		public DWORD ImageOffset;
		
		public void Save(BinaryWriter bw)
		{
			bw.Write(Width);
			bw.Write(Height);
			bw.Write(ColorCount);
			bw.Write(Reserved);
			bw.Write(Planes);
			bw.Write(BitCount);
			bw.Write(BytesInRes);
			bw.Write(ImageOffset);
		}
	
		public void Populate(BinaryReader br)
		{
			Width = br.ReadByte();
			Height = br.ReadByte();
			ColorCount = br.ReadByte();
			Reserved = br.ReadByte();
			Planes = br.ReadUInt16();
			BitCount = br.ReadUInt16();
			BytesInRes = br.ReadUInt32();
			ImageOffset = br.ReadUInt32();
		}
	}


	public struct BITMAPFILEHEADER 
	{
		public WORD    Type;
		public DWORD   Size;
		public WORD    Reserved1;
		public WORD    Reserved2;
		public DWORD   OffBits;

		public void Populate(BinaryReader br)
		{
			Type = br.ReadUInt16();
			Size = br.ReadUInt32();
			Reserved1 = br.ReadUInt16();
			Reserved2 = br.ReadUInt16();
			OffBits = br.ReadUInt32();
		}

		public void Save(BinaryWriter bw)
		{
			bw.Write(Type);
			bw.Write(Size);
			bw.Write(Reserved1);
			bw.Write(Reserved2);
			bw.Write(OffBits);
		}

	}
	public struct BITMAPINFO 
	{
		public BITMAPINFOHEADER    infoHeader;
		public RGBQUAD[]             colorMap;

		public void Populate(BinaryReader br)
		{
			infoHeader.Populate(br);
			colorMap = new RGBQUAD[getNumberOfColors()];
			// read in the color table
			for(int i=0; i<colorMap.Length; ++i)
			{
				colorMap[i].Populate(br);
			}
		}
		public void Save(BinaryWriter bw)
		{
			infoHeader.Save(bw);
			for(int i=0; i<colorMap.Length; i++)
				colorMap[i].Save(bw);
		}

		private uint getNumberOfColors() 
		{
			if (infoHeader.biClrUsed > 0)
			{
				// number of colors is specified
				return infoHeader.biClrUsed;
			}
			else
			{
				// number of colors is based on the bitcount
				switch(infoHeader.biBitCount)
				{
					case 1:
						return 2;
					case 4:
						return 16;
					case 8:
						return 256;
					default:
						return 0;
				}
			}
		}
	}
	
	/// <summary>
	/// Describes the format of the bitmap image
	/// </summary>
	/// <remarks>
	/// BITMAPHEADERINFO struct
	/// referenced by http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnwui/html/msdn_icons.asp
	/// defined by http://www.whisqu.se/per/docs/graphics52.htm
	/// Only the following members are used: biSize, biWidth, biHeight, biPlanes, biBitCount, biSizeImage. All other members must be 0. The biHeight member specifies the combined height of the XOR and AND masks. The members of icHeader define the contents and sizes of the other elements of the ICONIMAGE structure in the same way that the BITMAPINFOHEADER structure defines a CF_DIB format DIB
	/// </remarks>
	public struct  BITMAPINFOHEADER
	{
		public const int Size = 40;
		public DWORD  biSize;
		public LONG   biWidth;
		/// <summary>
		/// Height of bitmap.  For icons, this is the height of XOR and AND masks together. Divide by 2 to get true height.
		/// </summary>
		public LONG   biHeight;
		public WORD   biPlanes;
		public WORD   biBitCount;
		public DWORD  biCompression;
		public DWORD  biSizeImage;
		public LONG   biXPelsPerMeter;
		public LONG   biYPelsPerMeter;
		public DWORD  biClrUsed;
		public DWORD  biClrImportant;

		public void Save(BinaryWriter bw)
		{
			bw.Write(biSize);
			bw.Write(biWidth);
			bw.Write(biHeight);
			bw.Write(biPlanes);
			bw.Write(biBitCount);
			bw.Write(biCompression);
			bw.Write(biSizeImage);
			bw.Write(biXPelsPerMeter);
			bw.Write(biYPelsPerMeter);
			bw.Write(biClrUsed);
			bw.Write(biClrImportant);
		}

		public void Populate(BinaryReader br)
		{
			biSize = br.ReadUInt32();
			biWidth = br.ReadInt32();
			biHeight = br.ReadInt32();
			biPlanes = br.ReadUInt16();
			biBitCount = br.ReadUInt16();
			biCompression = br.ReadUInt32();
			biSizeImage = br.ReadUInt32();
			biXPelsPerMeter = br.ReadInt32();
			biYPelsPerMeter = br.ReadInt32();
			biClrUsed = br.ReadUInt32();
			biClrImportant = br.ReadUInt32();
		}
	} 

	// RGBQUAD structure changed by Pavel Janda on 14/11/2006
	public struct RGBQUAD
	{
		public const int Size = 4;
		public BYTE blue;
		public BYTE green;
		public BYTE red;
		public BYTE alpha;

		public RGBQUAD(BYTE[] bgr) : this(bgr[0], bgr[1], bgr[2]){}

		public RGBQUAD(BYTE blue, BYTE green, BYTE red)
		{
			this.blue = blue;
			this.green = green;
			this.red = red;
			this.alpha = 0;
		}

		public RGBQUAD(BYTE blue, BYTE green, BYTE red, BYTE alpha) 
		{
			this.blue = blue;
			this.green = green;
			this.red = red;
			this.alpha = alpha;
		}

		public void Save(BinaryWriter bw)
		{
			bw.BaseStream.WriteByte(blue);
			bw.BaseStream.WriteByte(green);
			bw.BaseStream.WriteByte(red);
			bw.BaseStream.WriteByte(alpha);
		}

		public void Populate(BinaryReader br)
		{
			blue = br.ReadByte();
			green = br.ReadByte();
			red = br.ReadByte();
			alpha = br.ReadByte();
		}
	}

}
