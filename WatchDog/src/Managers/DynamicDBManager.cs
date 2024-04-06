using System;
using System.Threading.Tasks;
using WatchDog.src.Helpers;
using WatchDog.src.Models;

namespace WatchDog.src.Managers
{
    internal static class DynamicDBManager
    {
        internal enum TargetDbEnum
        {
            SqlDb = 0,
            LiteDb,
            MongoDb
        }
        private static string _connectionString = WatchDogExternalDbConfig.ConnectionString;

        //private static bool isExternalDb() => !string.IsNullOrEmpty(_connectionString);

        private static TargetDbEnum GetTargetDbEnum
        {
            get {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    return TargetDbEnum.LiteDb;
                }
                if (WatchDogDatabaseDriverOption.DatabaseDriverOption == Enums.WatchDogDbDriverEnum.Mongo)
                {
                    return TargetDbEnum.MongoDb;
                }
                return TargetDbEnum.SqlDb;
            }
        }

        public static async Task<bool> ClearLogs() =>
            GetTargetDbEnum switch
            {
                TargetDbEnum.SqlDb => await SQLDbHelper.ClearLogs(),
                _ => throw new NotImplementedException()
            };

        // WATCHLOG OPERATIONS
        public static async Task<Page<WatchLog>> GetAllWatchLogs(string searchString, string verbString, string statusCode, int pageNumber) =>
            GetTargetDbEnum switch
            {
                TargetDbEnum.SqlDb => await SQLDbHelper.GetAllWatchLogs(searchString, verbString, statusCode, pageNumber),
                _ => throw new NotImplementedException()
            };

        public static async Task InsertWatchLog(WatchLog log)
        {
            switch (GetTargetDbEnum)
            {
                case TargetDbEnum.SqlDb: 
                    await SQLDbHelper.InsertWatchLog(log);
                    break;
            }
        }

        // WATCH EXCEPTION OPERATIONS
        public static async Task<Page<WatchExceptionLog>> GetAllWatchExceptionLogs(string searchString, int pageNumber) =>
            GetTargetDbEnum switch
            {
                TargetDbEnum.SqlDb => await SQLDbHelper.GetAllWatchExceptionLogs(searchString, pageNumber),
                _ => throw new NotImplementedException()
            };

        public static async Task InsertWatchExceptionLog(WatchExceptionLog log)
        {
            switch (GetTargetDbEnum)
            {
                case TargetDbEnum.SqlDb:
                    await SQLDbHelper.InsertWatchExceptionLog(log);
                    break;
            }
        }

        // LOG OPERATIONS
        public static async Task<Page<WatchLoggerModel>> GetAllLogs(string searchString, string logLevelString, int pageNumber) =>
            GetTargetDbEnum switch
            {
                TargetDbEnum.SqlDb => await SQLDbHelper.GetAllLogs(searchString, logLevelString, pageNumber),
                _ => throw new NotImplementedException()
            };

        public static async Task InsertLog(WatchLoggerModel log)
        {
            switch (GetTargetDbEnum) {
                case TargetDbEnum.SqlDb: 
                    await SQLDbHelper.InsertLog(log);
                    break;
            }
        }
    }
}
