// Written by Colin on 2023-11-05

using CoLib.Extensions;

namespace CoLibDemo.Extensions;

public static class ExceptionDemo
{

    public static void Start()
    {
        Test1();
        Test2();
        Test3();
        Test5();
    }
    
    static void Test1()
    {
        f1();
    }

    static void f1()
    {
        try
        {
            f2();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetFullStacktrace());
        }
    }
    
    static void f2()
    {
        f3();
    }
    
    static void f3()
    {
        var sum = 0;
        var value = 100 / sum;
    }


    static void Test2()
    {
        try
        {
            b1();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetFullStacktrace());
        }
    }

    static void b1()
    {
        try
        {
            b2();
        }
        catch (Exception e)
        {
            throw new Exception("b1 exception", e);
        }
    }
    
    static void b2()
    {
        try
        {
            b3();
        }
        catch (Exception e)
        {
            throw new Exception("b2 exception", e);
        }
    }
    
    static void b3()
    {
        var sum = 0;
        var value = 100 / sum;
    }
    
    static void Test3()
    {
        try
        {
            c1();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetFullStacktrace());
        }
    }

    static void c1()
    {
        Task task1 = Task.Factory.StartNew(() =>
        {
            throw new ArgumentException();
        } );
        Task task2 = Task.Factory.StartNew(() =>
        {
            throw new UnauthorizedAccessException();
        } );
        Task.WaitAll(task1, task2);
    }

    static async Task Test4()
    {
        try
        {
            await d1();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.GetFullStacktrace());
        }
    }
    
    static async Task d1()
    {
        Task task1 = Task.Factory.StartNew(() =>
        {
            throw new ArgumentException();
        } );
        Task task2 = Task.Factory.StartNew(() =>
        {
            throw new UnauthorizedAccessException();
        } );
        await Task.WhenAll(task1, task2);
    }
    
    static void Test5()
    {
        e1();
    }

    static void e1()
    {
        e2();
    }
    
    static void e2()
    {
        e3();
    }
    
    static void e3()
    {
        Console.WriteLine((new Exception("ttttttt")).GetFullStacktrace());
    }
}