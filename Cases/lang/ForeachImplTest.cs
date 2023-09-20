using System.Collections;

namespace Cases.lang;

[TestClass]
public class ForeachImplTest
{
    [TestMethod]
    public void Test_Foreach()
    {
        MyCollection collection = new MyCollection();

        // 使用 foreach 遍历自定义集合
        foreach (var item in collection)
        {
            Console.WriteLine(item);
        }
    }

    [TestMethod]
    public void Test_While_Enumerator()
    {
        MyCollection collection = new MyCollection();

        var enumerator = collection.GetEnumerator();
        while (enumerator.MoveNext())
        {
            Console.WriteLine(enumerator.Current);
        }
    }

    // 自定义集合类
    public class MyCollection : IEnumerable
    {
        private object[] items;

        public MyCollection()
        {
            items = new object[3];
            items[0] = "Apple";
            items[1] = "Banana";
            items[2] = "Orange";
        }

        // 实现 IEnumerable 接口的 GetEnumerator() 方法
        public IEnumerator GetEnumerator()
        {
            return new MyEnumerator(items);
        }
    }

    // 自定义枚举器类
    public class MyEnumerator : IEnumerator
    {
        private object[] items;
        private int position = -1;

        public MyEnumerator(object[] collection)
        {
            items = collection;
        }

        // 实现 IEnumerator 接口的 MoveNext() 方法
        public bool MoveNext()
        {
            position++;
            return (position < items.Length);
        }

        // 实现 IEnumerator 接口的 Reset() 方法
        public void Reset()
        {
            position = -1;
        }

        // 实现 IEnumerator 接口的 Current 属性
        public object Current
        {
            get { return items[position]; }
        }
    }

}
