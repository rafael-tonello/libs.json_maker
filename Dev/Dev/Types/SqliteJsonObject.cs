/*
 * Created by SharpDevelop.
 * User: rafael.tonello
 * Date: 13/11/2018
 * Time: 13:39
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Data.SQLite;
using System.Collections.Generic;

namespace JsonMaker
{
    /// <summary>
    /// Description of SqliteJsonObject.
    /// </summary>
    public class SqliteJsonObject : IJSONObject
    {
        protected SQLiteConnection Connection = null;
        public string dbFileName = "";
        private SqliteJsonObject modelObject;
        
        
        string keyName = "";

        public override void Initialize(IJSONObject pParent, string relativeName, IJSONObject modelObject)
        {            
            this.modelObject = (SqliteJsonObject)modelObject;
            
            this.parent = pParent;
            
            if (this.modelObject.Connection == null)
            {
                this.modelObject.Connection = new SQLiteConnection("Data Source="+this.modelObject.dbFileName+";Version=3;");
                this.modelObject.Connection.Open();
                
                SQLiteCommand command = new SQLiteCommand("PRAGMA journal_mode = WAL", this.modelObject.Connection);
                command.ExecuteNonQuery();
                
                command = new SQLiteCommand("PRAGMA synchronous = NORMAL", this.modelObject.Connection);
                command.ExecuteNonQuery();
                
                

            }

            if (relativeName.StartsWith("."))
                relativeName = relativeName.Substring(1);
                
            
            this.keyName = relativeName;
        }
        public override void clear()
        {
            SQLiteTransaction transaction = this.modelObject.Connection.BeginTransaction();
            
            SQLiteCommand command = new SQLiteCommand("DELETE FROM KeyValueStorage WHERE Key LIKE '"+this.keyName+"%'", this.modelObject.Connection);
            command.ExecuteNonQuery();
            
            transaction.Commit();
            
        }

        public override void delete(string name)
        {
            SQLiteTransaction transaction = this.modelObject.Connection.BeginTransaction();
            string whereClausule = "'" + this.keyName + "." + name + "%'";
            if (this.keyName == "")
                whereClausule = "'" + name + "%'";
                
            SQLiteCommand command = new SQLiteCommand("DELETE FROM KeyValueStorage WHERE Key = "+whereClausule, this.modelObject.Connection);
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        public override SOType getJSONType()
        {
            return __determineSoType(this.serializeSingleValue());
        }

        public override void setChild(string name, IJSONObject child)
        {
            
        }

        public override void setSingleValue(string value)
        {
            //if this is a object (with childs), remove all childs data
            this.clear();

            //THE DATABASE FILE was configured to replace records with same key (key is primarykey)
            SQLiteTransaction transaction = this.modelObject.Connection.BeginTransaction();
            string cmdStr = "INSERT INTO KeyValueStorage (Key, Value) VALUES ('" + this.keyName + "', '" + value + "')";
            if (cmdStr.ToLower().Contains("options"))
            {
                int i = 10;
                int b = i * i;
                b = b + 1;
                i = i * b;
                
                cmdStr += "";
            }
            
            SQLiteCommand command = new SQLiteCommand(cmdStr, this.modelObject.Connection);
            command.ExecuteNonQuery();
            transaction.Commit();
        }

        public override bool __containsChild(string name)
        {
            string sqlText = "SELECT COUNT(*) FROM KeyValueStorage WHERE Key LIKE '"+name+"%'";
            
            if (this.keyName != "")
                sqlText = "SELECT COUNT(*) FROM KeyValueStorage WHERE Key LIKE '"+this.keyName+"."+name+"%'";
            SQLiteCommand command = new SQLiteCommand(sqlText, this.modelObject.Connection);
            var ret = command.ExecuteReader();
            var result = ret.Read();
            
            ret.Close();
            command.Dispose();
            return result;
        }

        public override List<string> __getChildsNames()
        {
            string sqlText = "SELECT Key FROM KeyValueStorage";
            if (this.keyName != "")
                sqlText = "SELECT Key FROM KeyValueStorage WHERE Key LIKE '" + this.keyName + "%'";
            List<string> ret = new List<string>();
            SQLiteCommand command = new SQLiteCommand(this.modelObject.Connection);
            command.CommandText = sqlText;
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                string temp = reader.GetString(0);
                if (temp != this.keyName)
                {
                    if (temp.Length > this.keyName.Length)
                        temp = temp.Substring(this.keyName.Length);
                    
                    if (temp != "" && temp[0] == '.')
                        temp = temp.Substring(1);
                
                    if (temp.Contains("."))
                        temp = temp.Substring(0, temp.IndexOf('.'));
                
                    if ((temp != "") && (!ret.Contains(temp)))
                        ret.Add(temp);
                }
            }
            
            reader.Close();
            command.Dispose();
            return ret;
            
        }

        protected override string serializeSingleValue()
        {
            string ret = "null";
            
            
            string whereClausule = "'" + this.keyName + "'";
            
            SQLiteCommand command = new SQLiteCommand("SELECT Value FROM KeyValueStorage WHERE Key LIKE "+whereClausule, this.modelObject.Connection);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var raw = reader.GetValue(0);
                ret = reader.GetValue(0).ToString();
                
                if (raw is string)
                    ret = '"' + ret + '"';
            }
            
            reader.Close();
            command.Dispose();
            
            return ret;
        }

        public override IJSONObject __getChild(string name)
        {
            var ret = new SqliteJsonObject();
            ret.Initialize(this, this.keyName + "." + name, this.modelObject);
            return ret;
        }

        public override string getRelativeName()
        {
            return this.keyName;
        }

        public override bool isDeletable()
        {
            return true;
        }
    }
}
