using BHX.Sale.Domain.Enums;

using Microsoft.Extensions.Configuration;

using Oracle.ManagedDataAccess.Client;

using System.Collections;
using System.Data;

public class OracleData : Data, IData
{
    string _strConnect;

    #region Variables

    private OracleConnection objConnection = null;
    private OracleCommand objCommand = null;
    private OracleTransaction objTransaction = null;
    private string strTableName = "TableNameDefault";
    #endregion

    #region Properties

    IDbConnection IData.IConnection
    {
        get { return objConnection; }
        set { objConnection = (OracleConnection)value; }
    }

    IDbTransaction IData.ITransaction
    {
        get { return objTransaction; }
        set { objTransaction = (OracleTransaction)value; }
    }

    IDbCommand IData.ICommand
    {
        get { return objCommand; }
        set { objCommand = (OracleCommand)value; }
    }

    #endregion

    #region Constructors

    private readonly IConfiguration _config;

    public OracleData(IConfiguration config)
    {
        _config = config;
    }
    public OracleData(String strConnect)
    {
        _strConnect = strConnect;
    }
    ~OracleData()
    {
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
            objConnection = new OracleConnection(strConnectLocal);
        }

        try
        {
            objConnection.Open();
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
        if (objConnection == null || objConnection.State != System.Data.ConnectionState.Open)
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
    private OracleCommand SetCommand(String strSQL)
    {
        objCommand = new OracleCommand(strSQL, objConnection);
        objCommand.BindByName = true;
        objCommand.FetchSize = objCommand.FetchSize * 32;
        if (objTransaction != null)
            objCommand.Transaction = objTransaction;
        return objCommand;
    }

    /// <summary>
    /// Contructing IDataAdapter follow DataBase
    /// </summary>
    /// <param name="strSQL"></param>
    /// <returns></returns>
    private OracleDataAdapter SetDataAdapter(String strSQL)
    {
        return new OracleDataAdapter(strSQL, objConnection);
    }

    /// <summary>
    /// Contructing IDataAdapter follow DataBase
    /// </summary>
    /// <param name="iCmd"></param>
    /// <returns></returns>
    private OracleDataAdapter SetDataAdapter(OracleCommand objCommand)
    {
        return new OracleDataAdapter(objCommand);
    }

    /// <summary>
    /// Convert Data Type to OracleDataType
    /// </summary>
    /// <param name="enDataType"></param>
    /// <returns></returns>
    private OracleDbType GetOracleDataType(Globals.DATATYPE enDataType)
    {
        OracleDbType enResult = OracleDbType.Int32;
        switch (enDataType)
        {
            case Globals.DATATYPE.NUMBER:
                enResult = OracleDbType.Int32;
                break;
            case Globals.DATATYPE.CHAR:
                enResult = OracleDbType.Char;
                break;
            case Globals.DATATYPE.VARCHAR:
                enResult = OracleDbType.Varchar2;
                break;
            case Globals.DATATYPE.NVARCHAR:
                enResult = OracleDbType.NVarchar2;
                break;
            case Globals.DATATYPE.NTEXT:
                enResult = OracleDbType.NClob;
                break;
            case Globals.DATATYPE.BINARY:
                enResult = OracleDbType.BFile;
                break;
            case Globals.DATATYPE.BLOB:
                enResult = OracleDbType.Blob;
                break;
            case Globals.DATATYPE.CLOB:
                enResult = OracleDbType.Clob;
                break;
            case Globals.DATATYPE.NCLOB:
                enResult = OracleDbType.NClob;
                break;
            default:
                break;
        }

        return enResult;
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
        //FileLogger.LogAction(strLogFileGuidID, "[CommitTransaction]" + "\n" +
        //            new System.Diagnostics.StackTrace().ToString());
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
        strTableName = strStoreProName;
        objCommand = SetCommand(strStoreProName);
        objCommand.CommandType = System.Data.CommandType.StoredProcedure;
    }

    public void CreateNewStoredProcedure(String strStoreProName, int intTimeOut)
    {
        strTableName = strStoreProName;
        objCommand = SetCommand(strStoreProName);
        objCommand.CommandTimeout = intTimeOut;
        objCommand.CommandType = System.Data.CommandType.StoredProcedure;
    }

    public void AddParameter(String strParameterName, object objValue)
    {
        if (objValue != null && objValue.ToString().Equals("True", StringComparison.OrdinalIgnoreCase))
        {
            objValue = 1;
        }
        else if (objValue != null && objValue.ToString().Equals("False", StringComparison.OrdinalIgnoreCase))
        {
            objValue = 0;
        }
        objCommand.Parameters.Add(strParameterName.Replace("@", "v_"), objValue);
    }

    public void AddParameter(String strParameterName, object objValue, Globals.DATATYPE enDataType)
    {
        OracleParameter objPara = new OracleParameter(strParameterName.Replace("@", "v_"), GetOracleDataType(enDataType));
        objPara.Value = objValue;
        objCommand.Parameters.Add(objPara);
    }

    public void AddParameter(params object[] objArrParam)
    {
        for (int i = 0; i < objArrParam.Length / 2; i++)
        {
            //Lợi sửa thêm check productidlist (mục đích truyền object từ UI có chứa productidlist
            if (objArrParam[i * 2].ToString().ToLower() == "@productidlist" && objArrParam[i * 2 + 1] != null && objArrParam[i * 2 + 1].ToString().Trim() != string.Empty)
                AddParameter(objArrParam[i * 2].ToString().Trim(), objArrParam[i * 2 + 1], Globals.DATATYPE.NCLOB);
            else
                AddParameter(objArrParam[i * 2].ToString().Trim(), objArrParam[i * 2 + 1]);
        }
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

    public IDataReader ExecStoreToDataReader(String strOutParameter)
    {
        if (strOutParameter.Trim().Length == 0)
            strOutParameter = DEFAULT_OUT_PARAMETER;

        objCommand.Parameters.Add(strOutParameter, OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        return objCommand.ExecuteReader();
    }

    public Hashtable ExecStoreToHashtable()
    {
        return ExecStoreToHashtable("");
    }

    public Hashtable ExecStoreToHashtable(String strOutParameter)
    {
        if (strOutParameter.Trim().Length == 0)
            strOutParameter = DEFAULT_OUT_PARAMETER;

        objCommand.Parameters.Add(strOutParameter, OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        return ConvertHashTable(objCommand.ExecuteReader());
    }
    /// <summary>
    /// Convert IDataReader to HashTable
    /// </summary>
    /// <param name="drReader"></param>
    /// <returns></returns>
    public static Hashtable ConvertHashTable(IDataReader drReader)
    {
        Hashtable hstbItem = new Hashtable(); //Biến lưu giá trị trả về

        if (drReader.Read())
            for (int i = 0; i < drReader.FieldCount; i++)
                if (!hstbItem.Contains(drReader.GetName(i)))
                    if (drReader[i] == null || drReader.IsDBNull(i))
                        hstbItem.Add(drReader.GetName(i).ToUpper(), "");
                    else
                        hstbItem.Add(drReader.GetName(i), drReader[i]);

        return hstbItem;
    }

    public String ExecStoreToString()
    {
        return ExecStoreToString("");
    }

    public String ExecStoreToString(String strOutParameter)
    {
        if (strOutParameter.Length == 0)
            strOutParameter = DEFAULT_OUT_PARAMETER;

        try
        {
            objCommand.Parameters.Add(strOutParameter, OracleDbType.NVarchar2, DEFAULT_OUT_PARAMETER_LENGTH).Direction = ParameterDirection.Output;
            objCommand.ExecuteScalar();
            Object objTemp = objCommand.Parameters[strOutParameter].Value;

            if (Convert.IsDBNull(objTemp) || objTemp.ToString().Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            return objTemp.ToString().Trim();
        }
        catch (Exception ex)
        {
            #region Kiểm tra lỗi maximum idle time, và kô thuộc transaction nào
            if (this.objTransaction == null && ex.Message.ToString().Contains("02396") && ex.Message.ToString().Contains("ORA"))
            {
                try
                {
                    this.Connect();
                    objCommand.ExecuteScalar();
                    Object objTemp = objCommand.Parameters[strOutParameter].Value;

                    if (Convert.IsDBNull(objTemp) || objTemp.ToString().Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
                        return string.Empty;

                    return objTemp.ToString().Trim();
                }
                catch (Exception exn) { throw exn; }
            }
            #endregion

            ProcessException(ex);
            throw ex;
        }

    }

    //public byte[] ExecStoreToBinary()
    //{
    //    return ExecStoreToBinary("");
    //}

    //public byte[] ExecStoreToBinary(String strOutParameter)
    //{
    //    if (strOutParameter.Length == 0)
    //        strOutParameter = DEFAULT_OUT_PARAMETER;

    //    objCommand.Parameters.Add(strOutParameter, OracleDbType.BFile, 2000).Direction = ParameterDirection.Output;

    //    return (byte[])objCommand.ExecuteScalar();
    //}

    public int ExecNonQuery()
    {
        return objCommand.ExecuteNonQuery();
    }

    //public IDataAdapter ExecStoreToDataAdapter()
    //{
    //    return ExecStoreToDataAdapter("");
    //}

    //public IDataAdapter ExecStoreToDataAdapter(String strOutParameter)
    //{
    //    if (strOutParameter.Trim().Length == 0)
    //        strOutParameter = DEFAULT_OUT_PARAMETER;

    //    objCommand.Parameters.Add(strOutParameter, OracleDbType.RefCursor).Direction = ParameterDirection.Output;
    //    return SetDataAdapter(objCommand);
    //}

    public DataTable ExecStoreToDataTable()
    {
        return ExecStoreToDataTable("");
    }

    public DataTable ExecStoreToDataTable(String strOutParameter)
    {
        try
        {
            if (strOutParameter.Trim().Length == 0)
                strOutParameter = DEFAULT_OUT_PARAMETER;

            objCommand.Parameters.Add(strOutParameter, OracleDbType.RefCursor).Direction = ParameterDirection.Output;
            DataTable dtResult = new DataTable(strTableName);
            SetDataAdapter(objCommand).Fill(dtResult);
            return dtResult;
        }
        catch (Exception ex)
        {
            #region Kiểm tra lỗi maximum idle time
            if (this.objTransaction == null && ex.Message.ToString().Contains("02396") && ex.Message.ToString().Contains("ORA"))
            {
                try
                {
                    this.Connect();
                    DataTable dtResult = new DataTable(strTableName);
                    SetDataAdapter(objCommand).Fill(dtResult);
                    return dtResult;
                }
                catch { }
            }
            #endregion

            //Process RO Errors
            ProcessException(ex);
            throw ex;
        }
    }

    public List<object> ExecStoreToListObject()
    {
        return ExecStoreToListObject("");
    }

    public List<object> ExecStoreToListObject(String strOutParameter)
    {
        List<object> lstDataObjects = new List<object>();
        if (strOutParameter.Trim().Length == 0)
            strOutParameter = DEFAULT_OUT_PARAMETER;

        objCommand.Parameters.Add(strOutParameter, OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        IDataReader reader = null;
        try
        {
            reader = objCommand.ExecuteReader();
            while (reader.Read())
            {
                Dictionary<object, object> dicData = new Dictionary<object, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (!dicData.ContainsKey(reader.GetName(i)))
                        dicData.Add(reader.GetName(i), reader[i]);
                }
                lstDataObjects.Add(dicData);
            }
        }
        catch (Exception objEx)
        {
            lstDataObjects = new List<object>();
            throw (objEx);
        }
        finally
        {
            if (reader != null)
                reader.Close();
        }

        return lstDataObjects;
    }

    private void ProcessException(Exception ex)
    {
        //Nếu sử dụng RO mà chưa compile được thì gọi compile Store
        if (this.objTransaction == null && ex.Message.ToString().Contains("16000") && ex.Message.ToString().Contains("ORA"))
        {
            IData objDataNew = Data.CreateData(_strConnect.Replace("RO", "RW"));
            try
            {
                objDataNew.Connect();
                objDataNew.ExecUpdate("ALTER PROCEDURE " + this.objCommand.CommandText + " COMPILE");

            }
            catch { }
            finally { objDataNew.Disconnect(); }
        }
    }

    public DataSet ExecStoreToDataSet()
    {
        return ExecStoreToDataSet("");
    }

    public DataSet ExecStoreToDataSet(params String[] strOutParameter)
    {
        for (int i = 0; i < strOutParameter.Length; i++)
            if (strOutParameter[i].Trim().Length == 0)
                strOutParameter[i] = DEFAULT_OUT_PARAMETER;

        for (int i = 0; i < strOutParameter.Length; i++)
            objCommand.Parameters.Add(strOutParameter[i], OracleDbType.RefCursor).Direction = ParameterDirection.Output;

        DataSet dsResult = new DataSet();
        SetDataAdapter(objCommand).Fill(dsResult);
        return dsResult;
    }
    public void CreateNewBuckCopy(string strTableName, DataTable table)
    {
        throw new NotImplementedException();
    }

    public List<T> ExecFuncToList<T>(string functionName, List<object> parameters, int storeTimeout = 90) where T : new()
    {
        throw new NotImplementedException();
    }

    #endregion

}
