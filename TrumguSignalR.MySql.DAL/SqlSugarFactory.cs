using System;
using System.Configuration;
using System.Linq;
using SqlSugar;

namespace TrumguSignalR.MySql.DAL
{
    public class SqlSugarFactory
    {
        private static readonly string Conn = ConfigurationManager.AppSettings["ConStringMySQL"];
        public static SqlSugarClient GetInstance()
        {
            var db = new SqlSugarClient(new ConnectionConfig() { ConnectionString = Conn, DbType = DbType.MySql, IsAutoCloseConnection = true });
            db.Ado.IsEnableLogEvent = true;
            db.Ado.LogEventStarting = (sql, pars) =>
            {
                Console.WriteLine(sql + "\r\n" + db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                Console.WriteLine();
            };
            return db;
        }
    }
}
