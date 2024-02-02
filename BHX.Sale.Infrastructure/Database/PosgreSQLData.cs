using BHX.Sale.Domain.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Npgsql;
using System.Collections;
using System.Data;

public class PosgreSQLData : Data, IData
{
    string _strConnect;
    #region Variables

    private NpgsqlConnection objConnection = null;
    private NpgsqlCommand objCommand = null;
    private NpgsqlTransaction objTransaction = null;
    private string strStoreName = "";
    private List<object> listParam = new List<object>();
    private int? timeOut;
    #endregion

    #region Properties

    IDbConnection IData.IConnection
    {
        get { return objConnection; }
        set { objConnection = (NpgsqlConnection)value; }
    }

    IDbTransaction IData.ITransaction
    {
        get { return objTransaction; }
        set { objTransaction = (NpgsqlTransaction)value; }
    }

    IDbCommand IData.ICommand
    {
        get { return objCommand; }
        set { objCommand = (NpgsqlCommand)value; }
    }

    #endregion

    #region Constructors

    public PosgreSQLData(String strConnect)
    {
        //this.strConnect = strConnect;
        _strConnect = strConnect;
    }

    ~PosgreSQLData()
    {
        // Nếu còn kết nối thì ngắt kết nối
        if (IsConnected())
            Disconnect();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Connect Functions

    public bool Connect()
    {
        if (IsConnected())
            return true;

        if (objConnection == null)
        {
            string strConnectLocal = _strConnect.Replace(";Unicode=True", string.Empty);
            objConnection = new NpgsqlConnection(strConnectLocal);
        }

        try
        {
            objConnection.Open();
            objConnection.EnlistTransaction(System.Transactions.Transaction.Current);
        }
        catch (Exception objEx)
        {
            // ORA-02396: exceeded maximum idle time, please connect again
            // Nếu gặp lỗi này thì Reconnect lại 1 lần, nếu lỗi nữa thì through Exception

            if (objEx.Message.Contains("ORA-02396"))
            {
                objConnection.Open();
                return true;
            }

            throw objEx;
        }

        return true;
    }

    public bool Disconnect()
    {
        try
        {
            if (this.objCommand != null)
                this.objCommand.Dispose();

            if (IsConnected())
                objConnection.Close();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool IsConnected()
    {
        if (objConnection == null || objConnection.State != ConnectionState.Open)
            return false;
        return true;
    }

    #endregion

    #region Private functions

    /// <summary>
    /// Contructing IDbCommand follow DataBase
    /// </summary>
    /// <param name="strSQL"></param>
    /// <returns></returns>
    private NpgsqlCommand SetCommand(String strSQL)
    {
        objCommand = new NpgsqlCommand(strSQL, objConnection);
        if (objTransaction != null)
            objCommand.Transaction = objTransaction;
        return objCommand;
    }
    #endregion

    #region Transaction Functions

    public void BeginTransaction()
    {
        if (!IsConnected())
            Connect();

        this.objTransaction = objConnection.BeginTransaction();
    }

    public void CommitTransaction()
    {
        if (objTransaction != null)
            objTransaction.Commit();
    }

    public void RollBackTransaction()
    {
        if (objTransaction != null)
        {
            objTransaction.Rollback();
            objTransaction = null;
        }
    }

    #endregion

    #region Execute Text Queries
    public void ExecUpdate(String strSQL)
    {
        SetCommand(strSQL).ExecuteNonQuery();
    }

    #endregion

    #region Execute Stored Procedures

    public void CreateNewSqlText(String strSQL)
    {
        objCommand = SetCommand(strSQL);
        objCommand.CommandType = System.Data.CommandType.Text;
    }

    public void CreateNewStoredProcedure(String strStoreProName)
    {
        strStoreName = strStoreProName;
        listParam = new List<object>();
        timeOut = null;
    }

    public void CreateNewStoredProcedure(String strStoreProName, int intTimeOut)
    {
        strStoreName = strStoreProName;
        listParam = new List<object>();
        timeOut = intTimeOut;
    }

    public void AddParameter(String strParameterName, object objValue)
    {
        listParam.Add(objValue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="strParameterName"></param>
    /// <param name="objValue"></param>
    /// <param name="enDataType"></param>
    public void AddParameter(String strParameterName, object objValue, Globals.DATATYPE enDataType)
    {
        AddParameter(strParameterName, objValue);
    }

    /// <summary>
    /// Thêm 1 mảng các paramenter
    /// Dạng {"@paramName1", objValue1, "@paramName2", objValue2, ...}
    /// </summary>
    /// <param name="objArrParam"></param>
    public void AddParameter(params object[] objArrParam)
    {
        bool bolIsHasDATATYPE = false;
        if (objArrParam.Length > 3)
        {
            if (objArrParam[2].GetType().Name == "DATATYPE")
                bolIsHasDATATYPE = true;
        }

        if (bolIsHasDATATYPE)
            for (int i = 0; i < objArrParam.Length; i += 3)
                AddParameter(objArrParam[i].ToString().Trim(), objArrParam[i + 1].ToString().Trim(), (Globals.DATATYPE)objArrParam[i + 2]);
        else
            for (int i = 0; i < objArrParam.Length; i += 2)
                AddParameter(objArrParam[i].ToString().Trim(), objArrParam[i + 1]);

    }

    public void AddParameter(Hashtable hstParameter)
    {
        IDictionaryEnumerator objDicEn = hstParameter.GetEnumerator();

        while (objDicEn.MoveNext())
        {
            AddParameter(objDicEn.Key.ToString(), objDicEn.Value);
        }
    }

    public IDataReader ExecStoreToDataReader()
    {
        return ExecStoreToDataReader("");
    }

    private NpgsqlCommand CreateCommand()
    {
        string param = string.Join(',', listParam.Select((p, i) => $"${i + 1}"));

        var cmd = new NpgsqlCommand($"SELECT {strStoreName}({param})", objConnection, objTransaction);
        if (timeOut != null)
        {
            cmd.CommandTimeout = timeOut.Value;
        }
        if (listParam != null)
        {
            foreach (var parameter in listParam)
            {
                if (parameter == null)
                    cmd.Parameters.AddWithValue(DBNull.Value);
                else
                    cmd.Parameters.AddWithValue(parameter);
            }
        }
        return cmd;
    }

    public IDataReader ExecStoreToDataReader(String strOutParameter)
    {
        this.objCommand = CreateCommand();
        IDataReader dataReader = (IDataReader)null;

        var cursor = this.objCommand.ExecuteScalar();
        string sql = $"FETCH ALL IN \"{cursor}\"";

        using (var cmd = new NpgsqlCommand(sql, objConnection, objTransaction))
        {
            using (var reader = cmd.ExecuteReader())
            {
                dataReader = reader;
            }
        }
        return dataReader;
    }

    public Hashtable ExecStoreToHashtable()
    {
        return ExecStoreToHashtable("");
    }

    public Hashtable ExecStoreToHashtable(String strOutParameter)
    {
        this.objCommand = CreateCommand();

        Hashtable hstbItem = new Hashtable();

        NpgsqlDataReader dr = objCommand.ExecuteReader();

        while (dr.Read())
            for (int i = 0; i < dr.FieldCount; i++)
                hstbItem.Add(dr.GetName(i), dr[i]);

        return hstbItem;
    }

    public String ExecStoreToString()
    {
        return ExecStoreToString("");
    }

    public String ExecStoreToString(String strOutParameter)
    {
        this.objCommand = CreateCommand();

        Object objTemp = objCommand.ExecuteScalar();
        if (objTemp == null)
            return "";
        return objTemp.ToString().Trim();
    }

    public int ExecNonQuery()
    {
        this.objCommand = CreateCommand();
        return objCommand.ExecuteNonQuery();
    }

    public DataTable ExecStoreToDataTable()
    {
        return ExecStoreToDataTable("");
    }

    public DataTable ExecStoreToDataTable(String strOutParameter)
    {
        this.objCommand = CreateCommand();
        DataTable dataTable = new DataTable(this.strStoreName);

        var cursor = this.objCommand.ExecuteScalar();
        string sql = $"FETCH ALL IN \"{cursor}\"";

        using (var cmd = new NpgsqlCommand(sql, objConnection, objTransaction))
        {
            using (var reader = cmd.ExecuteReader())
            {
                dataTable.Load(reader);
            }
        }

        return dataTable;
    }

    public DataSet ExecStoreToDataSet()
    {
        return ExecStoreToDataSet("");
    }

    public DataSet ExecStoreToDataSet(params String[] strOutParameter)
    {
        this.objCommand = CreateCommand();
        DataSet dsResult = new DataSet();

        int intNumTable = 0;

        var cursor = this.objCommand.ExecuteScalar();
        string sql = $"FETCH ALL IN \"{cursor}\"";

        using (var cmd = new NpgsqlCommand(sql, objConnection, objTransaction))
        {
            using (var reader1 = cmd.ExecuteReader())
            {
                dsResult.Tables.Add(new DataTable());
                dsResult.Tables[intNumTable].Load(reader1);
            }
            //Execute cmd and process the results as normal
            intNumTable++;
        }

        return dsResult;
    }

    public void CreateNewBuckCopy(string strStoreName, DataTable table)
    {
        throw new NotImplementedException();
    }
    public List<object> ExecStoreToListObject()
    {
        throw new NotImplementedException();
    }

    public List<object> ExecStoreToListObject(String strOutParameter)
    {
        throw new NotImplementedException();
    }

    #endregion
}
