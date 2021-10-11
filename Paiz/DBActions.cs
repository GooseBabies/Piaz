using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace Paiz
{
    public class DBActions
    {
        //case-sesitive compare      COLLATE Latin1_General_CS_AS
        private readonly string BackupLocation = "C:\\Users\\Chris\\AppData\\Roaming\\Paiz\\Backup";
        private readonly string SQLiteConnectionString = @"URI=file:C:\Users\Chris\AppData\Roaming\Paiz\Database\nevada.db";

        #region SQLite

        private SQLiteConnection SQLiteConn;

        public void SQLiteConnect()
        {
            SQLiteConn = new(SQLiteConnectionString);
            SQLiteConn.Open();
        }

        private void CreateImageDataTable()
        {
            using SQLiteCommand cmd = new(SQLiteConn);

            cmd.CommandText = @"CREATE TABLE image_data(id INTEGER PRIMARY KEY, sha_id TEXT, path TEXT, name TEXT, ext TEXT, date_added NUMERIC, date_modified NUMERIC, height INTEGER, width INTEGER, rating INTEGER, tag_list TEXT, sources TEXT, IQDB INTEGER, phash TEXT, correlated INTEGER, video INTEGER, sound INTEGER, duration INTEGER, frame_rate REAL, md5 TEXT)";
            cmd.ExecuteNonQuery();
        }

        private void CreateTagDataTable()
        {
            using SQLiteCommand cmd = new(SQLiteConn);

            cmd.CommandText = @"CREATE TABLE tag_data(tagid INTEGER PRIMARY KEY, tag_name TEXT, tag_display TEXT, tag_count INTEGER, category_id INTEGER)";
            cmd.ExecuteNonQuery();
        }

        private void CreateDupesTable()
        {
            using SQLiteCommand cmd = new(SQLiteConn);

            cmd.CommandText = @"CREATE TABLE dupes(id INTEGER PRIMARY KEY, hash_1 TEXT, hash_2 TEXT, score REAL, processed INTEGER)";
            cmd.ExecuteNonQuery();
        }

        private void CreateDeletedImagesTable()
        {
            using SQLiteCommand cmd = new(SQLiteConn);

            cmd.CommandText = @"CREATE TABLE deleted_images(id INTEGER PRIMARY KEY, hash TEXT, deleted INTEGER, date_added NUMERIC)";
            cmd.ExecuteNonQuery();
        }

        private void CreateSettingsTable()
        {
            using SQLiteCommand cmd = new(SQLiteConn);

            cmd.CommandText = @"CREATE TABLE settings(id INTEGER PRIMARY KEY, main_list_orderby INTEGER, last_file_opened TEXT, closing_job INTEGER, last_backup NUMERIC, volume REAL, database_version INTEGER)";
            cmd.ExecuteNonQuery();
        }

        private void CreateParentDataTable()
        {
            using SQLiteCommand cmd = new(SQLiteConn);

            cmd.CommandText = @"CREATE TABLE parent_data(child_id INTEGER, parent_id INTEGER, retro INTEGER)";
            cmd.ExecuteNonQuery();
        }

        private void CreateSiblingDataTable()
        {
            using SQLiteCommand cmd = new(SQLiteConn);

            cmd.CommandText = @"CREATE TABLE sibling_data(old_id INTEGER, new_id INTEGER, retro INTEGER)";
            cmd.ExecuteNonQuery();
        }

        private void CreateBooruTagMapTable()
        {
            using SQLiteCommand cmd = new(SQLiteConn);

            cmd.CommandText = @"CREATE TABLE booru_tag_map(booru_tag TEXT PRIMARY KEY, tagid INTEGER)";
            cmd.ExecuteNonQuery();
        }

        public void CreateSQLiteTables()
        {
            CreateImageDataTable();
            CreateTagDataTable();
            CreateParentDataTable();
            CreateSiblingDataTable();
            CreateDupesTable();
            CreateDeletedImagesTable();
            CreateSettingsTable();
            CreateBooruTagMapTable();
        }

        public void SQLiteClose()
        {
            SQLiteConn.Close();
            SQLiteConn.Dispose();
        }

        public void ImageDataInsert(string sha_id, string path, string name, string ext, DateTime date_added, DateTime date_modified, int height, int width, int rating, string tag_list, bool IQDB, string phash, bool correlated, bool video, bool sound, int duration, float frame_rate, string md5)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO image_data(sha_id, path, name, ext, date_added, date_modified, height, width, rating, tag_list, IQDB, phash, correlated, video, sound, duration, frame_rate, md5) VALUES (@sha_id, @path, @name, @ext, @date_added, @date_modified, @height, @width, @rating, @tag_list, @IQDB, @phash, @correlated, @video, @sound, @duration, @frame_rate, @md5)";

            cmd.Parameters.AddWithValue("@sha_id", sha_id);
            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@ext", ext);
            cmd.Parameters.AddWithValue("@date_added", date_added);
            cmd.Parameters.AddWithValue("@date_modified", date_modified);
            cmd.Parameters.AddWithValue("@height", height);
            cmd.Parameters.AddWithValue("@width", width);
            cmd.Parameters.AddWithValue("@rating", rating);
            cmd.Parameters.AddWithValue("@tag_list", tag_list);
            cmd.Parameters.AddWithValue("@IQDB", IQDB);
            cmd.Parameters.AddWithValue("@phash", phash);
            cmd.Parameters.AddWithValue("@correlated", correlated);
            cmd.Parameters.AddWithValue("@video", video);
            cmd.Parameters.AddWithValue("@sound", sound);
            cmd.Parameters.AddWithValue("@duration", duration);
            cmd.Parameters.AddWithValue("@frame_rate", frame_rate);
            cmd.Parameters.AddWithValue("@md5", md5);
            cmd.Prepare();

            cmd.ExecuteNonQuery();
        }

        public void BooruTagMapInsert(string booru_tag, int tagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO booru_tag_map(booru_tag, tagid) VALUES (@booru_tag, @tagid)";

            cmd.Parameters.AddWithValue("@booru_tag", booru_tag);
            cmd.Parameters.AddWithValue("@tagid", tagid);
            cmd.Prepare();

            cmd.ExecuteNonQuery();
        }

        public void DeletedImagesInsert(string hash, bool deleted, DateTime date_added)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO deleted_images(hash, deleted, date_added) VALUES (@hash, @deleted, @date_added)";

            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@deleted", deleted);
            cmd.Parameters.AddWithValue("@date_added", date_added);
            cmd.Prepare();

            cmd.ExecuteNonQuery();
        }

        public void DupesInsert(string hash_1, string hash_2, float score, bool processed)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO dupes(hash_1, hash_2, score, processed) VALUES (@hash_1, @hash_2, @score, @processed)";

            cmd.Parameters.AddWithValue("@hash_1", hash_1);
            cmd.Parameters.AddWithValue("@hash_2", hash_2);
            cmd.Parameters.AddWithValue("@score", score);
            cmd.Parameters.AddWithValue("@processed", processed);
            cmd.Prepare();

            cmd.ExecuteNonQuery();
        }

        public void ParentDataInsert(int child_id, int parent_id, bool retro)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO parent_data(child_id, parent_id, retro) VALUES (@child_id, @parent_id, @retro)";

            cmd.Parameters.AddWithValue("@child_id", child_id);
            cmd.Parameters.AddWithValue("@parent_id", parent_id);
            cmd.Parameters.AddWithValue("@retro", retro);
            cmd.Prepare();

            cmd.ExecuteNonQuery();
        }
        public void SiblingDataInsert(int old_id, int new_id, bool retro)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO sibling_data(old_id, new_id, retro) VALUES (@old_id, @new_id, @retro)";

            cmd.Parameters.AddWithValue("@old_id", old_id);
            cmd.Parameters.AddWithValue("@new_id", new_id);
            cmd.Parameters.AddWithValue("@retro", retro);
            cmd.Prepare();

            cmd.ExecuteNonQuery();
        }

        public void TagDataInsert(int tagid, string tag_name, string tag_display, int tag_count, int category_id)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO tag_data(tagid, tag_name, tag_display, tag_count, category_id) VALUES (@tagid, @tag_name, @tag_display, @tag_count, @category_id)";

            cmd.Parameters.AddWithValue("@tagid", tagid);
            cmd.Parameters.AddWithValue("@tag_name", tag_name);
            cmd.Parameters.AddWithValue("@tag_display", tag_display);
            cmd.Parameters.AddWithValue("@tag_count", tag_count);
            cmd.Parameters.AddWithValue("@category_id", category_id);
            cmd.Prepare();

            cmd.ExecuteNonQuery();
        }

        public string[] ImageDataSelectSources(string hash)
        {
            using SQLiteCommand cmd = new(SQLiteConn);

            cmd.CommandText = "SELECT sources from image_data where sha_id = @hash";
            cmd.Parameters.AddWithValue("@hash", hash);

            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();
            if (rdr.Read())
            {
                string h = rdr["sources"].ToString();
                return h.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                return Array.Empty<string>();
            }
        }

        public void ImageDataUpdateSources(string hash, string sources)
        {
            using SQLiteCommand cmd = new(SQLiteConn);

            cmd.CommandText = "UPDATE image_data SET sources = @sources where sha_id=@hash";
            cmd.Parameters.AddWithValue("@sources", sources);
            cmd.Parameters.AddWithValue("@hash", hash);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region Main Server Connection

        //private readonly string ConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Chris\source\repos\Paiz\Paiz\Idaho.mdf;Integrated Security=True";

        //private SqlConnection conn;

        //public void Connect()
        //{
        //    conn = new SqlConnection(ConnectionString);

        //    conn.Open();
        //}

        //public void Disconnect()
        //{
        //    conn.Close();
        //}

        #endregion

        #region image_data Add/Update/Delete Functions

        public void InsertIntoImageData(string hash, string path, string name, string ext, DateTime date_added, DateTime date_modified, int height, int width, string md5)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO image_data (sha_id, path, name, ext, date_added, date_modified, height, width, rating, IQDB, md5) VALUES (@sha_id, @path, @name, @ext, @date_added, @date_modified, @height, @width, @rating, @IQDB, @md5)";

            cmd.Parameters.AddWithValue("@sha_id", hash);
            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@ext", ext);
            cmd.Parameters.AddWithValue("@date_added", date_added);
            cmd.Parameters.AddWithValue("@date_modified", date_modified);
            cmd.Parameters.AddWithValue("@height", height);
            cmd.Parameters.AddWithValue("@width", width);
            cmd.Parameters.AddWithValue("@rating", 0);
            cmd.Parameters.AddWithValue("@IQDB", false);
            cmd.Parameters.AddWithValue("@md5", md5);

            cmd.Prepare();
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Write("Error creating new row in image_data for file: " + path);
                Logger.Write(ex.Message);
            }
            Logger.Write("Added row in image_data for file: " + path);
        }

        public void UpdateRating(string path, int rating)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data SET rating=@rating WHERE path=@path";

            cmd.Parameters.AddWithValue("@rating", rating);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
            Logger.Write("Updated Rating to " + rating.ToString() + " for file: " + path);
        }

        public void UpdateRatingBasedOnName(string name, int rating)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data SET rating=@rating WHERE name=@name";

            cmd.Parameters.AddWithValue("@rating", rating);
            cmd.Parameters.AddWithValue("@name", name);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
            Logger.Write("Updated Rating to " + rating.ToString() + " for file: " + name);
        }

        public void UpdateTagList(string path, string tag_list)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "update image_data set tag_list=@tag_list where path = @path";

            cmd.Parameters.AddWithValue("@tag_list", tag_list);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
            //Logger.Write("Updated taglist to " + taglist.ToString() + " for file: " + path);
        }

        public List<TagItem> CopyAllTags(string CopyFrom, string CopyTo)
        {
            Logger.Write("Copying all Tags from " + CopyFrom + " to " + CopyTo);
            List<TagItem> TagItemList = new();
            string taglist = GetTagList(CopyFrom);
            UpdateTagList(CopyTo, taglist);
            string[] tags = taglist.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string tag in tags)
            {
                int tagid = int.Parse(tag);
                IncrementTagCount(tagid);
                TagItemList.Add(CreateTagItem(tagid, false));
            }
            return TagItemList;
        }

        public string AddTag(string path, string tag, string category)
        {
            int tagid = GetTagid(tag);
            string taglist = GetTagList(path);

            if (tagid != -1) //tag already added to tags table
            {
                if (!CheckDuplicateTags(tagid, path))
                {
                    string primarysibling = GetPreferredTag(tag);
                    if(!CheckDuplicateTags(GetTagid(primarysibling), path))
                    {
                        if (primarysibling != "")
                        {
                            AddNewTag(primarysibling, path);
                            taglist = CheckForParentTags(primarysibling, path);
                        }
                        else
                        {
                            AddNewTag(tag, path);
                            taglist = CheckForParentTags(tag, path);
                        }
                    }
                }
            }
            else    //tag not in tag table
            {
                InsertIntoTagData(tag, CategoryItem.CategoryByName[category].CategoryID);
                tagid = GetTagid(tag);

                if (!CheckDuplicateTags(tagid, path))
                {
                    AddNewTag(tag, path);
                    taglist = CheckForParentTags(tag, path);
                }
            }
            return taglist;
        }

        public string AddNewTag(string tag, string path)
        {
            int tagid = GetTagid(tag);

            string taglist = GetTagList(path);

            if (taglist is "" or ";")
            {
                taglist = ";" + tagid.ToString() + ";";
            }
            else
            {
                taglist += tagid.ToString() + ";";
            }

            UpdateTagList(path, taglist);
            Logger.Write("Added '" + tag + "' for file: " + path);

            IncrementTagCount(tagid);

            return taglist;
        }

        public string CheckForParentTags(string Child, string path)
        {
            List<string> ParentTags = GetParentTags(Child);
            int ParentTagID;
            string taglist = GetTagList(path);

            if (ParentTags.Count > 0)
            {
                foreach (string Tag in ParentTags)
                {
                    ParentTagID = GetTagid(Tag);
                    if (!CheckDuplicateTags(ParentTagID, path))
                    {
                        taglist = AddNewTag(Tag, path);
                        Logger.Write("Added '" + Tag + "' for file: " + path + " which is a parent to '" + Child + "'");
                    }
                    CheckForParentTags(Tag, path);
                }
            }
            return taglist;
        }

        public void ReplaceOldSiblingTag(string oldtag, string path)
        {
            string NewTag = GetPreferredTag(oldtag);
            int NewTagId = GetTagid(NewTag);
            int OldTagId = GetTagid(oldtag);
            if (NewTag != "")
            {
                if (!CheckDuplicateTags(NewTagId, path))
                {
                    AddNewTag(NewTag, path);
                    CheckForParentTags(NewTag, path);
                }
                RemoveTag(path, OldTagId);
            }
            return;
        }

        public void UpdatePathComponents(string hash, string path, string name, string ext)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data SET path=@path, name=@name, ext=@ext WHERE sha_id=@hash";

            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@ext", ext);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updated Path to " + path + " for hash: " + hash);
        }

        public void UpdateHeight(string path, int height)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data SET height=@height WHERE path=@path";

            cmd.Parameters.AddWithValue("@height", height);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updated Height to " + height.ToString() + " for file: " + path);
        }

        public void UpdateWidth(string path, int width)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data SET width=@width WHERE path=@path";

            cmd.Parameters.AddWithValue("@width", width);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updated Width to " + width.ToString() + " for file: " + path);
        }

        public void UpdateHeightAndWidth(string path, int height, int width)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data SET height=@height, width=@width WHERE path=@path";

            cmd.Parameters.AddWithValue("@height", height);
            cmd.Parameters.AddWithValue("@width", width);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updated HxW to " + height.ToString() + "x" + width.ToString() + " for file: " + path);
        }

        public void UpdateHash(string hash, string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data SET sha_id=@hash WHERE path=@path";

            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@hash", hash);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updated Hash to " + hash + " for path: " + path);
        }

        public void UpdateMD5(string md5, string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data SET md5=@md5 WHERE path=@path";

            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@md5", md5);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updated MD5 to " + md5 + " for path: " + path);
        }

        public void UpdateVideoItems(string path, bool vid, bool sound, int duration, float rate)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data SET video=@video, sound=@sound, duration=@duration, frame_rate=@frame_rate WHERE path=@path";

            cmd.Parameters.AddWithValue("@video", vid);
            cmd.Parameters.AddWithValue("@sound", sound);
            cmd.Parameters.AddWithValue("@duration", duration);
            cmd.Parameters.AddWithValue("@frame_rate", rate);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updated Video Parameters for file: " + path);
        }

        public void EditTag(string path, int oldtagid, int newtagid)
        {
            string taglist = GetTagList(path);

            taglist = taglist.Replace(";" + oldtagid + ";", ";" + newtagid + ";");

            UpdateTagList(path, taglist);

            Logger.Write("Changed TagID from '" + GetTagName(oldtagid) + "' to '" + GetTagName(newtagid) + "' for file: " + path);

            DecerementTagCount(oldtagid);
            IncrementTagCount(newtagid);
        }

        public void RemoveTag(string path, int tagid)
        {
            string taglist = GetTagList(path);

            taglist = taglist.Replace(";" + tagid + ";", ";");

            UpdateTagList(path, taglist);

            Logger.Write("Removed TagID: '" + GetTagName(tagid) + "' for file: " + path);

            DecerementTagCount(tagid);
        }

        public void DeleteAllImageTags(string path)
        {
            //select all tags first and decrement counts for each of them
            string taglist = GetTagList(path);
            string[] tags = taglist.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string tag in tags)
            {
                DecerementTagCount(int.Parse(tag));
            }

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data SET tag_list=@tag_list WHERE path=@path";

            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@tag_list", ";");

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Removed All tags from " + path);
        }

        public void DeleteFromImageData(string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "DELETE FROM image_data WHERE path=@path";

            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Removed row from image_data for " + path);
        }

        public void UpdateIqdb(string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data set IQDB=@IQDB WHERE path=@path";

            cmd.Parameters.AddWithValue("@IQDB", true);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Set IQDB flag for " + path);
        }

        public void Updatephash(string path, byte[] phash)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data set phash=@phash WHERE path=@path";

            cmd.Parameters.AddWithValue("@phash", phash);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            if (phash != null)
            {
                Logger.Write("Updating PHash to " + phash.ToString() + " for " + path);
            }
            else
            {
                Logger.Write("PHash is null for " + path);
            }
        }

        public void UpdateCorrelated(string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE image_data set correlated=@correlated WHERE path=@path";

            cmd.Parameters.AddWithValue("@correlated", true);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            //Logger.Write("Set Correlated flag for " + path);
        }

        public void RemoveBrokenTagsfromImages()
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "update image_data set tag_list = REPLACE(tag_list, ';-1;', ';') where tag_list like '%;-1;%'";

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Clearing Broken Tags from Images");
        }

        public void AddPrimarySource(string path, string source)
        {
            List<string> sources = GetSources(path);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "update image_data set sources = @sources where path = @path";

            cmd.Parameters.AddWithValue("@sources", source + " " + string.Join(' ', sources));
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public void AddNonPrimarySource(string path, string source)
        {
            List<string> sources = GetSources(path);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "update image_data set sources = @sources where path = @path";

            cmd.Parameters.AddWithValue("@sources", string.Join(' ', sources) + " " + source);
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public void UpdateSpecificSource(string path, string oldsource, string newsource)
        {
            List<string> sources = GetSources(path);
            sources[sources.IndexOf(oldsource)] = newsource;

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "update image_data set sources = @sources where path = @path";

            cmd.Parameters.AddWithValue("@sources", string.Join(' ', sources));
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public void ChangePrimarySource(string path, string newprimary)
        {
            List<string> sources = GetSources(path);

            sources.Remove(newprimary);
            sources.Insert(0, newprimary);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "update image_data set sources = @sources where path = @path";

            cmd.Parameters.AddWithValue("@sources", string.Join(' ', sources));
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public void RemoveSource(string path, string source)
        {
            List<string> sources = GetSources(path);

            sources.Remove(source);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "update image_data set sources = @sources where path = @path";

            cmd.Parameters.AddWithValue("@sources", string.Join(' ', sources));
            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region image_data Select Functions
        public string GetHash(string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT sha_id FROM image_data WHERE path=@path";

            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                //Logger.Write("Getting Hash: " + rdr["sha_id"].ToString() + " for file: " + path);
                return rdr["sha_id"].ToString();
            }
            else
            {
                Logger.Write("No Hash found in DB for file: " + path);
                return "";
            }

        }

        public string GetPath(string hash)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT path FROM image_data WHERE sha_id=@hash";

            cmd.Parameters.AddWithValue("@hash", hash);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                //Logger.Write("Getting path: " + rdr["path"].ToString() + " for file hash: " + hash);
                return rdr["path"].ToString();
            }
            else
            {
                Logger.Write("No path found in DB for file hash: " + hash);
                return "";
            }
        }

        public string GetFileName(string hash)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT name FROM image_data WHERE sha_id=@hash";

            cmd.Parameters.AddWithValue("@hash", hash);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                //Logger.Write("Getting file name: " + rdr["name"].ToString() + " for file hash: " + hash);
                return rdr["name"].ToString();
            }
            else
            {
                Logger.Write("No name found in DB for file hash: " + hash);
                return "";
            }
        }

        public bool CheckDuplicateHash(string hash)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT COUNT(*) FROM image_data WHERE sha_id=@hash";

            cmd.Parameters.AddWithValue("@hash", hash);

            cmd.Prepare();
            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
            {
                //Logger.Write("Hash " + hash + " already in Database");
                return true;
            }
            else
            {
                Logger.Write("Hash " + hash + " not currently in database");
                return false;
            }
        }
        public string GetMD5(string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT md5 FROM image_data WHERE path=@path";

            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                //Logger.Write("Getting Hash: " + rdr["sha_id"].ToString() + " for file: " + path);
                return rdr["md5"].ToString();
            }
            else
            {
                //Logger.Write("No MD5 found in DB for file: " + path);
                return "";
            }
        }

        public bool CheckDuplicateTags(int tagid, string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT COUNT(*) FROM image_data WHERE path=@path AND tag_list like @tagid";

            cmd.Parameters.AddWithValue("@path", path);
            cmd.Parameters.AddWithValue("@tagid", "%;" + tagid.ToString() + ";%");

            cmd.Prepare();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public List<string> GetAllPaths(int orderby, bool asc)
        {
            string dir = asc ? "ASC" : "DESC";
            string order = orderby switch
            {
                0 => "sha_id",
                1 => "path",
                2 => "name",
                3 => "date_added",
                4 => "date_modified",
                5 => "height",
                6 => "width",
                7 => "rating",
                8 => "ID",
                _ => "date_modified",
            };

            List<string> Output = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT path FROM image_data where sha_id not in (select deleted_images.hash from deleted_images) ORDER BY " + order + " " + dir + ";";

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Output.Add(rdr["path"].ToString());
            }

            //Logger.Write("Getting all paths in image_data");
            return Output;
        }

        public ImageItem GetImage(string path)
        {
            ImageItem Output = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM image_data where path=@path";

            cmd.Parameters.AddWithValue("@path", path);
            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                Output = new ImageItem()
                {
                    hash = rdr["sha_id"].ToString(),
                    path = rdr["path"].ToString(),
                    date_added = rdr.GetDateTime(5),
                    date_modfied = rdr.GetDateTime(6),
                    height = rdr.GetInt32(7),
                    width = rdr.GetInt32(8),
                    rating = rdr.GetInt32(9),
                    video = rdr.GetBoolean(15),
                    sound = rdr.GetBoolean(16),
                    tag_list = rdr["tag_list"].ToString(),
                    primary_source = rdr["sources"].ToString() == "" ? "" : rdr["sources"].ToString().Split(" ", StringSplitOptions.RemoveEmptyEntries)[0]
                };
            }
            return Output;
        }

        public List<ImageItem> GetAllImageItems(int orderby, bool asc)
        {
            string dir = asc ? "ASC" : "DESC";
            string order = orderby switch
            {
                0 => "sha_id",
                1 => "path",
                2 => "name",
                3 => "date_added",
                4 => "date_modified",
                5 => "height",
                6 => "width",
                7 => "rating",
                8 => "ID",
                _ => "date_modified",
            };

            List<ImageItem> Output = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            {
                cmd.CommandText = "SELECT * FROM image_data ORDER BY " + order + " " + dir + ";";
                using SQLiteDataReader rdr = cmd.ExecuteReader();
                {
                    while (rdr.Read())
                    {
                        Output.Add(new ImageItem()
                        {
                            hash = rdr["sha_id"].ToString(),
                            path = rdr["path"].ToString(),
                            date_added = rdr.GetDateTime(5),
                            date_modfied = rdr.GetDateTime(6),
                            height = rdr.GetInt32(7),
                            width = rdr.GetInt32(8),
                            rating = rdr.GetInt32(9),
                            video = rdr.GetBoolean(15),
                            sound = rdr.GetBoolean(16),
                            tag_list = rdr["tag_list"].ToString(),
                            primary_source = rdr["sources"].ToString() == "" ? "" : rdr["sources"].ToString().Split(" ", StringSplitOptions.RemoveEmptyEntries)[0],
                            md5 = rdr["md5"].ToString()
                        });
                    }
                }
            }

            return Output;
        }

        public int GetDuration(string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT duration FROM image_data where path=@path";

            cmd.Parameters.AddWithValue("@path", path);
            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            return rdr.Read() ? int.Parse(rdr["duration"].ToString()) : 0;
        }

        public string GetTagList(string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "select tag_list from image_data where path = @path";

            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                //Logger.Write("Getting tag_list: " + rdr["tag_list"].ToString() + " for file: " + path);
                return rdr["tag_list"].ToString();
            }
            else
            {
                Logger.Write("tag_list empty for file: " + path);
                return "";
            }
        }

        public List<ImageItem> GetNonIQDBImages()
        {
            List<ImageItem> Output = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM image_data where IQDB=@IQDB ORDER BY date_modified desc;";

            cmd.Parameters.AddWithValue("@IQDB", false);
            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Output.Add(new ImageItem()
                {
                    hash = rdr["sha_id"].ToString(),
                    path = rdr["path"].ToString(),
                    height = rdr.GetInt32(7),
                    width = rdr.GetInt32(8),
                    rating = rdr.GetInt32(9),
                    md5 = rdr["md5"].ToString()
                });
            }

            Logger.Write("Getting all Imageitems with IQDB Flag not raised");
            return Output;
        }

        public List<ImageItem> GetImagesWithSpecificTag(int tagid)
        {
            List<ImageItem> Output = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM image_data where tag_list like @tagid order by id asc";

            cmd.Parameters.AddWithValue("@tagid", "%;" + tagid + ";%");
            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Output.Add(new ImageItem()
                {
                    hash = rdr["sha_id"].ToString(),
                    path = rdr["path"].ToString(),
                    date_added = rdr.GetDateTime(5),
                    date_modfied = rdr.GetDateTime(6),
                    height = rdr.GetInt32(7),
                    width = rdr.GetInt32(8),
                    rating = rdr.GetInt32(9),
                    video = rdr.GetBoolean(15),
                    sound = rdr.GetBoolean(16),
                    tag_list = rdr["tag_list"].ToString(),
                    primary_source = rdr["sources"].ToString() == "" ? "" : rdr["sources"].ToString().Split(" ", StringSplitOptions.RemoveEmptyEntries)[0],
                    md5 = rdr["md5"].ToString()
                });
            }

            Logger.Write("Getting Image that are tagged with '" + GetTagName(tagid) + "' - found " + Output.Count.ToString());
            return Output;
        }

        public List<TagItem> GetFileTags(string tag_list)
        {
            List<TagItem> Output = new();
            string[] tags = tag_list.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string tag in tags)
            {
                Output.Add(CreateTagItem(int.Parse(tag), true));
            }

            return Output;
        }

        public int CalculateTagCount(int tagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT COUNT(*) from image_data where tag_list like @tagid";

            cmd.Parameters.AddWithValue("@tagid", "%;" + tagid + ";%");

            cmd.Prepare();
            return (int)cmd.ExecuteScalar();
        }

        public List<string> SearchByExactTag(string Tag)
        {
            int tagid = GetTagid(Tag);

            List<string> Output = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM image_data where tag_list like @param1";

            cmd.Parameters.AddWithValue("@param1", "%;" + tagid + ";%");
            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Output.Add(rdr["path"].ToString());
            }

            Logger.Write("Getting Image paths that are tagged with '" + Tag + "' - found " + Output.Count.ToString());
            return Output;
        }

        public List<ImageItem> ComplexSearch(string searchvalue)
        {
            string pattern = @"(&{2}|\|{2})";
            string[] substrings = Regex.Split(searchvalue, pattern).Where(i => !string.IsNullOrWhiteSpace(i)).ToArray();
            string reformedsearchtext = "";

            for (int str = 0; str <= substrings.Length - 1; str++)
            {
                substrings[str] = substrings[str].Trim();

                int AmountOpenParens = substrings[str].Length - substrings[str].Replace("(", "").Length;
                int AmountCloseParens = substrings[str].Length - substrings[str].Replace(")", "").Length;

                if (AmountOpenParens == AmountCloseParens)
                {
                    if (substrings[str].Contains("$"))
                    {
                        if(substrings[str].Contains("$source"))
                        {
                            substrings[str] = substrings[str].Replace("$source", "source like '%");
                            substrings[str] += "%' ";
                        }
                        else if(substrings[str].Contains("$name"))
                        {
                            substrings[str] = substrings[str].Replace("$name", "name like '%");
                            substrings[str] += "%' ";
                        }
                        else if (substrings[str] == "$video")
                        {
                            substrings[str] = "video=1 ";
                        }
                        else if (substrings[str] == "$sound")
                        {
                            substrings[str] = "sound=1 ";
                        }
                        else if (substrings[str] == "$!tagged")
                        {
                            substrings[str] = "(tag_list in ('', ';') or tag_list is null) ";
                        }
                        else if (substrings[str].Contains("$resolution"))
                        {
                            substrings[str] = substrings[str].Replace("$resolution", "video=1 and height");
                        }
                        else
                        {
                            substrings[str] = substrings[str].Replace("$", "");
                        } 
                    }
                    else if (substrings[str].StartsWith("!"))
                    {
                        substrings[str] = substrings[str].Remove(0, 1);
                        substrings[str] = "tag_list not like '%;" + GettagidCaseInsensitive(SelectPreferredTagIfExists(substrings[str])).ToString() + ";%'";
                    }
                    else if (substrings[str].Contains("&&"))
                    {
                        substrings[str] = substrings[str].Replace("&&", " and ");
                    }
                    else if (substrings[str].Contains("||"))
                    {
                        substrings[str] = substrings[str].Replace("||", " or ");
                    }
                    else
                    {
                        substrings[str] = "tag_list like '%;" + GettagidCaseInsensitive(SelectPreferredTagIfExists(substrings[str])).ToString() + ";%'";
                    }
                }
                else if (AmountOpenParens > AmountCloseParens)
                {
                    substrings[str] = substrings[str].TrimStart(new char[] { '(' });
                    if (substrings[str].Contains("$"))
                    {
                        if (substrings[str].Contains("$source"))
                        {
                            substrings[str] = substrings[str].Replace("$source", "source like '%");
                            substrings[str] += "%' ";
                        }
                        else if (substrings[str].Contains("$name"))
                        {
                            substrings[str] = substrings[str].Replace("$name", "name like '%");
                            substrings[str] += "%' ";
                        }
                        else if (substrings[str] == "$video")
                        {
                            substrings[str] = "video=1 ";
                        }
                        else if (substrings[str] == "$sound")
                        {
                            substrings[str] = "sound=1 ";
                        }
                        else if (substrings[str] == "$!tagged")
                        {
                            substrings[str] = "(tag_list in ('', ';') or tag_list is null) ";
                        }
                        else if (substrings[str].Contains("$resolution"))
                        {
                            substrings[str] = substrings[str].Replace("$resolution", "video=1 and height");
                        }
                        else
                        {
                            substrings[str] = substrings[str].Replace("$", "");
                        }
                    }
                    else if (substrings[str].StartsWith("!"))
                    {
                        substrings[str] = substrings[str].Remove(0, 1);
                        substrings[str] = "tag_list not like '%;" + GettagidCaseInsensitive(SelectPreferredTagIfExists(substrings[str])).ToString() + ";%'";
                    }
                    else if (substrings[str].Contains("&&"))
                    {
                        substrings[str] = substrings[str].Replace("&&", " and ");
                    }
                    else if (substrings[str].Contains("||"))
                    {
                        substrings[str] = substrings[str].Replace("||", " or ");
                    }
                    else
                    {
                        substrings[str] = "tag_list like '%;" + GettagidCaseInsensitive(SelectPreferredTagIfExists(substrings[str])).ToString() + ";%'";
                    }
                    substrings[str] = "(" + substrings[str];
                }
                else if (AmountCloseParens > AmountOpenParens)
                {
                    substrings[str] = substrings[str].TrimEnd(new char[] { ')' });
                    if (substrings[str].Contains("$"))
                    {
                        if (substrings[str].Contains("$source"))
                        {
                            substrings[str] = substrings[str].Replace("$source", "source like '%");
                            substrings[str] += "%' ";
                        }
                        else if (substrings[str].Contains("$name"))
                        {
                            substrings[str] = substrings[str].Replace("$name", "name like '%");
                            substrings[str] += "%' ";
                        }
                        else if (substrings[str] == "$video")
                        {
                            substrings[str] = "video=1 ";
                        }
                        else if (substrings[str] == "$sound")
                        {
                            substrings[str] = "sound=1 ";
                        }
                        else if (substrings[str] == "$!tagged")
                        {
                            substrings[str] = "(tag_list in ('', ';') or tag_list is null) ";
                        }
                        else if (substrings[str].Contains("$resolution"))
                        {
                            substrings[str] = substrings[str].Replace("$resolution", "video=1 and height");
                        }
                        else
                        {
                            substrings[str] = substrings[str].Replace("$", "");
                        }
                    }
                    else if (substrings[str].StartsWith("!"))
                    {
                        substrings[str] = substrings[str].Remove(0, 1);
                        substrings[str] = "tag_list not like '%;" + GettagidCaseInsensitive(SelectPreferredTagIfExists(substrings[str])).ToString() + ";%'";
                    }
                    else if (substrings[str].Contains("&&"))
                    {
                        substrings[str] = substrings[str].Replace("&&", " and ");
                    }
                    else if (substrings[str].Contains("|"))
                    {
                        substrings[str] = substrings[str].Replace("|", " or ");
                    }
                    else
                    {
                        substrings[str] = "tag_list like '%;" + GettagidCaseInsensitive(SelectPreferredTagIfExists(substrings[str])).ToString() + ";%'";
                    }
                    substrings[str] = substrings[str] + ")";
                }
                reformedsearchtext += substrings[str];
            }

            List<ImageItem> Output = new();

            string query = "SELECT * FROM image_data where " + reformedsearchtext + " and image_data.sha_id not in (select deleted_images.hash from deleted_images) order by ID asc";

            try
            {
                using SQLiteCommand cmd = new(SQLiteConn);
                cmd.CommandText = query;

                cmd.Prepare();
                using SQLiteDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Output.Add(new ImageItem()
                    {
                        hash = rdr["sha_id"].ToString(),
                        path = rdr["path"].ToString(),
                        date_added = rdr.GetDateTime(5),
                        date_modfied = rdr.GetDateTime(6),
                        height = rdr.GetInt32(7),
                        width = rdr.GetInt32(8),
                        rating = rdr.GetInt32(9),
                        video = rdr.GetBoolean(15),
                        sound = rdr.GetBoolean(16),
                        tag_list = rdr["tag_list"].ToString(),
                        primary_source = rdr["sources"].ToString() == "" ? "" : rdr["sources"].ToString().Split(" ", StringSplitOptions.RemoveEmptyEntries)[0],
                        md5 = rdr["md5"].ToString()
                    });
                }

                Logger.Write("Getting ImageItems based on search query: " + searchvalue);
                Logger.Write("Search query converted into sql query: " + query);
                Logger.Write("Search Query returned " + Output.Count.ToString() + " Files");
            }
            catch(Exception ex)
            {
                Logger.Write("Error Getting ImageItems based on search query: " + searchvalue);
                Logger.Write("Search Query Error: " + ex.Message);
                Logger.Write("Search query converted into sql query: " + query);
                Logger.Write("Search Query returned " + Output.Count.ToString() + " Files");
            }

            return Output;
        }

        public int GetRating(string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT rating FROM image_data WHERE path=@path";

            cmd.Parameters.AddWithValue("@path", path);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                if (rdr["rating"].ToString() == "")
                {
                    Logger.Write("There is no rating for file: " + path);
                    return -1;
                }
                else
                {
                    //Logger.Write("Rating for file: " + path + " is " + rdr["rating"].ToString());
                    return rdr.GetInt32(0);
                }
            }
            else
            {
                Logger.Write("There is no rating for file: " + path);
                return -1;
            }
        }

        public List<string> GetImagesWithoutphash()
        {
            List<string> Output = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM image_data where phash is null";

            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Output.Add(rdr["path"].ToString());
            }

            Logger.Write("Getting all Files where the Phash has not been calculated yet");
            return Output;
        }

        public List<ImageItem> GetUncorrelatedImages()
        {
            List<ImageItem> Output = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM image_data where (correlated is null or correlated=@correlated) and ext in ('.jpg', '.jpeg', '.gif', '.png')";

            cmd.Parameters.AddWithValue("@correlated", false);
            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Output.Add(new ImageItem() { path = rdr["path"].ToString(), hash = rdr["sha_id"].ToString(), phash = rdr["phash"] as byte[] });
            }

            Logger.Write("Getting all Image Files that have NOT been correlated to check for Duplicates yet.");
            return Output;
        }

        public List<ImageItem> GetCorrelatedImages()
        {
            List<ImageItem> Output = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM image_data where correlated=@correlated and ext in ('.jpg', '.jpeg', '.gif', '.png')";

            cmd.Parameters.AddWithValue("@correlated", true);
            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Output.Add(new ImageItem() { path = rdr["path"].ToString(), hash = rdr["sha_id"].ToString(), phash = rdr["phash"] as byte[] });
            }

            Logger.Write("Getting all Image Files that have been correlated.");
            return Output;
        }

        public List<string> GetSources(string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT sources FROM image_data where path=@path";

            cmd.Parameters.AddWithValue("@path", path);
            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            return rdr.Read()
                ? rdr["sources"].ToString() == "" ? new List<string>() : new List<string>(rdr["sources"].ToString().Split(" ", StringSplitOptions.RemoveEmptyEntries))
                : new List<string>();
        }

        public string GetPrimarySource(string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT sources FROM image_data where path=@path";

            cmd.Parameters.AddWithValue("@path", path);
            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            return rdr.Read()
                ? rdr["sources"].ToString() == "" ? "" : rdr["sources"].ToString().Split(" ", StringSplitOptions.RemoveEmptyEntries)[0]
                : "";
        }

        public string GetImgurSource(string path)
        {
            string GetImgurSource = "";
            List<string> sources = GetSources(path);

            foreach(string source in sources)
            {
                if (source.Contains("imgur"))
                {
                    GetImgurSource = source;
                }
            }

            return GetImgurSource;
        }

        public bool CheckIfSourceExists(string path, string source)
        {
            List<string> sources = GetSources(path);

            foreach(string s in sources)
            {
                if (s == source)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region tag_data Add/Update/Delete Functions

        public void InsertIntoTagData(string tagname, int categoryid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO tag_data (tag_name, category_id, tag_display, tag_count) VALUES (@tag_name, @category_id, @tag_display, @tag_count);";

            cmd.Parameters.AddWithValue("@tag_name", tagname.Trim());
            cmd.Parameters.AddWithValue("@category_id", categoryid);
            cmd.Parameters.AddWithValue("@tag_display", tagname.Trim());
            cmd.Parameters.AddWithValue("@tag_count", 0);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Inserting Tag '" + tagname + "' into tag_data table");
        }

        public void InsertIntoTagData(string tagname, string tagdisplay, int categoryid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO tag_data (tag_name, category_id, tag_display, tag_count) VALUES (@tag_name, @category_id, @tag_display, @tag_count);";

            cmd.Parameters.AddWithValue("@tag_name", tagname.Trim());
            cmd.Parameters.AddWithValue("@category_id", categoryid);
            cmd.Parameters.AddWithValue("@tag_display", tagdisplay.Trim());
            cmd.Parameters.AddWithValue("@tag_count", 0);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Inserting Tag '" + tagname + "' into tag_data table");
        }

        public void UpdateTagCategory(int categoryid, int tagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE tag_data SET category_id=@category_id WHERE tagid=@tagid";

            cmd.Parameters.AddWithValue("@category_id", categoryid);
            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updating Tag '" + GetTagName(tagid) + "' Category to " + CategoryItem.CategoryByID[categoryid].Category);
        }

        public void UpdateTagDisplay(string tagdisplay, int tagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE tag_data SET tag_display=@tag_display WHERE tagid=@tagid";

            cmd.Parameters.AddWithValue("@tag_display", tagdisplay.Trim());
            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updating Tag '" + GetTagName(tagid) + "' Display to " + tagdisplay);
        }

        public void UpdateTag(string tag, int tagid)
        {
            string oldtag = GetTagName(tagid);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE tag_data SET tag_name=@tag_name, tag_display=@tag_display WHERE tagid=@tagid";

            cmd.Parameters.AddWithValue("@tag_name", tag.Trim());
            cmd.Parameters.AddWithValue("@tagid", tagid);
            cmd.Parameters.AddWithValue("@tag_display", tag.Trim());

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Editing Tag '" + oldtag + "' to '" + GetTagName(tagid) + "'");
        }

        public void UpdateTag(string tag, string tagdisplay, int tagid)
        {
            string oldtag = GetTagName(tagid);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE tag_data SET tag_name=@tag_name, tag_display=@tag_display WHERE tagid=@tagid";

            cmd.Parameters.AddWithValue("@tag_name", tag.Trim());
            cmd.Parameters.AddWithValue("@tagid", tagid);
            cmd.Parameters.AddWithValue("@tag_display", tagdisplay.Trim());

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Editing Tag '" + oldtag + "' to '" + GetTagName(tagid) + "'");
        }

        public void IncrementTagCount(int tagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE tag_data SET tag_count=tag_count + 1 WHERE tagid=tagid";

            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public void DecerementTagCount(int tagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE tag_data SET tag_count=tag_count - 1 WHERE tagid=@tagid";

            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public void UpdateTagCount(int tagid, int CalculatedTagCount)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE tag_data SET tag_count=@tag_count WHERE tagid=@tagid";

            cmd.Parameters.AddWithValue("@tag_count", CalculatedTagCount);
            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updating Tag Count for Tag '" + GetTagName(tagid) + "' to " + CalculatedTagCount);
        }

        public void DeleteTagData(int tagid)
        {
            Logger.Write("Deleting Tag '" + GetTagName(tagid) +"' from database");

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "DELETE FROM tag_data WHERE tagid=@tagid";

            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region tag_data Select Functions

        public int GetTagid(string tagname)
        {
            if (tagname == null)
            {
                return -1;
            }

            tagname = tagname.Trim();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT tagid FROM tag_data WHERE tag_name=@tag_name";

            cmd.Parameters.AddWithValue("@tag_name", tagname);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                return rdr.GetInt32(0);
            }
            else
            {
                Logger.Write("No tag id found for tag '" + tagname + "' (case sensitive)");
                return -1;
            }
        }

        public int GettagidCaseInsensitive(string tagname)
        {
            if (tagname == null)
            {
                Logger.Write("tag is null");
                return -1;
            }

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT tagid FROM tag_data WHERE tag_name=@tag_name COLLATE NOCASE";

            cmd.Parameters.AddWithValue("@tag_name", tagname);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                return rdr.GetInt32(0);
            }
            else
            {
                Logger.Write("No tag id found for tag '" + tagname + "' (case insensitive)");
                return -1;
            }
        }

        public string GetTagName(int tagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT tag_name FROM tag_data WHERE tagid=@tagid;";

            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                return rdr["tag_name"].ToString();
            }
            else
            {
                Logger.Write("No tag found for tag id '" + tagid + "'");
                return "";
            }
        }

        public int GetTagCount(int tagid)
        {
            string tag = GetTagName(tagid);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT tag_count FROM tag_data WHERE tagid=@tagid";

            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                //Logger.Write("Tag '" + tag + "' has a count of " + rdr["tag_count"].ToString());
                return rdr.GetInt32(0);
            }
            else
            {
                Logger.Write("No tag count found for tag id '" + tagid + "'");
                return -1;
            }
        }

        public List<TagItem> TagSuggestions(string searchtext, int limit)
        {
            string TitleID = CategoryItem.CategoryByName["Title"].CategoryID.ToString();

            List<int> TagOutput = new();
            List<TagItem> Tags = new();
            int ExactID = -1;

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT tagid FROM tag_data WHERE tag_name=@tag_name AND category_id != " + TitleID;

            cmd.Parameters.AddWithValue("@tag_name", searchtext);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                ExactID = rdr.GetInt32(0);
                TagOutput.Add(ExactID);
            }

            if (ExactID != -1)
            {
                limit -= 1;
            }

            using SQLiteCommand cmd2 = new(SQLiteConn);
            cmd2.CommandText = "SELECT tagid FROM tag_data WHERE tag_name LIKE @tag_name AND category_id != " + TitleID + " ORDER BY tag_count DESC LIMIT " + (ExactID != -1 ? limit : (limit + 1)).ToString();

            cmd2.Parameters.AddWithValue("@tag_name", "%" + searchtext + "%");

            cmd2.Prepare();
            using SQLiteDataReader rdr2 = cmd2.ExecuteReader();

            while (rdr2.Read())
            {
                if (rdr2.GetInt32(0) != ExactID)
                {
                    TagOutput.Add(rdr2.GetInt32(0));
                }
            }

            foreach (int tagids in TagOutput)
            {
                Tags.Add(CreateTagItem(tagids, true));
            }

            return Tags;
        }

        public TagItem CreateTagItem(int tagid, bool NeedDisplay)
        {
            TagItem tag = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM tag_data WHERE tagid = @tagid";

            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                tag = new TagItem() { Name = rdr["tag_name"].ToString(), ID = rdr.GetInt32(0), CategoryID = rdr.GetInt32(4), Count = rdr.GetInt32(3) };
            }

            tag.Category = CategoryItem.CategoryByID[tag.CategoryID].Category;
            tag.Color = CategoryItem.CategoryByID[tag.CategoryID].Color;
            if (NeedDisplay)
            {
                string primarysibling = GetPreferredTag(tag.Name);
                tag.Display = tag.Name + " [" + tag.Count.ToString() + "]";
                if (primarysibling != "")
                {
                    tag.Display += " (Primary: " + primarysibling + ")";
                }
                tag.Display += DisplayParentTags(tag.Name, "", 1);
            }
            else
            {
                tag.Display = "";
            }

            return tag;
        }

        public List<TagItem> TagItemSearch(string searchtext)
        {
            List<TagItem> TagOutput = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "select tag_name, category_id from tag_data where tag_name like @search order by tag_name;";

            cmd.Parameters.AddWithValue("@search", "%" + searchtext + "%");

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                TagOutput.Add(new TagItem() { Name = rdr["tag_name"].ToString(), Color = CategoryItem.CategoryByID[rdr.GetInt32(1)].Color });
            }

            Logger.Write("Getting tags containing text: " + searchtext);
            return TagOutput;
        }

        public List<TagItem> GetAllTags()
        {
            List<TagItem> TagOutput = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "select tag_name, category_id from tag_data order by tag_name";

            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                TagOutput.Add(new TagItem() { Name = rdr["tag_name"].ToString(), Color = CategoryItem.CategoryByID[rdr.GetInt32(1)].Color });
            }

            Logger.Write("Getting all Tags");
            return TagOutput;
        }

        public List<int> GetAllTagIDs()
        {
            List<int> tagids = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM tag_data ORDER BY tag_name";

            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                tagids.Add(rdr.GetInt32(0));
            }

            Logger.Write("Getting all Tags Ids");
            return tagids;
        }

        public List<TagItem> GetAllTagIdsInCategory(string Category)
        {
            List<TagItem> Tags = new();
            int CategoryID = CategoryItem.CategoryByName[Category].CategoryID;

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "select tag_name from tag_data where tag_data.category_id = @category_id order by tag_name";
            cmd.Parameters.AddWithValue("@catgeory_id", CategoryID);

            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                Tags.Add(new TagItem() { Name = rdr["tag_name"].ToString(), Color = CategoryItem.CategoryByID[CategoryID].Color });
            }

            Logger.Write("Getting all Tags in Category: " + Category);
            return Tags;
        }

        public int GetTagCateory(int tagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT category_id FROM tag_data where tagid=@tagid";

            cmd.Prepare();
            cmd.Parameters.AddWithValue("@tagid", tagid);

            using SQLiteDataReader rdr = cmd.ExecuteReader();

            return rdr.Read() ? rdr.GetInt32(0) : -1;
        }

        #endregion

        #region settings Add/Update Functions

        public void UpdateLastFileOpened(string path)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE settings SET last_file_opened=@last_file_opened WHERE id=1";

            cmd.Parameters.AddWithValue("@last_file_opened", path);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public void UpdateClosingJob(int job)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE settings SET closing_job=@closing_job WHERE id=1";

            cmd.Parameters.AddWithValue("@closing_job", job);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public void UpdateVolume(float volume)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE settings SET volume=@volume WHERE id=1";

            cmd.Parameters.AddWithValue("@volume", volume);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region settings Select Functions

        public string GetLastFileOpened()
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT last_file_opened FROM settings WHERE id=1";

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            return rdr.Read() ? rdr["last_file_opened"].ToString() : "";
        }

        public int GetClosingJob()
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT closing_job FROM settings WHERE id=1";

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            return rdr.Read() ? rdr.GetInt32(0) : -1;
        }

        public double GetVolume()
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT volume FROM settings WHERE id=1";

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            return rdr.Read() ? rdr.GetFloat(0) : -1;
        }

        #endregion

        #region sibling_data Add/Update/Delete Functions

        public void AddRowToSiblingTable(string aliastag, string preferredtag)
        {
            int aliasid = GetTagid(aliastag.Trim());
            int preferredid = GetTagid(preferredtag.Trim());

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO sibling_data (old_id, new_id, retro) VALUES (@aslias, @preferred, @retro)";

            cmd.Parameters.AddWithValue("@alias", aliasid);
            cmd.Parameters.AddWithValue("@preferred", preferredid);
            cmd.Parameters.AddWithValue("@retro", false);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("New Sibling Relationship: alias - " + aliastag + " & preferred - " + preferredtag);
        }

        public void AddSiblingTag(string aliastag, string preferredtag)
        {
            int aliasid = GetTagid(aliastag.Trim());
            int preferredid = GetTagid(preferredtag.Trim());

            if (aliasid == preferredid)
            {
                return;
            }

            if (GetPreferredTag(aliastag) == preferredtag)
            {
                return;
            }

            if (!CheckifSiblingRelationshipExists(aliasid, preferredid))
            {
                if (aliasid == -1) //if old sibling tag is not in tag_data table
                {
                    InsertIntoTagData(aliastag, aliastag + " Displays as: " + preferredtag, GetTagCateory(preferredid));
                }
                else if (preferredid == -1) //if primary sibling is not in tag_data table
                {
                    InsertIntoTagData(preferredtag, GetTagCateory(aliasid));
                }
                AddRowToSiblingTable(aliastag, preferredtag);
            }
        }

        public void DeleteSiblingRelationship(string aliastag, string preferredtag)
        {
            int aliasid = GetTagid(preferredtag.Trim());
            int preferredid = GetTagid(aliastag.Trim());

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "DELETE FROM sibling_data WHERE new_id=@preferred AND old_id=@alias";

            cmd.Parameters.AddWithValue("@aslias", aliasid);
            cmd.Parameters.AddWithValue("@preferred", preferredid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Removed Sibling Relationship: alias - " + aliastag + " & preferred - " + preferredtag);
        }

        public void DeleteSiblingRelationship(int aliasid, int preferredid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "DELETE FROM sibling_data WHERE new_id=@preferred AND old_id=@alias";

            cmd.Parameters.AddWithValue("@preferred", preferredid);
            cmd.Parameters.AddWithValue("@alias", aliasid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Removed Sibling Relationship: alias - " + GetTagName(aliasid) + " & preferred - " + GetTagName(preferredid));
        }

        public void SwapSiblingRelationship(string aliastag, string preferredtag)
        {
            DeleteSiblingRelationship(aliastag, preferredtag);
            AddSiblingTag(preferredtag, aliastag);
        }

        public void UpdatePreferredTag(int aliasid, int oldpreferredid, int newpreferredid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE sibling_data SET new_id=@newpreferred WHERE new_id=@oldpreferred AND old_id=@alias";

            cmd.Parameters.AddWithValue("@newpreferred", SqlDbType.Int).Value = newpreferredid;
            cmd.Parameters.AddWithValue("@oldpreferred", SqlDbType.Int).Value = oldpreferredid;
            cmd.Parameters.AddWithValue("@alias", aliasid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updating preffered tag in Sibling Relationship: alias - " + GetTagName(aliasid) + " & preivous preferred - " + GetTagName(oldpreferredid) + " to new preferred " + GetTagName(newpreferredid));
        }

        public void UpdateAliasTag(int oldaliasid, int newaliasid, int preferredid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE sibling_data SET old_id=@newalias WHERE new_id=@preferred AND old_id=@oldalias";

            cmd.Parameters.AddWithValue("@newalaias", SqlDbType.Int).Value = newaliasid;
            cmd.Parameters.AddWithValue("@preferred", preferredid);
            cmd.Parameters.AddWithValue("@oldalias", SqlDbType.Int).Value = oldaliasid;

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updating Alias tag in Sibling Relationship: previous alias - " + GetTagName(oldaliasid) + " to new alias " + GetTagName(newaliasid) + " & preferred - " + GetTagName(preferredid));
        }

        public void UpdateSiblingRetro(int aliasid, int preferredid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE sibling_data SET retro=@retro WHERE old_id=@alias AND new_id=@preferred";

            cmd.Parameters.AddWithValue("@retro", true);
            cmd.Parameters.AddWithValue("@aslias", aliasid);
            cmd.Parameters.AddWithValue("@preferred", preferredid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updating Retro Flag for Sibling Relationship: alias - " + GetTagName(aliasid) + " & preferred - " + GetTagName(preferredid));
        }

        #endregion

        #region sibling_data Select Functions

        public string GetPreferredTag(string aliastag)
        {
            int preferredid;
            int aliasid = GetTagid(aliastag);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT new_id FROM sibling_data WHERE old_id=@alias;";

            cmd.Parameters.AddWithValue("@alias", aliasid);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                preferredid = rdr.GetInt32(0);
            }
            else
            {
                return "";
            }

            return GetTagName(preferredid);
        }

        public string SelectPreferredTagIfExists(string aliastag)
        {
            int preferredid;
            int aliasid = GettagidCaseInsensitive(aliastag);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT new_id FROM sibling_data WHERE old_id=@alias";

            cmd.Parameters.AddWithValue("@palias", aliasid);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                preferredid = rdr.GetInt32(0);
            }
            else
            {
                return aliastag;
            }

            return GetTagName(preferredid);
        }

        public List<RelationshipItem> GetAllSiblingTagRelationships()
        {
            List<RelationshipItem> SiblingTags = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM sibling_data;";

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                SiblingTags.Add(new RelationshipItem() { ParentPreferredId = rdr.GetInt32(1), ChildAliasId = rdr.GetInt32(0) });
            }

            foreach (RelationshipItem g in SiblingTags)
            {
                g.ChildAliasTag = GetTagName(g.ChildAliasId);
                g.ParentPreferredTag = GetTagName(g.ParentPreferredId);
            }
            List<RelationshipItem> tags = SiblingTags.OrderBy(x => x.ChildAliasTag).ToList();
            Logger.Write("Getting all Sibling Tag Relationships");
            return tags;
        }

        public bool CheckifSiblingRelationshipExists(int aliasid, int preferredid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT COUNT(*) FROM sibling_data WHERE new_id=@preferred AND old_id=@alias";

            cmd.Parameters.AddWithValue("@preferred", preferredid);
            cmd.Parameters.AddWithValue("@alias", aliasid);

            cmd.Prepare();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public List<RelationshipItem> GetSiblingTagRelationshipsContaining(string tag)
        {
            List<RelationshipItem> SiblingTags = new();
            int tagid = GetTagid(tag);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM sibling_data where old_id=@alias or new_id=@preferred";

            cmd.Prepare();
            cmd.Parameters.AddWithValue("@alias", tagid);
            cmd.Parameters.AddWithValue("@preferred", tagid);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                SiblingTags.Add(new RelationshipItem() { ParentPreferredId = rdr.GetInt32(1), ChildAliasId = rdr.GetInt32(0) });
            }

            foreach (RelationshipItem g in SiblingTags)
            {
                g.ChildAliasTag = GetTagName(g.ChildAliasId);
                g.ParentPreferredTag = GetTagName(g.ParentPreferredId);
            }

            Logger.Write("Getting Sibling Tag Relationships containing '" + tag + "'");
            return SiblingTags.OrderBy(x => x.ChildAliasTag).ToList();
        }

        public List<RelationshipItem> GetRetroSiblingTagRelationships()
        {
            List<RelationshipItem> SiblingRelationships = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM sibling_data WHERE retro=0 OR retro IS NULL;";

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                SiblingRelationships.Add(new RelationshipItem() { ParentPreferredId = rdr.GetInt32(1), ChildAliasId = rdr.GetInt32(0) });
            }

            Logger.Write("Getting Sibling Tag Relationships that haven't been retroactively applied");
            return SiblingRelationships;
        }

        public List<RelationshipItem> GetBrokenSiblingRelationships()
        {
            List<RelationshipItem> SiblingRelationships = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "select * from sibling_data left join tag_data on sibling_data.old_id = tag_data.tagid where tag_data.tagid is null;";

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                SiblingRelationships.Add(new RelationshipItem() { ParentPreferredId = rdr.GetInt32(1), ChildAliasId = rdr.GetInt32(0) });
            }

            using SQLiteCommand cmd2 = new(SQLiteConn);
            cmd2.CommandText = "select * from sibling_data left join tag_data on sibling_data.new_id = tag_data.tagid where tag_data.tagid is null;";

            cmd2.Prepare();
            using SQLiteDataReader rdr2 = cmd2.ExecuteReader();

            while (rdr2.Read())
            {
                SiblingRelationships.Add(new RelationshipItem() { ParentPreferredId = rdr2.GetInt32(1), ChildAliasId = rdr2.GetInt32(0) });
            }

            Logger.Write("Getting Sibling Tag Relationships where alias or preferred tag no longer exists");
            return SiblingRelationships;
        }

        #endregion

        #region parent_data Add/Update/Delete Functions

        public void AddRowToParentTable(string child, string parent)
        {
            int childid = GetTagid(child);
            int parentid = GetTagid(parent);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO parent_data (child_id, parent_id, retro) VALUES (@child_id, @parent_id, @retro);";

            cmd.Parameters.AddWithValue("@child_id", childid);
            cmd.Parameters.AddWithValue("@parent_id", parentid);
            cmd.Parameters.AddWithValue("@retro", false);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Adding Parent Relationship to Database: Child - " + child + " & Parent - " + parent);
        }

        public void AddParentTag(string child, string parent)
        {
            if (parent == child)   //Don't do anything if parent and child tag are the same
            {
                return;
            }

            int parentid = GetTagid(parent);
            int childid = GetTagid(child);

            foreach (string f in GetParentTags(parent))   //Don't do anything if parent has a parent tag that is the child tag
            {
                if (f == child)
                {
                    return;
                }
            }

            if (!CheckifParentRelationshipExists(childid, parentid))
            {
                if (childid == -1)    //don't do anything if the child tag doesn't exist
                {
                    return;
                }
                else
                {
                    if (parentid == -1)  //if the parent tag doesn't exist just add a row to the parent tag table
                    {
                        InsertIntoTagData(parent, GetTagCateory(childid));
                    }
                    AddRowToParentTable(child, parent);
                }
            }
        }

        public void DeleteParentRelationship(string child, string parent)
        {
            int parentid = GetTagid(parent);
            int childid = GetTagid(child);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "DELETE FROM parent_data WHERE parent_id=@parent_id AND child_id=@child_id";

            cmd.Parameters.AddWithValue("@parent_id", parentid);
            cmd.Parameters.AddWithValue("@child_id", childid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Deleting Parent Relationship to Database: Child - " + child + " & Parent - " + parent);
        }

        public void DeleteParentRelationship(int childid, int parentid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "DELETE FROM parent_data WHERE parent_id=@parent_id AND child_id=@child_id";

            cmd.Parameters.AddWithValue("@parent_id", parentid);
            cmd.Parameters.AddWithValue("@child_id", childid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Deleting Parent Relationship to Database: Child - " + GetTagName(childid) + " & Parent - " + GetTagName(parentid));
        }

        public void UpdateChildTag(int oldchildid, int newchildid, int parentid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE parent_data SET child_id=@newchild WHERE parent_id=@preant AND child_id=@oldchild";

            cmd.Parameters.AddWithValue("@newchild", SqlDbType.Int).Value = newchildid;
            cmd.Parameters.AddWithValue("@parent", parentid);
            cmd.Parameters.AddWithValue("@oldchild", SqlDbType.Int).Value = oldchildid;

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updating Child tag in Parent Relationship: previous Child - " + GetTagName(oldchildid) + " to new child " + GetTagName(newchildid) + " & Parent - " + GetTagName(parentid));
        }

        public void UpdateParentTag(int childid, int oldparentid, int newparentid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE parent_data SET parent_id=@newparent WHERE parent_id=@oldparent AND child_id=@child";

            cmd.Parameters.AddWithValue("@newparent", SqlDbType.Int).Value = newparentid;
            cmd.Parameters.AddWithValue("@oldparent", SqlDbType.Int).Value = oldparentid;
            cmd.Parameters.AddWithValue("@child", childid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updating Parent tag in Parent Relationship: Child - " + GetTagName(childid) + " & previous Parent - " + GetTagName(oldparentid) + " to new Parent " + GetTagName(newparentid));
        }


        public void UpdateParentRetro(int childid, int parentid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE parent_data SET retro=@retro WHERE parent_id=@parent AND child_id=@child";

            cmd.Parameters.AddWithValue("@retro", true);
            cmd.Parameters.AddWithValue("@parent", parentid);
            cmd.Parameters.AddWithValue("@child", childid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Setting Retro Flag for Parent Relationship: Child - " + GetTagName(childid) + " & Parent - " + GetTagName(parentid));
        }

        #endregion

        #region parent_data Select Functions

        public List<string> GetParentTags(string child)
        {
            int childid = GetTagid(child);
            List<int> ParentTagIDs = new();
            List<string> ParentTags = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT parent_id FROM parent_data WHERE child_id=@child";

            cmd.Parameters.AddWithValue("@child", childid);

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();
                
            while (rdr.Read())
            {
                ParentTagIDs.Add(rdr.GetInt32(0));
            }

            foreach (int ID in ParentTagIDs)
            {
                ParentTags.Add(GetTagName(ID));
            }
            return ParentTags;
        }

        public bool CheckifParentRelationshipExists(int childid, int parentid)
        {

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT COUNT(*) FROM parent_data WHERE child_id=@child AND parent_id=@parent";

            cmd.Parameters.AddWithValue("@child", childid);
            cmd.Parameters.AddWithValue("@parent", parentid);

            cmd.Prepare();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public List<RelationshipItem> GetAllParentTagRelationships()
        {
            List<RelationshipItem> ParentTags = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM parent_data";

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();
                
            while (rdr.Read())
            {
                ParentTags.Add(new RelationshipItem() { ParentPreferredId = rdr.GetInt32(1), ChildAliasId = rdr.GetInt32(0) });
            }

            foreach (RelationshipItem g in ParentTags)
            {
                g.ChildAliasTag = GetTagName(g.ChildAliasId);
                g.ParentPreferredTag = GetTagName(g.ParentPreferredId);
            }

            Logger.Write("Getting all parent tag relationships");
            return ParentTags.OrderBy(x => x.ChildAliasTag).ToList();
        }

        public string DisplayParentTags(string child, string disp, int limit, int level = 1)
        {
            List<string> ParentTags = GetParentTags(child);
            if (ParentTags.Count > 0)
            {
                foreach (string Tag in ParentTags)
                {
                    int count = GetTagCount(GetTagid(Tag));
                    disp += Environment.NewLine + new string(' ', level * 3) + Tag + " [" + count.ToString() + "]";
                    if (level < limit)
                    {
                        disp = DisplayParentTags(Tag, disp, limit, level + 1);
                    }
                }
            }
            return disp;
        }

        public List<RelationshipItem> GetRetroParentTagRelationships()
        {
            List<RelationshipItem> ParentRelationships = new();
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM parent_data WHERE retro=0 OR retro IS NULL;";

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                ParentRelationships.Add(new RelationshipItem() { ParentPreferredId = rdr.GetInt32(1), ChildAliasId = rdr.GetInt32(0) });
            }

            Logger.Write("Getting parent tag relationships that need to be retroactively applied");
            return ParentRelationships;
        }

        public List<RelationshipItem> GetBrokenParentRelationships()
        {
            List<RelationshipItem> ParentRelationships = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "select * from parent_data left join tag_data on parent_data.child_id = tag_data.tagid where tag_data.tagid is null;";

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                ParentRelationships.Add(new RelationshipItem() { ParentPreferredId = rdr.GetInt32(1), ChildAliasId = rdr.GetInt32(0) });
            }

            using SQLiteCommand cmd2 = new(SQLiteConn);
            cmd2.CommandText = "select * from parent_data left join tag_data on parent_data.parent_id = tag_data.tagid where tag_data.tagid is null;";

            cmd.Prepare();
            using SQLiteDataReader rdr2 = cmd2.ExecuteReader();

            while (rdr2.Read())
            {
                ParentRelationships.Add(new RelationshipItem() { ParentPreferredId = rdr2.GetInt32(1), ChildAliasId = rdr2.GetInt32(0) });
            }

            Logger.Write("Getting parent tag relationships where child tag or parent tag don't exist");
            return ParentRelationships;
        }

        public List<RelationshipItem> GetParentTagRelationshipsContaining(string tag)
        {
            List<RelationshipItem> ParentTags = new List<RelationshipItem>();

            int tagid = GetTagid(tag);

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM parent_data where child_id=@child or parent_id=@parent";

            cmd.Prepare();
            cmd.Parameters.AddWithValue("@child", tagid);
            cmd.Parameters.AddWithValue("@parent", tagid);
            using SQLiteDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                ParentTags.Add(new RelationshipItem() { ParentPreferredId = rdr.GetInt32(1), ChildAliasId = rdr.GetInt32(0) });
            }

            foreach (RelationshipItem g in ParentTags)
            {
                g.ChildAliasTag = GetTagName(g.ChildAliasId);
                g.ParentPreferredTag = GetTagName(g.ParentPreferredId);
            }

            Logger.Write("Getting parent tag relationships where child tag or parent tag is " + tag);
            return ParentTags.OrderBy(x => x.ChildAliasTag).ToList();
        }

        #endregion

        #region dupes Add/Update/Delete Functions

        public void AddRowToDupesTable(string hash1, string hash2, float score)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO dupes (hash_1, hash_2, score) VALUES (@hash1, @hash2, @score);";

            cmd.Parameters.AddWithValue("@hash1", hash1);
            cmd.Parameters.AddWithValue("@hash2", hash2);
            cmd.Parameters.AddWithValue("@score", score);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public void UpdateDupesProcessed(string hash1, string hash2)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE dupes set processed=@processed WHERE hash_1=@hash1 and hash_2=@hash2";

            cmd.Parameters.AddWithValue("@processed", true);
            cmd.Parameters.AddWithValue("@hash1", hash1);
            cmd.Parameters.AddWithValue("@hash2", hash2);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        public void DeleteDupe(string hash)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "delete from dupes WHERE hash_1=@hash or hash_2=@hash";

            cmd.Parameters.AddWithValue("@hash", hash);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region dupes Select Functions

        public bool CheckifDupeExists(string hash1, string hash2)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT COUNT(*) FROM dupes WHERE (hash_1=@hash1 AND hash_2=@hash2) OR (hash_1=@hash2 AND hash_2=@hash1)";

            cmd.Parameters.AddWithValue("@hash1", hash1);
            cmd.Parameters.AddWithValue("@hash2", hash2);

            cmd.Prepare();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public List<DupesItem> GetDupes()
        {
            List<DupesItem> Output = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM dupes where processed = @precessed or processed is null";

            cmd.Parameters.AddWithValue("@processed", false);
            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();
                
            while (rdr.Read())
            {
                Output.Add(new DupesItem() { hash1 = rdr["hash_1"].ToString(), hash2 = rdr["hash_2"].ToString() });
            }

            return Output;
        }

        public int GetDupesCount()
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT COUNT(*) FROM dupes WHERE processed=@processed or processed is null";

            cmd.Parameters.AddWithValue("@processed", false);

            cmd.Prepare();
            return (int)cmd.ExecuteScalar();
        }

        #endregion

        #region deleted_images Add/Update/Delete Functions

        public void AddRowToDeletedImagesTable(string hash)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO deleted_images (hash, deleted, date_added) VALUES (@hash, @deleted, @date_added);";

            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@deleted", false);
            cmd.Parameters.AddWithValue("@date_added", DateTime.Now.Date);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Adding File: " + GetPath(hash) + " to deleted files table");
        }

        public void UpdateDeleted(string hash)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE deleted_images set deleted=@deleted WHERE hash=@hash";

            cmd.Parameters.AddWithValue("@deleted", true);
            cmd.Parameters.AddWithValue("@hash", hash);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region deleted_images Select Functions

        public List<string> GetImagesToDelete()
        {
            List<string> Output = new();
            List<string> TrueOutput = new();

            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT * FROM deleted_images where (deleted = @deleted or deleted is null) and date_added < DATEADD(day, -30, GETDATE());";

            cmd.Parameters.AddWithValue("@deleted", false);
            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();
                
            while (rdr.Read())
            {
                Output.Add(rdr["hash"].ToString());
            }

            foreach (string o in Output)
            {
                string path = GetPath(o);
                if (string.IsNullOrEmpty(path))
                {
                    UpdateDeleted(o);
                }
                else
                {
                    TrueOutput.Add(path);
                }
            }

            Logger.Write("Getting all images to be deleted");
            return TrueOutput;
        }

        public bool CheckIfHashHasBeenDeletedBefore(string hash)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT COUNT(*) FROM deleted_images WHERE hash=@hash";

            cmd.Parameters.AddWithValue("@hash", hash);

            cmd.Prepare();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        #endregion

        #region Booru tag map Add/Update/Delete Functions

        public void AddRowToTagMapTable(string boorutag, int tagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "INSERT INTO booru_tag_map (booru_tag, tagid) VALUES (@booru_tag, @tagid);";

            cmd.Parameters.AddWithValue("@booru_tag", boorutag);
            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Adding tag map between booru tag: " + boorutag + " and Tag: " + GetTagName(tagid));
        }

        public void UpdateTagMapPodoboID(string boorutag, int oldtagid, int newtagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE booru_tag_map set tagid=@newtagid WHERE booru_tag=@booru_tag and tagid=@oldtagid";

            cmd.Parameters.AddWithValue("@newtagid", newtagid);
            cmd.Parameters.AddWithValue("@booru_tag", boorutag);
            cmd.Parameters.AddWithValue("@oldtagid", oldtagid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updating Tag Map Podobo Tag: " + GetTagName(oldtagid) + " to new Podobo Tag: " + GetTagName(newtagid) + " for booru tag: " + boorutag);
        }

        public void UpdateTagMapBooruTag(string oldboorutag, string newboorutag, int tagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "UPDATE booru_tag_map set booru_tag=@newbooru WHERE booru_tag=@oldbooru and tagid=@tagid";

            cmd.Parameters.AddWithValue("@newbooru", newboorutag);
            cmd.Parameters.AddWithValue("@oldbooru", oldboorutag);
            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();

            Logger.Write("Updating Tag Map Booru Tag: " + oldboorutag + " to new Booru Tag: " + newboorutag + " for Podobo tag: " + GetTagName(tagid));
        }

        public void RemoveTagMap(string boorutag, int tagid)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "DELETE from booru_tag_map WHERE booru_tag=@booru_tag and tagid=@tagid";

            cmd.Parameters.AddWithValue("@booru_tag", boorutag);
            cmd.Parameters.AddWithValue("@tagid", tagid);

            cmd.Prepare();
            cmd.ExecuteNonQuery();
            
            Logger.Write("Deleteing Tag Map: " + boorutag + " ->: " + GetTagName(tagid));
        }

        #endregion

        #region Booru Tag map Select Functions

        public bool CheckIfTagMapExists(string boorutag)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT COUNT(*) FROM booru_tag_map WHERE booru_tag=@booru_tag";

            cmd.Parameters.AddWithValue("@booru_tag", boorutag);

            cmd.Prepare();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public int GetTagIdfromTagMap(string boorutag)
        {
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT tagid FROM booru_tag_map WHERE booru_tag=@booru_tag";

            cmd.Parameters.AddWithValue("@booru_tag", boorutag);
            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();
            return rdr.Read() ? rdr.GetInt32(0) : -1;
        }

        public string GetTagNamefromTagMap(string boorutag)
        {
            int tagid;
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "SELECT tagid FROM booru_tag_map WHERE booru_tag=@booru_tag";

            cmd.Parameters.AddWithValue("@booru_tag", boorutag);
            cmd.Prepare();

            using SQLiteDataReader rdr = cmd.ExecuteReader();
            tagid = rdr.Read() ? rdr.GetInt32(0) : -1;

            return tagid == -1 ? "" : GetTagName(tagid);
        }

        #endregion

        public List<string> GetEmptySources(string Sourcekey)
        {
            List<string> Output = new();
            using SQLiteCommand cmd = new(SQLiteConn);
            cmd.CommandText = "select * from image_data full join sources on image_data.sha_id = sources.sha_id where path like '%" + Sourcekey + "%' and source is null";

            cmd.Prepare();
            using SQLiteDataReader rdr = cmd.ExecuteReader();
                
            while (rdr.Read())
            {
                Output.Add(rdr["path"].ToString());
            }

            return Output;
        }
    }

    public class TagItem
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public int ID;
        public int CategoryID;
        public string Category;
        public string Display;
        public int Count;
        public string Description;
    }

    public class ImageItem
    {
        public string hash;
        public string path;
        public DateTime date_added;
        public DateTime date_modfied;        
        public int height;
        public int width;
        public int rating;
        public string tag_list;
        public string primary_source;
        public bool video;
        public bool sound;
        public byte[] phash;
        public string md5;
    }

    //public class ImageThumbnailItem
    //{
    //    public string hash;
    //    public string path;
    //    public string name;
    //    public bool isTagged;
    //    public int rating;
    //    public string primary_source;
    //    public bool video;
    //    public bool sound;
    //    public int duration;
    //}

    public class RelationshipItem
    {
        public string ParentPreferredTag;
        public string ChildAliasTag;
        public int ParentPreferredId;
        public int ChildAliasId;
    }

    public class CategoryItem
    {
        public static List<string> CategoryList = new string[] { "Female", "Male", "Artist", "Studio", "IP", "Title", "Afilliation", "Race", "Sex", "Body Part", "Clothing", "Action", "Position", "Setting", "General", "Date", "Meta" }.ToList();

        public static Dictionary<int, CategoryElements> CategoryByID = new();
        public static Dictionary<string, CategoryElements> CategoryByName = new();
        public CategoryItem()
        {
            CategoryByID.Add(2, new CategoryElements() { Category = "Female", Color = "OrangeRed", DisplayName = "Female" });
            CategoryByID.Add(3, new CategoryElements() { Category = "Male", Color = "OrangeRed", DisplayName = "Male" });
            CategoryByID.Add(4, new CategoryElements() { Category = "Artist", Color = "DarkOrange", DisplayName = "Artist" });
            CategoryByID.Add(5, new CategoryElements() { Category = "Studio", Color = "AquaMarine", DisplayName = "Studio/Network" });
            CategoryByID.Add(1, new CategoryElements() { Category = "IP", Color = "CornflowerBlue", DisplayName = "IP/Series" });
            CategoryByID.Add(15, new CategoryElements() { Category = "Title", Color = "DarkSeaGreen", DisplayName = "Title" });
            CategoryByID.Add(7, new CategoryElements() { Category = "Afilliation", Color = "ForestGreen", DisplayName = "Affiliation/Group" });
            CategoryByID.Add(8, new CategoryElements() { Category = "Race", Color = "BlueViolet", DisplayName = "Race/Species/Ethnicity" });
            CategoryByID.Add(6, new CategoryElements() { Category = "Sex", Color = "MediumOrchid", DisplayName = "Sex" });
            CategoryByID.Add(9, new CategoryElements() { Category = "Body Part", Color = "White", DisplayName = "Body Part" });
            CategoryByID.Add(10, new CategoryElements() { Category = "Clothing", Color = "White", DisplayName = "Clothing/Accessories" });
            CategoryByID.Add(13, new CategoryElements() { Category = "Action", Color = "White", DisplayName = "Action" });
            CategoryByID.Add(11, new CategoryElements() { Category = "Position", Color = "White", DisplayName = "Position" });
            CategoryByID.Add(12, new CategoryElements() { Category = "Setting", Color = "White", DisplayName = "Setting" });
            CategoryByID.Add(0, new CategoryElements() { Category = "General", Color = "White", DisplayName = "General" });
            CategoryByID.Add(16, new CategoryElements() { Category = "Date", Color = "White", DisplayName = "Release Date" });
            CategoryByID.Add(14, new CategoryElements() { Category = "Meta", Color = "GoldenRod", DisplayName = "Meta" });

            CategoryByName.Add("Female", new CategoryElements() { CategoryID = 2, Color = "OrangeRed", DisplayName = "Female" });
            CategoryByName.Add("Male", new CategoryElements() { CategoryID = 3, Color = "OrangeRed", DisplayName = "Male" });
            CategoryByName.Add("Artist", new CategoryElements() { CategoryID = 4, Color = "DarkOrange", DisplayName = "Artist" });
            CategoryByName.Add("Studio", new CategoryElements() { CategoryID = 5, Color = "AquaMarine", DisplayName = "Studio/Network" });
            CategoryByName.Add("IP", new CategoryElements() { CategoryID = 1, Color = "CornflowerBlue", DisplayName = "IP/Series" });
            CategoryByName.Add("Title", new CategoryElements() { CategoryID = 15, Color = "DarkSeaGreen", DisplayName = "Title" });
            CategoryByName.Add("Afilliation", new CategoryElements() { CategoryID = 7, Color = "ForestGreen", DisplayName = "Affiliation/Group" });
            CategoryByName.Add("Race", new CategoryElements() { CategoryID = 8, Color = "BlueViolet", DisplayName = "Race/Species/Ethnicity" });
            CategoryByName.Add("Sex", new CategoryElements() { CategoryID = 6, Color = "MediumOrchid", DisplayName = "Sex" });
            CategoryByName.Add("Body Part", new CategoryElements() { CategoryID = 9, Color = "White", DisplayName = "Body Part" });
            CategoryByName.Add("Clothing", new CategoryElements() { CategoryID = 10, Color = "White", DisplayName = "Clothing/Accessories" });
            CategoryByName.Add("Action", new CategoryElements() { CategoryID = 13, Color = "White", DisplayName = "Action" });
            CategoryByName.Add("Position", new CategoryElements() { CategoryID = 11, Color = "White", DisplayName = "Position" });
            CategoryByName.Add("Setting", new CategoryElements() { CategoryID = 12, Color = "White", DisplayName = "Setting" });
            CategoryByName.Add("General", new CategoryElements() { CategoryID = 0, Color = "White", DisplayName = "General" });
            CategoryByName.Add("Date", new CategoryElements() { CategoryID = 16, Color = "White", DisplayName = "Release Date" });
            CategoryByName.Add("Meta", new CategoryElements() { CategoryID = 14, Color = "GoldenRod", DisplayName = "Meta" });
        }
    }

    public class CategoryElements
    {
        public string Category;
        public int CategoryID;
        public string Color;
        public string DisplayName;
    }

    public class DupesItem
    {
        public string hash1;
        public string hash2;
    }

}
