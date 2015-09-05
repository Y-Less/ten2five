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
	/// Based on documentation at http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnwui/html/msdn_icons.asp
	/// </remarks>
	public class IconHolder
	{
		public ICONDIR iconDirectory;
		public ICONIMAGE[] iconImages;

		public IconHolder()
		{
			iconDirectory.Reserved = 0;
			iconDirectory.ResourceType = 1;
			iconDirectory.EntryCount = 1;
			iconImages = new ICONIMAGE[]{new ICONIMAGE()};
		}

		public void Open(string filename)
		{
			this.Open(File.OpenRead(filename));
		}
		
		public void Open(Stream stream)
		{
			using (BinaryReader br = new BinaryReader(stream))
			{
				iconDirectory.Populate(br);
				iconImages = new ICONIMAGE[iconDirectory.EntryCount];
				// Loop through and read in each image
				for(int i=0; i < iconImages.Length; i++)
				{
					// Seek to the location in the file that has the image
					//  SetFilePointer( hFile, pIconDir->idEntries[i].dwImageOffset, NULL, FILE_BEGIN );
					br.BaseStream.Seek(iconDirectory.Entries[i].ImageOffset, SeekOrigin.Begin);
					// Read the image data
					//  ReadFile( hFile, pIconImage, pIconDir->idEntries[i].dwBytesInRes, &dwBytesRead, NULL );
					// Here, pIconImage is an ICONIMAGE structure. Party on it :)
					iconImages[i] = new ICONIMAGE();
					iconImages[i].Populate(br);
				}
			}
		}
		public void Save(string filename)
		{
			using (BinaryWriter bw = new BinaryWriter(File.OpenWrite(filename)))
			{
				this.Save(bw);
			}
		}
		public void Save(BinaryWriter bw)
		{
			iconDirectory.Save(bw);
			for(int i=0; i<iconImages.Length; i++)
				iconImages[i].Save(bw);
		}
		public System.Drawing.Icon ToIcon()
		{
			System.Drawing.Icon newIcon;
			using (BinaryWriter bw = new BinaryWriter(new MemoryStream()))
			{
				this.Save(bw);
				bw.BaseStream.Position = 0;
				newIcon = new System.Drawing.Icon(bw.BaseStream);
			}
			return newIcon;
		}
	}
}
