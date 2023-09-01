using Microsoft.Data.Sqlite;
using Dapper;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Threading;

namespace BRB5
{
    public enum eTypeCommit
    {
        Auto,
        Manual
    }

    public class SQLite: IDisposable
    {
        SqliteConnection connection;
        SqliteTransaction transaction = null;
        public eTypeCommit TypeCommit { get; set; }
        /// <summary>
        /// Чи можна користуватись базою
        /// </summary>
        protected static bool IsLock = false;

        public SQLite()
        {
            var connectionString = new SqliteConnectionStringBuilder("Data Source=SQLite.db" + ";Version=3;")
            {
                
                // DefaultIsolationLevel = IsolationLevel.Serializable
            }.ToString();

            connection = new SqliteConnection(connectionString);
            connection.Open();
            //TypeCommit = eTypeCommit.Auto;
        }

        
        private bool disposedValue;

        public SQLite(String varConectionString) 
        {

            var connectionString = new SqliteConnectionStringBuilder("Data Source=" + varConectionString + ";")
            {
               // DefaultIsolationLevel = IsolationLevel.Serializable
            }.ToString();

            connection = new SqliteConnection(connectionString);
            connection.Open();
            TypeCommit = eTypeCommit.Auto;
        }

        ~SQLite()
        {
            //Close();
        }
        public  void Close(bool isWait = false)
        {
            //          $"[{GetType()} -{ GetHashCode()}] close connection".WriteLogMessage();
            //Зупиняємо всі запити до БД і чекаємо 1/4 секунди. щоб встигли завершитись запити.

            if (isWait)
            {
                SetLock(true);
                Thread.Sleep(250);
            }
            if (connection != null)
            {
                connection.Close();
                connection = null;
            }
            if (isWait)
            {
                WaitCollect(150);
            }
            SetLock(false);
        }

        void WaitCollect(int pMs = 150)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(pMs);
        }

        public  IEnumerable<T1> Execute<T, T1>(string query, T parameters)
        {
            if (IsLock) ExceptionIsLock();
            return connection.Query<T1>(query, parameters);
        }

        public  IEnumerable<T1> Execute<T1>(string query)
        {
            if (IsLock) ExceptionIsLock();
            return connection.Query<T1>(query);
        }

        public  Task<IEnumerable<T1>> ExecuteAsync<T, T1>(string query, T parameters)
        {
            if (IsLock) ExceptionIsLock();
            return connection.QueryAsync<T1>(query, parameters);
        }

        public  Task<IEnumerable<T1>> ExecuteAsync<T1>(string query)
        {
            if (IsLock) ExceptionIsLock();
            return connection.QueryAsync<T1>(query);
        }
        public  void BeginTransaction()
        {
            transaction = connection.BeginTransaction();
        }

        public  void CommitTransaction()
        {
            transaction.Commit();
        }

        public  int ExecuteNonQuery<T>(string pQuery, T Parameters, int CountTry = 3)
        {
            if (IsLock) ExceptionIsLock();
            try
            {
                if (TypeCommit == eTypeCommit.Auto)
                    return connection.Execute(pQuery, Parameters);
                else
                    return connection.Execute(pQuery, Parameters, transaction);
            }
            catch (Exception e)
            {
                CountTry--;
                if (CountTry > 0 && e.Message.Contains("database is locked"))
                {
                    //FileLogger.WriteLogMessage($"ExecuteNonQuery<T> CountTry=>{CountTry} SQL=>{pQuery}");
                    WaitCollect(100);
                    return ExecuteNonQuery(pQuery, Parameters, CountTry);
                }
                throw new Exception(e.Message, e);
            }
        }

        public  int ExecuteNonQuery(string pQuery, int CountTry = 3)
        {
            if (IsLock) ExceptionIsLock();
            try
            {
                if (TypeCommit == eTypeCommit.Auto)
                    return connection.Execute(pQuery);
                else
                    return connection.Execute(pQuery, null, transaction);
            }
            catch (Exception e)
            {
                CountTry--;
                if (CountTry > 0 && e.Message.Contains("database is locked"))
                {
                    //FileLogger.WriteLogMessage($"ExecuteNonQuery CountTry=>{CountTry} SQL=>{pQuery}");
                    WaitCollect(100);
                    return ExecuteNonQuery(pQuery, CountTry);
                }
                throw new Exception(e.Message, e);
            }
        }

        public  Task<int> ExecuteNonQueryAsync<T>(string parQuery, T Parameters)
        {
            if (IsLock) ExceptionIsLock();
            if (TypeCommit == eTypeCommit.Auto)
                return connection.ExecuteAsync(parQuery, Parameters);
            else
                return connection.ExecuteAsync(parQuery, Parameters, transaction);
        }
        public  Task<int> ExecuteNonQueryAsync(string parQuery)
        {
            if (IsLock) ExceptionIsLock();
            if (TypeCommit == eTypeCommit.Auto)
                return connection.ExecuteAsync(parQuery);
            else
                return connection.ExecuteAsync(parQuery, null, transaction);
        }

        public  T1 ExecuteScalar<T1>(string query)
        {
            if (IsLock) ExceptionIsLock();
            return connection.ExecuteScalar<T1>(query);
        }

        public  T1 ExecuteScalar<T, T1>(string query, T parameters)
        {
            if (IsLock) ExceptionIsLock();
            return connection.ExecuteScalar<T1>(query, parameters);
        }

        public  Task<T1> ExecuteScalarAsync<T1>(string query)
        {
            if (IsLock) ExceptionIsLock();
            return connection.ExecuteScalarAsync<T1>(query);
        }

        public  Task<T1> ExecuteScalarAsync<T, T1>(string query, T parameters)
        {
            if (IsLock) ExceptionIsLock();
            return connection.ExecuteScalarAsync<T1>(query, parameters);
        }


        public int ExecuteNonQuery<T>(string parQuery, T Parameters, SqliteTransaction transaction)
        {
            if (IsLock) ExceptionIsLock();
            return connection.Execute(parQuery, Parameters, transaction);
        }

        public  int BulkExecuteNonQuery<T>(string parQuery, IEnumerable<T> Parameters)
        {
            int i = 0;
            if (IsLock) ExceptionIsLock();
            transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            // FileLogger.ExtLogForClass(transaction.GetType(), transaction.GetHashCode(), "Begin transaction");
            try
            {
                foreach (var el in Parameters)
                { 
                    ExecuteNonQuery(parQuery, el, transaction); 
                    i++; 
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                new Exception("BulkExecuteNonQuery =>" + ex.Message, ex);
            }
            
            //FileLogger.ExtLogForClass(transaction.GetType(), transaction.GetHashCode(), "End transaction");
            return i;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (connection != null)
                    {
                        connection.Close();
                        connection.Dispose();
                        connection = null;
                    }
                }
                disposedValue = true;
            }
            //           $"[{GetType()} -{ GetHashCode()}] Dispose connection".WriteLogMessage();
        }

        // // TODO:  finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SQLite()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        public virtual void ExceptionIsLock()
        {
            throw new Exception("SqlLite is Lock for FullUpdate");
        }
        public void SetLock(bool parIsLock)
        {
            IsLock = parIsLock;
        }
    }
}

