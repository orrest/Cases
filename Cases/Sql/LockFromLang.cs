using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Data.SqlClient;

namespace Cases.Sql;

[TestClass]
public class LockFromLang
{
	private string connectionString;
	private string tableName;

	[TestInitialize]
	public void Init()
	{
		connectionString = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true;Pooling=true";
		tableName = "TestDb.dbo.TestTrans";
	}

	private object lockobj = new object();
	[TestMethod]
	public void LockConcurrentInsert()
	{
		var th1 = new Thread(() =>
		{
			lock (lockobj)
			{
				Console.WriteLine(DateTime.Now.Second);
				Thread.Sleep(4000);
				InsertTable();
			}
		});

		var th2 = new Thread(() =>
		{
			lock(lockobj)
			{
				Console.WriteLine(DateTime.Now.Second);
				Thread.Sleep(4000);
				InsertTable();
			}
		});

		th1.Start();
		th2.Start();
		th1.Join();
		th2.Join();
	}

	/// <summary>
	/// 不要在多线程上共享数据库连接!
	/// </summary>
	[TestMethod]
    public void ConcurrentInsert()
    {
		var th1 = new Thread(() =>
		{
			Console.WriteLine(DateTime.Now);
			InsertTable();
		});

		var th2 = new Thread(() =>
		{
			Console.WriteLine(DateTime.Now);
			InsertTable();
		});

		th1.Start();
		th2.Start();
		th1.Join();
		th2.Join();
    }

	/// <summary>
	/// 不要在多线程上共享数据库连接!
	/// https://stackoverflow.com/questions/27425362/using-the-same-open-sql-connection-from-different-threads
	/// </summary>
	[TestMethod]
	public void InsertTable()
	{
		var connection = new SqlConnection(connectionString);
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
