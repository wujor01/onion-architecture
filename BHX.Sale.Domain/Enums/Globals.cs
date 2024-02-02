using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BHX.Sale.Domain.Enums
{
    public class Globals
    {
        public enum DATATYPE
        {
            NUMBER,
            CHAR,
            VARCHAR,
            NVARCHAR,
            NTEXT,
            BINARY,
            BLOB,
            CLOB,
            NCLOB,
            SMALLINT,
            TIMESTAMP,
            BOOLEAN,
            BIGINT,
            INTEGER,
            TEXT,
            NUMERIC,
            DATE,
            DATETIME,
            REFCURSOR,
            BIT,
            TIME,
            DOUBLE,
            SINGLE,
            REAL
        }

        public enum DATABASETYPE
        {
            NONE = 0,
            SQLSERVER = 1,
            ORACLE = 2,
            MySQL = 3,
            MsAccess = 4,
            PosgreSQL = 5,
            SQLite = 6
        }
    }
}
