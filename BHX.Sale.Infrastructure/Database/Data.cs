using BHX.Sale.Application.Core.Services;
using BHX.Sale.Domain.Enums;

using Microsoft.Extensions.Configuration;

using System.Configuration;

using static BHX.Sale.Domain.Enums.Globals;

public class Data
{
    #region Constants

    // Tên tham số Out mặc định trả về trong StoredProcedure (Oracle)
    protected const String DEFAULT_OUT_PARAMETER = "v_Out";
    // Kích thước vùng nhớ mặc định cho tham số Out trong StoredProcedure (Oracle)
    protected const int DEFAULT_OUT_PARAMETER_LENGTH = 4000;

    #endregion

    public static DATABASETYPE DataBaseType = DATABASETYPE.NONE;

    /// <summary>
    /// Nhận dạng loại CSDL từ chuỗi kết nối
    /// </summary>
    /// <param name="strConnect"></param>
    /// <returns></returns>
    public static Globals.DATABASETYPE RegconizeStringConnect(String strConnect)
    {
        if (strConnect == null)
        {
            strConnect = "";
        }

        String[] strOracle = { "Data Source", "User ID", "Password", "Unicode", "(Description", "LOAD_BALANCE", "ADDRESS_LIST", "SERVICE_NAME" };
        String[] strSQLSvr = { "Server", "DataBase", "UID", "Pwd", "Data Source", "User ID", "Password", "Initial Catalog" };
        String[] strMySQL = { "Server", "User ID", "Password", "DataBase" };
        String[] strMsAccess = { "Provider", "Microsoft", "Jet", "OLEDB", "Data Source" };
        String[] strPosgreSQL = { "Server", "Port", "User ID", "Password", "Database" };
        String[] strSQLite = { "Data Source", "Version", "Password" };

        //---------------------------------------------------------
        // Đếm sự có mặt của các từ khóa ORACLE trong chuỗi kết nối
        int intOraCount = 0;

        // Đếm tần suất xuất hiện của từ khóa Ora
        intOraCount += strConnect.ToUpper().Split(new String[] { "ORA" }, StringSplitOptions.None).Length;

        for (int i = 0; i < strOracle.Length; i++)
            if (strConnect.ToUpper().Contains(strOracle[i].ToUpper()))
                intOraCount++;

        //---------------------------------------------------------
        // Đếm sự có mặt của các từ khóa SQL SERVER trong chuỗi kết nối
        int intSqlCount = 0;

        // Đếm tần suất xuất hiện của từ khóa Sql
        intSqlCount += strConnect.ToUpper().Split(new String[] { "SQL" }, StringSplitOptions.None).Length;

        for (int i = 0; i < strSQLSvr.Length; i++)
            if (strConnect.ToUpper().Contains(strSQLSvr[i].ToUpper()))
                intSqlCount++;

        //---------------------------------------------------------
        // Đếm sự có mặt của các từ khóa MYSQL trong chuỗi kết nối
        int intMySqlCount = 0;

        for (int i = 0; i < strMySQL.Length; i++)
            if (strConnect.ToUpper().Contains(strMySQL[i].ToUpper()))
                intMySqlCount++;

        //---------------------------------------------------------
        // Đếm sự có mặt của các từ khóa POSGRESQL trong chuỗi kết nối
        int intPosgreSQLCount = 0;

        for (int i = 0; i < strPosgreSQL.Length; i++)
            if (strConnect.ToUpper().Contains(strPosgreSQL[i].ToUpper()))
                intPosgreSQLCount++;

        //---------------------------------------------------------
        // Đếm sự có mặt của các từ khóa MSACCESS trong chuỗi kết nối
        int intMsAccessCount = 0;

        for (int i = 0; i < strMsAccess.Length; i++)
            if (strConnect.ToUpper().Contains(strMsAccess[i].ToUpper()))
                intMsAccessCount++;

        //---------------------------------------------------------
        // Đếm sự có mặt của các từ khóa SQLite trong chuỗi kết nối
        int intSQLite = 0;

        for (int i = 0; i < strSQLite.Length; i++)
            if (strConnect.ToUpper().Contains(strSQLite[i].ToUpper()))
                intSQLite++;

        if (intSQLite == 3)
            return Globals.DATABASETYPE.SQLite;

        if (intPosgreSQLCount >= 5)
            return Globals.DATABASETYPE.PosgreSQL;

        if (intMySqlCount >= 4)
            return Globals.DATABASETYPE.MySQL;

        if (intMsAccessCount >= 5)
            return Globals.DATABASETYPE.MsAccess;

        // Trả về loại CSDL có nhiều từ khóa hơn
        return intOraCount >= intSqlCount ? Globals.DATABASETYPE.ORACLE : Globals.DATABASETYPE.SQLSERVER;
    }

    public static IData CreateData(String strConnect)
    {
        Data.DataBaseType = RegconizeStringConnect(strConnect);

        switch (Data.DataBaseType)
        {
            case DATABASETYPE.ORACLE:
                return new OracleData(strConnect);
            case DATABASETYPE.PosgreSQL:
                return new PosgreSQLData(strConnect);
            default:
                return null;
        }
    }
}
