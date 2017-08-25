using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Ten2Five.Utils
{
    public class M3UItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int M3U { get; set; }
        public int Order { get; set; }
        public string File { get; set; }
    }

    public class M3UFile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string Name { get; set; }
    }

    public class M3Uhandler
    {
        private readonly StreamWriter stream_;

        private int order_ = 0;
        private readonly int id_;
        private readonly SQLiteConnection db_;

        public M3Uhandler(SQLiteConnection db, string filename, string path = "./")
        {
            db_ = db;
            db_.CreateTable<M3UItem>();
            db_.CreateTable<M3UFile>();

            if (!filename.EndsWith(".m3u"))
                filename = filename + ".m3u";

            if (path.Last() != '/' && path.Last() != '\\')
                path = path + '/';

            if (File.Exists(path + filename))
            {
                // Already exists, don't do anything.
                stream_ = null;
                id_ = -1;
            }
            else
            {
                stream_ = new StreamWriter(path + filename, true, Encoding.UTF8);
                M3UFile
                    found = db_.Find<M3UFile>(x => x.Name == filename);
                if (found == null)
                {
                    found = new M3UFile()
                        {
                            Name = filename,
                        };
                    db_.Insert(found);
                    id_ = found.Id;
                }
                else
                {
                    // Check to see if an M3U with this exact name was ever
                    // written before, and if it was, dump that from the DB
                    // instead of creating a whole new one.
                    id_ = found.Id;
                    foreach (var i in db_.Table<M3UItem>().Where(t => t.M3U == id_))
                    {
                        AddFile(i.File, false);
                    }
                    stream_ = null;
                }
            }
        }

        public void AddFile(string path, bool db = true)
        {
            if (stream_ != null)
            {
                stream_.WriteLine(path);
                stream_.Flush();
                if (db)
                {
                    var
                        todb = new M3UItem()
                        {
                            File = path,
                            Order = order_,
                            M3U = id_,
                        };
                    db_.Insert(todb);
                    ++order_;
                }
            }
        }
    }
}

