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
using System.Collections;
using System.Drawing;

namespace FlimFlan.IconEncoder
{
	/// <summary>
	/// Provides methods for converting between the bitmap and icon formats
	/// </summary>
	public class Converter
	{
		private Converter(){}
		public static Icon BitmapToIcon(Bitmap b)
		{
			IconHolder ico = BitmapToIconHolder(b);
			Icon newIcon;
			using (BinaryWriter bw = new BinaryWriter(new MemoryStream()))
			{
				ico.Save(bw);
				bw.BaseStream.Position = 0;
				newIcon = new Icon(bw.BaseStream);
			}
			return newIcon;
		}

		public static IconHolder BitmapToIconHolder(Bitmap b)
		{
			BitmapHolder bmp = new BitmapHolder();;
			using (MemoryStream stream = new MemoryStream())
			{
				b.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
				stream.Position = 0;
				bmp.Open(stream);
			}
			return BitmapToIconHolder(bmp);
		}

		public static IconHolder BitmapToIconHolder(BitmapHolder bmp)
		{
			bool mapColors = (bmp.info.infoHeader.biBitCount <= 24);
			int maximumColors = 1 << bmp.info.infoHeader.biBitCount;
			//Hashtable uniqueColors = new Hashtable(maximumColors);
			// actual colors is probably nowhere near maximum, so dont try to initialize the hashtable
			Hashtable uniqueColors = new Hashtable();
			
			int sourcePosition = 0;
			int numPixels = bmp.info.infoHeader.biHeight * bmp.info.infoHeader.biWidth;
			byte[] indexedImage = new byte[numPixels];
			byte colorIndex;

			if (mapColors)
			{
				for (int i=0; i < indexedImage.Length; i++)
				{
					//TODO: currently assumes source bitmap is 24bit color
					//read 3 bytes, convert to color
					byte[] pixel = new byte[3];
					Array.Copy(bmp.imageData, sourcePosition, pixel, 0, 3);
					sourcePosition += 3;

					RGBQUAD color = new RGBQUAD(pixel);
					if (uniqueColors.Contains(color))
					{
						colorIndex = Convert.ToByte(uniqueColors[color]);
					}
					else 
					{
						if (uniqueColors.Count > byte.MaxValue) 
						{
							throw new NotSupportedException(String.Format("The source image contains more than {0} colors.", byte.MaxValue));
						}
						colorIndex = Convert.ToByte(uniqueColors.Count);
						uniqueColors.Add(color, colorIndex);
					}
					// store pixel as an index into the color table
					indexedImage[i] = colorIndex;
				}
			}
			else 
			{
				// added by Pavel Janda on 14/11/2006
				if (bmp.info.infoHeader.biBitCount == 32) 
				{
					for (int i=0; i < indexedImage.Length; i++) 
					{
						//TODO: currently assumes source bitmap is 32bit color with alpha set to zero
						//ignore first byte, read another 3 bytes, convert to color
						byte[] pixel = new byte[4];
						Array.Copy(bmp.imageData, sourcePosition, pixel, 0, 4);
						sourcePosition += 4;

						RGBQUAD color = new RGBQUAD(pixel[0], pixel[1], pixel[2], pixel[3]);
						if (uniqueColors.Contains(color)) 
						{
							colorIndex = Convert.ToByte(uniqueColors[color]);
						}
						else 
						{
							if (uniqueColors.Count > byte.MaxValue) 
							{
								throw new NotSupportedException(String.Format("The source image contains more than {0} colors.", byte.MaxValue));
							}
							colorIndex = Convert.ToByte(uniqueColors.Count);
							uniqueColors.Add(color, colorIndex);
						}
						// store pixel as an index into the color table
						indexedImage[i] = colorIndex;
					}
					// end of addition
				} 
				else 
				{
					//TODO: implement converting an indexed bitmap
					throw new NotImplementedException("Unable to convert indexed bitmaps.");
				}
			}
			
			ushort bitCount = getBitCount(uniqueColors.Count);
			// *** Build Icon ***
			IconHolder ico = new IconHolder();
			ico.iconDirectory.Entries = new ICONDIRENTRY[1];
			//TODO: is it really safe to assume the bitmap width/height are bytes?
			ico.iconDirectory.Entries[0].Width = (byte) bmp.info.infoHeader.biWidth;
			ico.iconDirectory.Entries[0].Height = (byte) bmp.info.infoHeader.biHeight;
			ico.iconDirectory.Entries[0].BitCount = bitCount; // maybe 0?
			ico.iconDirectory.Entries[0].ColorCount = (uniqueColors.Count > byte.MaxValue) ? (byte)0 : (byte)uniqueColors.Count;
			//HACK: safe to assume that the first imageoffset is always 22
			ico.iconDirectory.Entries[0].ImageOffset = 22;
			ico.iconDirectory.Entries[0].Planes = 0;
			ico.iconImages[0].Header.biBitCount = bitCount;
			ico.iconImages[0].Header.biWidth = ico.iconDirectory.Entries[0].Width;
			// height is doubled in header, to account for XOR and AND
			ico.iconImages[0].Header.biHeight = ico.iconDirectory.Entries[0].Height << 1;
			ico.iconImages[0].XOR = new byte[ico.iconImages[0].numBytesInXor()];
			ico.iconImages[0].AND = new byte[ico.iconImages[0].numBytesInAnd()];
			ico.iconImages[0].Header.biSize = 40; // always
			ico.iconImages[0].Header.biSizeImage = (uint)ico.iconImages[0].XOR.Length;
			ico.iconImages[0].Header.biPlanes = 1;
			ico.iconImages[0].Colors = buildColorTable(uniqueColors, bitCount);
			//BytesInRes = biSize + colors * 4 + XOR + AND
			ico.iconDirectory.Entries[0].BytesInRes = (uint)(ico.iconImages[0].Header.biSize
				+ (ico.iconImages[0].Colors.Length * 4)
				+ ico.iconImages[0].XOR.Length
				+ ico.iconImages[0].AND.Length);
			
			// copy image data
			int bytePosXOR = 0;
			int bytePosAND = 0;
			byte transparentIndex = 0;
			transparentIndex = indexedImage[0];
			//initialize AND
			ico.iconImages[0].AND[0] = byte.MaxValue;

			int pixelsPerByte;
			int bytesPerRow; // must be a long boundary (multiple of 4)
			int[] shiftCounts;

			switch (bitCount)
			{
				case 1:
					pixelsPerByte = 8;
					shiftCounts = new int[] {7, 6, 5, 4, 3, 2, 1, 0};
					break;
				case 4:
					pixelsPerByte = 2;
					shiftCounts = new int[] {4, 0};
					break;
				case 8:
					pixelsPerByte = 1;
					shiftCounts = new int[] {0};
					break;
				default:
					throw new NotSupportedException("Bits per pixel must be 1, 4, or 8");
			}
			bytesPerRow = ico.iconDirectory.Entries[0].Width / pixelsPerByte;
			int padBytes = bytesPerRow % 4;
			if (padBytes > 0)
				padBytes = 4 - padBytes;

			byte currentByte;
			sourcePosition = 0;
			for (int row=0; row < ico.iconDirectory.Entries[0].Height; ++row)
			{
				for (int rowByte=0; rowByte < bytesPerRow; ++rowByte)
				{
					currentByte = 0;
					for (int pixel=0; pixel < pixelsPerByte; ++pixel)
					{
						byte index = indexedImage[sourcePosition++];
						byte shiftedIndex = (byte)(index << shiftCounts[pixel]);
						currentByte |= shiftedIndex;
					}
					ico.iconImages[0].XOR[bytePosXOR] = currentByte;
					++bytePosXOR;
				}
				// make sure each scan line ends on a long boundary
				bytePosXOR += padBytes;
			}

			for(int i=0; i<indexedImage.Length; i++)
			{
				byte index = indexedImage[i];
				int bitPosAND = 128 >> (i % 8);
				if (index != transparentIndex)
					ico.iconImages[0].AND[bytePosAND] ^= (byte)bitPosAND;
				if (bitPosAND == 1)
				{
					// need to start another byte for next pixel
					if (bytePosAND % 2 ==1)
					{
						//TODO: fix assumption that icon is 16px wide
						//skip some bytes so that scanline ends on a long barrier
						bytePosAND += 3;
					}
					else 
					{
						bytePosAND += 1;
					}
					if (bytePosAND < ico.iconImages[0].AND.Length)
						ico.iconImages[0].AND[bytePosAND] = byte.MaxValue;
				}
			}
			return ico;
		}

		private static ushort getBitCount(int uniqueColorCount)
		{
			if (uniqueColorCount <= 2)
			{
				return 1;
			} 
			if (uniqueColorCount <= 16) 
			{
				return 4;
			}
			if (uniqueColorCount <= 256)
			{
				return 8;
			}
			return 24;
		}

		private static RGBQUAD[] buildColorTable(Hashtable colors, ushort bpp)
		{
			//RGBQUAD[] colorTable = new RGBQUAD[colors.Count];
			//HACK: it looks like the color array needs to be the max size based on bitcount
			int numColors = 1 << bpp;
			RGBQUAD[] colorTable = new RGBQUAD[numColors];
			foreach(RGBQUAD color in colors.Keys)
			{
				int colorIndex = Convert.ToInt32( colors[color] );
				colorTable[ colorIndex ] = color;
			}
			return colorTable;
		}

		//		public static BitmapHolder IconToBitmap(IconHolder ico)
		//		{
		//			//TODO: implement
		//			return new BitmapHolder();
		//		}
	}
}
