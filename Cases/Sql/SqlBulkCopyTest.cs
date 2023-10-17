using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.SqlClient;

namespace Cases.Sql;

[TestClass]
public class SqlBulkCopyTest
{
	private string connectionString;
	private SqlConnection connection;
	private string tableName;

	[TestInitialize]
	public void Init()
	{
		connectionString = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true";
		connection = new SqlConnection(connectionString);
		tableName = "TestDb.dbo.TestTrans";
	}

	[TestMethod]
	public void InsertTable()
	{
		connection.Open();
		var transaction = connection.BeginTransaction();

		var level = transaction.IsolationLevel;
		Console.WriteLine(level);

		using (SqlBulkCopy bulkCopy = new SqlBulkCopy(
						connection, SqlBulkCopyOptions.Default,
						transaction))
		{
			bulkCopy.BatchSize = 10;
			bulkCopy.DestinationTableName = tableName;

			// Write from the source to the destination.
			// This should fail with a duplicate key error.
			try
			{
				bulkCopy.WriteToServer(CreateDataTable());
				transaction.Commit();
				Console.WriteLine("Transaction committed.");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				transaction.Rollback();
			}
		}
        connection.Close();
	}

	/// <summary>
	/// SqlBulkCopy在超时的时候会释放事务吗?
	/// SqlBulkCopy超时的时候: 
	/// - 内部事务会释放(如果有的话)
	/// - 数据库连接会释放(对外部事务的操作会抛异常)
	/// https://referencesource.microsoft.com/#System.Data/fx/src/data/System/Data/SqlClient/SqlBulkCopy.cs,981076e024513cec
	/// </summary>
	[TestMethod]
	public void TimeoutReleaseTransaction()
	{
		connection.Open();
		var transaction = connection.BeginTransaction();

		var level = transaction.IsolationLevel;
		Console.WriteLine(level);

		try
		{
			// 默认获取行锁
			using (SqlBulkCopy bulkCopy = new SqlBulkCopy(
						connection, SqlBulkCopyOptions.Default,
						transaction))
			{
                Console.WriteLine(bulkCopy.BatchSize);
                bulkCopy.BulkCopyTimeout = 1; // 5s
				bulkCopy.DestinationTableName = tableName;

				bulkCopy.WriteToServer(CreateDataTable());

				// 如果事务未提交时抛出异常
				transaction.Commit();
				// 如果事务提交后抛出异常
				Console.WriteLine("Transaction committed.");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			transaction.Rollback();
		}

		connection.Close();
	}

	/// <summary>
	/// 更建议上面InsertTable的写法.
	/// 
	/// 事务未提交时抛出异常会正常回滚.
	/// 事务提交之后又抛出了异常会导致transaction.Rollback失败:
	///   Test method Cases.Sql.SqlBulkCopyTest.TrySqlBulkCopyCatch threw exception: 
	///   System.InvalidOperationException: This SqlTransaction has completed; it is no longer usable.
	/// </summary>
	[TestMethod]
	public void TrySqlBulkCopyCatch()
	{
		connection.Open();
		var transaction = connection.BeginTransaction();

		var level = transaction.IsolationLevel;
		Console.WriteLine(level);

		try
		{
			using (SqlBulkCopy bulkCopy = new SqlBulkCopy(
						connection, SqlBulkCopyOptions.Default,
						transaction))
			{
				bulkCopy.DestinationTableName = tableName;

				bulkCopy.WriteToServer(CreateDataTable());
				// 如果事务未提交时抛出异常
				transaction.Commit();
				// 如果事务提交后抛出异常
				Console.WriteLine("Transaction committed.");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
			transaction.Rollback();
		}
		
		connection.Close();
	}

	[TestMethod]
	public void ReadAndInsert()
	{
		var dt = ReadTable();

		TrySqlBulkCopyCatch();

		dt = ReadTable();
    }

	[TestMethod]
	public DataTable ReadTable()
	{
		DataTable dt = new DataTable("");

		var sql = $"select * from {tableName}";

		try
		{
			connection.Open();

			SqlDataAdapter da = new SqlDataAdapter(sql, connection);
			da.SelectCommand.CommandTimeout = 60 * 10;

			da.Fill(dt);
		}
		catch (Exception ex)
		{
            Console.WriteLine(string.Format("[读数据库表失败] {0}", ex.Message));
			throw ex;
		}
		finally
		{
			connection.Close();
            Console.WriteLine(string.Format("[读数据库表结束] {0}行", dt.Rows.Count));
		}

		return dt;
	}

	private DataTable CreateDataTable()
	{
		// 创建 DataTable 对象
		DataTable dataTable = new DataTable("table1");

		// 添加列定义
		DataColumn column1 = new DataColumn("Column1", typeof(int));
		DataColumn column2 = new DataColumn("Column2", typeof(string));
		DataColumn column3 = new DataColumn("Column3", typeof(DateTime));
		dataTable.Columns.Add(column1);
		dataTable.Columns.Add(column2);
		dataTable.Columns.Add(column3);

		// 添加数据行
		DataRow row1 = dataTable.NewRow();
		row1["Column1"] = 1;
		row1["Column2"] = DateTime.Now.Millisecond;
		row1["Column3"] = DateTime.Now;
		dataTable.Rows.Add(row1);

		DataRow row2 = dataTable.NewRow();
		row2["Column1"] = 2;
		row2["Column2"] = DateTime.Now.Millisecond;
		row2["Column3"] = DateTime.Now;
		dataTable.Rows.Add(row2);

		return dataTable;
	}
}
