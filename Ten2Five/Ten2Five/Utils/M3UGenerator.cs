using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Ten2Five.Plugins;

namespace Ten2Five.Utils
{
    class M3UGenerator
    {
        public static IEnumerable<T> Concatenate<T>(params IEnumerable<T>[] lists)
        {
            return lists.SelectMany(x => x);
        }

        private static Random random = new Random();

        public static T GetRandom<T>(IList<T> list)
        {
            return list[random.Next(0, list.Count)];
        }

        public static void GenerateForToday(SQLiteConnection db, ObservableCollection<WordMap> words)
        {
            if (words.Count == 0)
                return;
            DateTimeFormatInfo
                dtfi = CultureInfo.CreateSpecificCulture("en-GB").DateTimeFormat;
            dtfi.ShortDatePattern = @"dd-MM-yyyy";
            DateTime
                now = DateTime.Now;
            string
                filename = now.ToString("d", dtfi);
            M3UHandler
                m3u = new M3UHandler(db, filename, "./Playlists");
            if (m3u.Done)
                return;
            // Get the start of the week (about midnight on sunday).
//            DateTime
//                startOfWeek = now.AddDays(-(int)now.DayOfWeek - now.TimeOfDay.TotalDays);
//            // Get all the words older than this cutoff (so that we aren't
//            // including newly added words just yet (actually, why not).
            IOrderedEnumerable<WordMap>
                ws = words.OrderByDescending(x => x.Added);
            // The newest 50 words twice.
            // The second oldest 50 words once.
            List<WordMap>
                olderWords;
            foreach (WordMap w in Concatenate(ws.Take(50), ws.Take(50), ws.Skip(50).Take(50)).ToList())
            {
                Console.WriteLine("Adding: " + w.Id + " = " + w.Meaning);
                m3u.AddFile("../Clips/" + w.Id + "_English_Russian.mp3");
                m3u.AddFile("../Clips/" + w.Id + "_Russian_English.mp3");
            }
            // A random 15 of the third oldest 50 words.
            olderWords = ws.Skip(100).Take(50).ToList();
            if (olderWords.Count != 0)
                for (int i = 0; i != 15; ++i)
                {
                    WordMap
                        w = GetRandom(olderWords);
                    Console.WriteLine("Adding: " + w.Id + " = " + w.Meaning);
                    m3u.AddFile("../Clips/" + w.Id + "_English_Russian.mp3");
                    m3u.AddFile("../Clips/" + w.Id + "_Russian_English.mp3");
                }
            // A random 15 of all the older words.
            olderWords = ws.Skip(150).ToList();
            if (olderWords.Count != 0)
                for (int i = 0; i != 15; ++i)
                {
                    WordMap
                        w = GetRandom(olderWords);
                    Console.WriteLine("Adding: " + w.Id + " = " + w.Meaning);
                    m3u.AddFile("../Clips/" + w.Id + "_English_Russian.mp3");
                    m3u.AddFile("../Clips/" + w.Id + "_Russian_English.mp3");
                }
            // A random 20 of all the words.
            if (olderWords.Count != 0)
                for (int i = 0; i != 20; ++i)
                {
                    WordMap
                        w = GetRandom(words);
                    Console.WriteLine("Adding: " + w.Id + " = " + w.Meaning);
                    m3u.AddFile("../Clips/" + w.Id + "_English_Russian.mp3");
                    m3u.AddFile("../Clips/" + w.Id + "_Russian_English.mp3");
                }
            m3u.Dispose();
        }
    }
}

