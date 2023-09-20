namespace Cases.lang;

[TestClass]
public class InheritanceTest
{
    [TestMethod]
    public void test()
    {
        A a = new A();
        B b = new B();
        a.Func2(b);
        b.Func2(a);
    }

    class A
    {
        public virtual void Func1(int i)
        {
            Console.WriteLine(i);
        }

        public void Func2(A a)
        {
            a.Func1(1);
            Func1(5);
        }
    }

    class B : A
    {
        public override void Func1(int i)
        {
            base.Func1(i+1);
        }

        public void Func2(A a)
        {
            a.Func1(1);
            Func1(5);
        }
    }
}
