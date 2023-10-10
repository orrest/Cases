using System.Data.SqlClient;

namespace Cases.Sql;

[TestClass]
public class SharedSqlConnectionClose
{
	[TestMethod]
	public void wrong_table_name_exception_thrown()
	{
		var connectionString = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true";
		using (SqlConnection connection = new SqlConnection(connectionString))
		{
			connection.Open();

			SqlTransaction sqlTran = connection.BeginTransaction();
			SqlCommand command = connection.CreateCommand();
			command.Transaction = sqlTran;

			try
			{
				command.CommandText =
				  "INSERT INTO Production.ScrapReason(Name) VALUES('Wrong size')";
				command.ExecuteNonQuery();
				command.CommandText =
				  "INSERT INTO Production.ScrapReason(Name) VALUES('Wrong color')";
				command.ExecuteNonQuery();

				sqlTran.Commit();
				Console.WriteLine("Both records were written to database.");
			}
			// Wrong table name!
			catch (SqlException ex)
			{
				var i = 0;
				try
				{
					i = 1;
					// Attempt to roll back the transaction.
					sqlTran.Rollback();
				}
				catch (Exception exRollback)
				{
					i = 2;
					// Throws an InvalidOperationException if the connection
					// is closed or the transaction has already been rolled
					// back on the server.
					Console.WriteLine(exRollback.Message);
				}
				// Successfully rollback!
				Assert.AreEqual(1, i);
			}
		}
	}

	/// <summary>
	/// Throw exception: This SqlTransaction has completed; it is no longer usable.
	/// </summary>
	[TestMethod]
	public void close_before_manual_rollback()
	{
		var connectionString = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true";
		using (SqlConnection connection = new SqlConnection(connectionString))
		{
			connection.Open();

			SqlTransaction sqlTran = connection.BeginTransaction();
			SqlCommand command = connection.CreateCommand();
			command.Transaction = sqlTran;

			try
			{
				command.CommandText =
				  "INSERT INTO Production.ScrapReason(Name) VALUES('Wrong size')";
				command.ExecuteNonQuery();

				sqlTran.Commit();
			}
			catch (SqlException ex)
			{
				var i = 0;
				try
				{
					i = 1;
					// Close before manual rollback!
					connection.Close();
					// The rollback will fail!
					sqlTran.Rollback();
				}
				catch (Exception exRollback)
				{
					Assert.AreEqual("This SqlTransaction has completed; it is no longer usable.", exRollback.Message);
					i = 2;
					// Throws an InvalidOperationException if the connection
					// is closed or the transaction has already been rolled
					// back on the server.
					Console.WriteLine(exRollback.Message);
				}
				Assert.AreEqual(2, i);
			}
		}
	}

	/// <summary>
	/// No exception thrown.
	/// </summary>
	[TestMethod]
	public void close_inorder_and_manual_rollback()
	{
		var connectionString = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true";
		using (SqlConnection connection = new SqlConnection(connectionString))
		{
			// Close before manual rollback!
			connection.Close();

			connection.Open();

			SqlTransaction sqlTran = connection.BeginTransaction();
			SqlCommand command = connection.CreateCommand();
			command.Transaction = sqlTran;

			try
			{
				command.CommandText =
				  "INSERT INTO Production.ScrapReason(Name) VALUES('Wrong size')";
				command.ExecuteNonQuery();

				sqlTran.Commit();
			}
			catch (SqlException ex)
			{
				var i = 0;
				try
				{
					i = 1;
					sqlTran.Rollback();
				}
				catch (Exception exRollback)
				{
					Assert.AreEqual("This SqlTransaction has completed; it is no longer usable.", exRollback.Message);
					i = 2;
					// Throws an InvalidOperationException if the connection
					// is closed or the transaction has already been rolled
					// back on the server.
					Console.WriteLine(exRollback.Message);
				}
				Assert.AreEqual(1, i);
			}
		}
	}

	/// <summary>
	/// No exception thrown.
	/// </summary>
	[TestMethod]
	public void commit_twice()
	{
		var connectionString = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true";
		using (SqlConnection connection = new SqlConnection(connectionString))
		{
			// Close before manual rollback!
			connection.Close();

			connection.Open();

			SqlTransaction sqlTran = connection.BeginTransaction();
			SqlCommand command = connection.CreateCommand();
			command.Transaction = sqlTran;

			try
			{
				command.CommandText =
				  "INSERT INTO Production.ScrapReason(Name) VALUES('Wrong size')";
				command.ExecuteNonQuery();

				sqlTran.Commit();
				sqlTran.Commit();
			}
			catch (SqlException ex)
			{
				var i = 0;
				try
				{
					i = 1;
					sqlTran.Rollback();
				}
				catch (Exception exRollback)
				{
					Assert.AreEqual("This SqlTransaction has completed; it is no longer usable.", exRollback.Message);
					i = 2;
					// Throws an InvalidOperationException if the connection
					// is closed or the transaction has already been rolled
					// back on the server.
					Console.WriteLine(exRollback.Message);
				}
				Assert.AreEqual(1, i);
			}
		}
	}

	/// <summary>
	/// Exception: This SqlTransaction has completed; it is no longer usable.
	/// </summary>
	[TestMethod]
	public void rollback_twice()
	{
		var connectionString = "Server=(localdb)\\MSSQLLocalDB;Integrated Security=true";
		using (SqlConnection connection = new SqlConnection(connectionString))
		{
			// Close before manual rollback!
			connection.Close();

			connection.Open();

			SqlTransaction sqlTran = connection.BeginTransaction();
			SqlCommand command = connection.CreateCommand();
			command.Transaction = sqlTran;

			try
			{
				command.CommandText =
				  "INSERT INTO Production.ScrapReason(Name) VALUES('Wrong size')";
				command.ExecuteNonQuery();

				sqlTran.Commit();
			}
			catch (SqlException ex)
			{
				var i = 0;
				try
				{
					i = 1;
					sqlTran.Rollback();
					sqlTran.Rollback();
				}
				catch (Exception exRollback)
				{
					Assert.AreEqual("This SqlTransaction has completed; it is no longer usable.", exRollback.Message);
					i = 2;
					// Throws an InvalidOperationException if the connection
					// is closed or the transaction has already been rolled
					// back on the server.
					Console.WriteLine(exRollback.Message);
				}
				Assert.AreEqual(2, i);
			}
		}
	}
}
