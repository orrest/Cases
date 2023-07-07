namespace Cases.lang.tasks;

/// <summary>
/// 1. Run task on threadpool thread
/// 2. Run task with ContinueWith to handle aggregation exceptions
/// 3. Run task with await and handle exceptions in outer code by catch the unwrapped exception
/// 4. It's better to use await when you need the return value.
/// </summary>
[TestClass]
public class RunTaskOnOtherThreadTest
{
    /// <summary>
    /// Run task on threadpool thread.
    /// </summary>
    [TestMethod]
    public void run_task_with_continue()
    {
        var task = new Task(() => {
            Console.WriteLine($"Task run on {Thread.CurrentThread.ManagedThreadId}");
        });

        Console.WriteLine($"run_task_with_continue method run on {Thread.CurrentThread.ManagedThreadId}");

        task.ContinueWith(task =>
        {
            Console.WriteLine($"Task {task.Id} continue on {Thread.CurrentThread.ManagedThreadId}.");
            Console.WriteLine($"Task {task.Id} completed successfully.");
        }, TaskContinuationOptions.OnlyOnRanToCompletion);

        task.ContinueWith(task =>
        {
            Console.WriteLine($"Task {task.Id} failed on {Thread.CurrentThread.ManagedThreadId}.");
            Console.WriteLine($"Task {task.Id}: {task.Exception?.Message}.");
        }, TaskContinuationOptions.OnlyOnFaulted);

        task.Start();

        Thread.Sleep(TimeSpan.FromSeconds(5));
     
        Console.WriteLine($"run_task_with_continue method run on {Thread.CurrentThread.ManagedThreadId} return.");
    }

    /// <summary>
    /// Run tasks on threadpool threads.
    /// Tackle aggregation exceptions in task.ContinueWith, TaskContinuationOptions.OnlyOnFaulted.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    [TestMethod]
    public void run_task_with_continue_throw()
    {
        var task = new Task(() => {
            Console.WriteLine($"Task run on {Thread.CurrentThread.ManagedThreadId}");
            throw new InvalidOperationException("Intend throw");
        });

        Console.WriteLine($"run_task_with_continue method run on {Thread.CurrentThread.ManagedThreadId}");

        task.ContinueWith(task =>
        {
            Console.WriteLine($"Task {task.Id} continue on {Thread.CurrentThread.ManagedThreadId}.");
            Console.WriteLine($"Task {task.Id} completed successfully.");
        }, TaskContinuationOptions.OnlyOnRanToCompletion);

        task.ContinueWith(task =>
        {
            Console.WriteLine($"Task {task.Id} failed on {Thread.CurrentThread.ManagedThreadId}.");
            Console.WriteLine($"Task {task.Id}: {task.Exception?.Message}.");
            task.Exception?.Handle(ex =>
            {
                Console.WriteLine($"Exceptions in Aggregation: ");

                Assert.IsInstanceOfType(ex, typeof(AggregateException));

                switch (ex)
                {
                    case InvalidOperationException e: Console.WriteLine($"{e.Message}"); return true;
                }
                return false;
            });
        }, TaskContinuationOptions.OnlyOnFaulted);

        task.Start();

        Thread.Sleep(TimeSpan.FromSeconds(5));

        Console.WriteLine($"run_task_with_continue method run on {Thread.CurrentThread.ManagedThreadId} return.");
    }

    /// <summary>
    /// Run tasks on threadpool threads.
    /// Catch the first exception from await.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="AccessViolationException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    [TestMethod]
    public async Task await_task_whenall_exceptions()
    {
        Console.WriteLine($"Method run on {Thread.CurrentThread.ManagedThreadId}...");

        Task task1 = Task.Run(() => {
            Console.WriteLine($"Task 1 run on {Thread.CurrentThread.ManagedThreadId}...");
            throw new AccessViolationException("Exception 1");
        });
        Task task2 = Task.Run(() => {
            Console.WriteLine($"Task 2 run on {Thread.CurrentThread.ManagedThreadId}...");
            throw new InvalidOperationException("Exception 2");
        });

        var t = Task.WhenAll(task1, task2);

        try
        {
            await t;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Catch run on {Thread.CurrentThread.ManagedThreadId}...");

            Assert.IsInstanceOfType(ex, typeof(AccessViolationException));
        }
    }
}