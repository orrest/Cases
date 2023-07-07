namespace Cases.lang;

[TestClass]
public class TryCatchFinallyReturnTest
{
    [TestMethod]
    public void return_in_try_and_catch()
    {
        Console.WriteLine("抛出异常");
        Console.WriteLine($"return {return_in_try_and_catch_ex()}");
        Console.WriteLine("不抛出异常");
        Console.WriteLine($"return {return_in_try_and_catch_no_ex()}");
    }

    private int return_in_try_and_catch_no_ex()
    {
        try
        {
            Console.WriteLine("try");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("catch");
            return -1;
        }
        finally
        {
            Console.WriteLine("finally");
        }
    }

    private int return_in_try_and_catch_ex()
    {
        try
        {
            Console.WriteLine("try");
            throw new Exception();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("catch");
            return -1;
        }
        finally
        {
            Console.WriteLine("finally");
        }
    }
}