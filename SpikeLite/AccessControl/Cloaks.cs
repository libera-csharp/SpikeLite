/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Sharkbite.Irc;
using System.Configuration;

namespace SpikeLite.AccessControl
{
    public class Cloaks : IList<Cloak>
    {
        #region Fields

        private Connection _connection;
        private List<Cloak> _internedCloaks = new List<Cloak>();
        private Dictionary<string, Cloak> _cloakDictionary = new Dictionary<string, Cloak>();

        #endregion

        #region Properties

        public Dictionary<string, Cloak> CloakIndex
        {
            get { return _cloakDictionary; }
        }

        #endregion

        #region Constuctor

        public Cloaks(Connection ircClient)
        {
            _connection = ircClient;

            Populate();
        }

        #endregion

        private void Populate()
        {
            using (SQLiteConnection con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["AccessControl"].ConnectionString))
            {
                using (SQLiteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT ID FROM Cloak";

                    con.Open();

                    try
                    {
                        SQLiteDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            Cloak cloak = new Cloak(reader.GetInt32(reader.GetOrdinal("ID")), _connection);

                            Add(cloak);

                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("{0} {1} Loaded Cloak: {2}, Match Type: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), cloak.CloakMask, cloak.CloakMatchType.ToString());
                        }
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
        }

        #region IList<Cloak> Members

        public int IndexOf(Cloak item)
        {
            return _internedCloaks.IndexOf(item);
        }

        public void Insert(int index, Cloak item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RemoveAt(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Cloak this[int index]
        {
            get { return _internedCloaks[index]; }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region ICollection<Cloak> Members

        public void Add(Cloak item)
        {
            _internedCloaks.Add(item);
            _cloakDictionary.Add(item.CloakMask, item);
        }

        public void Clear()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Contains(Cloak item)
        {
            return _internedCloaks.Contains(item);
        }

        public void CopyTo(Cloak[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count
        {
            get { return _internedCloaks.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Cloak item)
        {
            _cloakDictionary.Remove(item.CloakMask);

            return _internedCloaks.Remove(item);
        }

        #endregion

        #region IEnumerable<Cloak> Members

        public IEnumerator<Cloak> GetEnumerator()
        {
            return _internedCloaks.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _internedCloaks.GetEnumerator();
        }

        #endregion
    }
}