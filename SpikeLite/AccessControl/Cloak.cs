/**
 * SpikeLite C# IRC Bot
 * Copyright (c) 2008 FreeNode ##Csharp Community
 * 
 * This source is licensed under the terms of the MIT license. Please see the 
 * distributed license.txt for details.
 */
using System;
using System.Data.SQLite;
using Sharkbite.Irc;
using System.Configuration;

// TODO: Kog 08/25/2008 - refactor to use hibernate

namespace SpikeLite.AccessControl
{
    public enum CloakMatchType
    { 
        Full = 0,
        Start = 1,
        End = 2,
        Contains = 3
    }

    public enum AccessLevel
    {
        None = 0,
        Private = 1,
        Public = 2,
        Admin = 3,
        PermanentAdmin = 4
    }

    public class Cloak
    {
        #region Fields

        private int _id = 0;
        private string _cloakMask = string.Empty;
        private CloakMatchType _cloakMatchType;
        private AccessLevel _userLevel = AccessLevel.None;
        private Connection _connection;

        #endregion

        #region Properties

        public string CloakMask
        {
            get { return _cloakMask; }
            set { _cloakMask = value; }
        }

        public CloakMatchType CloakMatchType
        {
            get { return _cloakMatchType; }
            set { _cloakMatchType = value; }
        }

        public AccessLevel UserLevel
        {
            get { return _userLevel; }
            set { _userLevel = value; }
        }

        #endregion

        #region Constructors

        public Cloak(Connection ircClient)
        {
            _connection = ircClient;

            Init();
        }
        public Cloak(int id, Connection ircClient)
        {
            _id = id;
            _connection = ircClient;

            Init();

            Populate();
        }

        #endregion

        #region Methods

        private void Init(){ }

        #region DataAccess

        private void Delete()
        {
            using (SQLiteConnection con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["AccessControl"].ConnectionString))
            {
                using (SQLiteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = String.Format("DELETE FROM Cloak WHERE ID = {0}", _id);

                    con.Open();

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
        }

        private void Persist()
        {
            using (SQLiteConnection con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["AccessControl"].ConnectionString))
            {
                using (SQLiteCommand cmd = con.CreateCommand())
                {
                    if (_id == 0)
                    {
                        cmd.CommandText = string.Format("INSERT INTO Cloak (Cloak, MatchType, UserLevel) VALUES('{0}', {1}, {2})",
                                                        _cloakMask,
                                                        _cloakMatchType,
                                                        _userLevel);
                        
                        _id = GetCloakID();
                    }
                    else
                    {
                        cmd.CommandText = string.Format("UPDATE Cloak SET Cloak = '{0}', MatchType = {1}, UserLevel = {2}", 
                                                        _cloakMask, 
                                                        _cloakMatchType, 
                                                        _userLevel);
                    }

                    con.Open();

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
        }

        private void Populate()
        {
            if (_id > 0)
            {
                using (SQLiteConnection con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["AccessControl"].ConnectionString))
                {
                    using (SQLiteCommand cmd = con.CreateCommand())
                    {
                        cmd.CommandText = string.Format("SELECT * FROM Cloak WHERE ID = {0}", _id);

                        con.Open();

                        try
                        {
                            SQLiteDataReader reader = cmd.ExecuteReader();

                            if (reader.Read())
                            {
                                _cloakMask = reader.GetString(reader.GetOrdinal("Cloak"));
                                _cloakMatchType = (CloakMatchType)reader.GetInt32(reader.GetOrdinal("MatchType"));
                                _userLevel = (AccessLevel)reader.GetInt32(reader.GetOrdinal("UserLevel"));
                            }
                            else
                            {
                                throw new ApplicationException(string.Format("Record with ID '{0}' doesn't exist.", _id));
                            }
                        }
                        finally
                        {
                            con.Close();
                        }
                    }
                }
            }
            else
            {
                throw new ApplicationException("ID must be greater than 0.");
            }
        }

        private int GetCloakID()
        {
            using (SQLiteConnection con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["AccessControl"].ConnectionString))
            {
                using (SQLiteCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = string.Format("SELECT ID FROM Cloak WHERE Cloak = '{0}'", _cloakMask);

                    con.Open();

                    try
                    {
                        SQLiteDataReader reader = cmd.ExecuteReader();
                        int id = -1;

                        if (reader.Read())
                        {
                            id = reader.GetInt32(reader.GetOrdinal("ID"));
                        }

                        return id;
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
        }

        #endregion

        public bool Matches( string otherMask )
        {
            switch ( CloakMatchType )
            {
                case CloakMatchType.Full:
                    return CloakMask == otherMask;
                case CloakMatchType.Start:
                    return otherMask.StartsWith(CloakMask);
                case CloakMatchType.End:
                    return otherMask.EndsWith(CloakMask);
                case CloakMatchType.Contains:
                    return otherMask.Contains(CloakMask);
                default:
                    throw new InvalidOperationException(string.Format("Unrecognised CloakMatchType {0}", CloakMatchType));
            }
        }

        #endregion
    }
}
