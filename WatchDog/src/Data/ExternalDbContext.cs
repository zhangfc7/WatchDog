using Dapper;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using WatchDog.src.Enums;
using WatchDog.src.Exceptions;
using WatchDog.src.Models;
using WatchDog.src.Utilities;

namespace WatchDog.src.Data
{
    internal static class ExternalDbContext
    {
        private static string _connectionString = WatchDogExternalDbConfig.ConnectionString;

        public static IDbConnection CreateSQLConnection()
            => WatchDogDatabaseDriverOption.DatabaseDriverOption switch
            {
                WatchDogDbDriverEnum.MSSQL => CreateMSSQLConnection(),
                WatchDogDbDriverEnum.MySql => CreateMySQLConnection(),
            };

        public static void Migrate() => BootstrapTables();

        public static void BootstrapTables()
        {
            var createWatchTablesQuery = GetSqlQueryString();

            using (var connection = CreateSQLConnection())
            {
                try
                {
                    connection.Open();
                    _ = connection.Query(createWatchTablesQuery);
                    connection.Close();
                }
                catch (SqlException ae)
                {
                    Debug.WriteLine(ae.Message.ToString());
                    throw ae;
                }
                catch (Exception ex)
                {
                    throw new WatchDogDatabaseException(ex.Message);
                }
            }

        }

        

        public static string GetSqlQueryString() =>
            WatchDogDatabaseDriverOption.DatabaseDriverOption switch
            {
                WatchDogDbDriverEnum.MSSQL => @$"
                                  IF OBJECT_ID('dbo.{Constants.WatchLogTableName}', 'U') IS NULL CREATE TABLE {Constants.WatchLogTableName} (
                                  id              INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                  responseBody    VARCHAR(max),
                                  responseStatus  int NOT NULL,
                                  requestBody     VARCHAR(max),
                                  queryString     VARCHAR(max),
                                  path            VARCHAR(max),
                                  requestHeaders  VARCHAR(max),
                                  responseHeaders VARCHAR(max),
                                  method          VARCHAR(30),
                                  host            VARCHAR(max),
                                  ipAddress       VARCHAR(30),
                                  timeSpent       VARCHAR(100),
                                  startTime       VARCHAR(100) NOT NULL,
                                  endTime         VARCHAR(100) NOT NULL
                            );
                                IF OBJECT_ID('dbo.{Constants.WatchLogExceptionTableName}', 'U') IS NULL CREATE TABLE {Constants.WatchLogExceptionTableName} (
                                id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                message       VARCHAR(max),
                                stackTrace    VARCHAR(max),
                                typeOf        VARCHAR(max),
                                source        VARCHAR(max),
                                path          VARCHAR(max),
                                method        VARCHAR(30),
                                queryString   VARCHAR(max),
                                requestBody   VARCHAR(max),
                                encounteredAt VARCHAR(100) NOT NULL
                             );
                                IF OBJECT_ID('dbo.{Constants.LogsTableName}', 'U') IS NULL CREATE TABLE {Constants.LogsTableName} (
                                id            INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                eventId       VARCHAR(100),
                                message       VARCHAR(max),
                                timestamp     VARCHAR(100) NOT NULL,
                                callingFrom   VARCHAR(100),
                                callingMethod VARCHAR(100),
                                lineNumber    INT,
                                logLevel      VARCHAR(30)
                             );
                        ",

                WatchDogDbDriverEnum.MySql => @$"
                             CREATE TABLE IF NOT EXISTS {Constants.WatchLogTableName} (
                              id              INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
                              responseBody    TEXT,
                              responseStatus  INT NOT NULL,
                              requestBody     TEXT,
                              queryString     TEXT,
                              path            TEXT,
                              requestHeaders  TEXT,
                              responseHeaders TEXT,
                              method          VARCHAR(30),
                              host            TEXT,
                              ipAddress       VARCHAR(30),
                              timeSpent       VARCHAR(100),
                              startTime       VARCHAR(100) NOT NULL,
                              endTime         VARCHAR(100) NOT NULL,
                              createUserId    VARCHAR(100),
                              createUserName  VARCHAR(100)
                            );
                           CREATE TABLE IF NOT EXISTS {Constants.WatchLogExceptionTableName} (
                                id            INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
                                message       TEXT,
                                stackTrace    TEXT,
                                typeOf        TEXT,
                                source        TEXT,
                                path          TEXT,
                                method        VARCHAR(30),
                                queryString   TEXT,
                                requestBody   TEXT,
                                encounteredAt VARCHAR(100) NOT NULL,
                                createUserId    VARCHAR(100),
                                createUserName  VARCHAR(100)
                             );
                           CREATE TABLE IF NOT EXISTS {Constants.LogsTableName} (
                                id            INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
                                eventId       VARCHAR(100),
                                message       TEXT,
                                timestamp     VARCHAR(100) NOT NULL,
                                callingFrom   VARCHAR(100),
                                callingMethod VARCHAR(100),
                                lineNumber    INT,
                                logLevel      VARCHAR(30),
                                createUserId  VARCHAR(100),
                                createUserName VARCHAR(100)
                             );
                        ",

                WatchDogDbDriverEnum.PostgreSql => @$"
                             CREATE TABLE IF NOT EXISTS {Constants.WatchLogTableName} (
                              id              SERIAL PRIMARY KEY,
                              responseBody    VARCHAR,
                              responseStatus  int NOT NULL,
                              requestBody     VARCHAR,
                              queryString     VARCHAR,
                              path            VARCHAR,
                              requestHeaders  VARCHAR,
                              responseHeaders VARCHAR,
                              method          VARCHAR(30),
                              host            VARCHAR,
                              ipAddress       VARCHAR(30),
                              timeSpent       VARCHAR,
                              startTime       TIMESTAMP with time zone NOT NULL,
                              endTime         TIMESTAMP with time zone NOT NULL
                            );
                           CREATE TABLE IF NOT EXISTS {Constants.WatchLogExceptionTableName} (
                                id            SERIAL PRIMARY KEY,
                                message       VARCHAR,
                                stackTrace    VARCHAR,
                                typeOf        VARCHAR,
                                source        VARCHAR,
                                path          VARCHAR,
                                method        VARCHAR(30),
                                queryString   VARCHAR,
                                requestBody   VARCHAR,
                                encounteredAt TIMESTAMP with time zone NOT NULL
                             );
                           CREATE TABLE IF NOT EXISTS {Constants.LogsTableName} (
                                id            SERIAL PRIMARY KEY,
                                eventId       VARCHAR(100),
                                message       VARCHAR,
                                timestamp     TIMESTAMP with time zone NOT NULL,
                                callingFrom   VARCHAR,
                                callingMethod VARCHAR(100),
                                lineNumber    INTEGER,
                                logLevel      VARCHAR(30)
                             );
                        ",
                _ => ""
            };

        

        public static MySqlConnection CreateMySQLConnection()
        {
            try
            {
                return new MySqlConnection(_connectionString);
            }
            catch (Exception ex)
            {
                throw new WatchDogDatabaseException(ex.Message);
            }
        }

        public static SqlConnection CreateMSSQLConnection()
        {
            try
            {
                return new SqlConnection(_connectionString);
            }
            catch (Exception ex)
            {
                throw new WatchDogDatabaseException(ex.Message);
            }
        }
    }
}
