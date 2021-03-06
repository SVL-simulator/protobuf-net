#if NET_3_0
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;
using Xunit;
using ProtoBuf.ServiceModel;

namespace Examples.ServiceModel
{
    // in case of errors, see: http://msdn.microsoft.com/en-us/library/ms733768.aspx
    // for example: netsh http add urlacl url=http://+:84/MyService user=mydomain\myuser
    [DataContract]
    public class MyData
    {
        public MyData() { SubData = new List<MySubData>(); }
        [DataMember(Order = 1)]
        public List<MySubData> SubData { get; private set; }
    }
    [DataContract]
    public class MySubData
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }
        [DataMember(Order = 2)]
        public int Number { get; set; }
    }

    [ServiceContract]
    interface IMyService
    {
        [OperationContract, ProtoBehavior]
        MyData UsingProtoItem(MyData data);

        [OperationContract]
        MyData RegularWcfItem(MyData data);

        [OperationContract, ProtoBehavior]
        MyData[] UsingProtoList(List<MyData> data);

        [OperationContract]
        MyData[] RegularWcfList(List<MyData> data);

        [OperationContract, ProtoBehavior]
        int ComplexMethod(List<MyData> a, MyData b, MyData c, List<MyData> d);

        [OperationContract]
        bool Ping();

        [OperationContract]
        string SimpleTypesRegular(int value);
        [OperationContract, ProtoBehavior]
        string SimpleTypesProto(int value);
    }
    class MyService : IMyService
    {
        public MyData RegularWcfItem(MyData data) { return data; }
        public MyData UsingProtoItem(MyData data) { return data; }
        public MyData[] RegularWcfList(List<MyData> data) { return data == null ? null : data.ToArray(); }
        public MyData[] UsingProtoList(List<MyData> data) { return data == null ? null : data.ToArray(); }

        public bool Ping() { return true; }

        public string SimpleTypesRegular(int value)
        {
            return value.ToString();
        }
        public string SimpleTypesProto(int value)
        {
            return value.ToString();
        }

        public int ComplexMethod(List<MyData> a, MyData b, MyData c, List<MyData> d)
        {
            int count = 0;
            if (a != null) count += a.Count;
            if (b != null) count++;
            if (c != null) count++;
            if (d != null) count += d.Count;
            return count;
        }
    }


    
    public class WcfTest
    {
        ServiceHost host;
        [TestFixtureSetUp]
        public void StartServer()
        {
            try
            {
                StopServer();
                host = new ServiceHost(typeof(MyService),
                    new Uri("http://localhost:84/MyService"));
                host.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine("You may need to execute (as admin):");
                Console.WriteLine(@"netsh http add urlacl url=http://+:84/MyService/ user=""NT AUTHORITY\Authenticated Users""");
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [TestFixtureTearDown]
        public void StopServer()
        {
            if (host != null)
            {
                host.Close();
                host = null;
            }
        }

        static WcfProxy<IMyService> GetProxy()
        {
            return new WcfProxy<IMyService>();
        }
        [Fact]
        public void Ping()
        {
            using (var proxy = GetProxy())
            {
                Assert.True(proxy.Service.Ping());
            }
        }

        [Fact]
        public void RoundTripTrivial()
        {
            using(var proxy = GetProxy())
            {
                var data = proxy.Service.UsingProtoItem(new MyData {});
                Assert.NotNull(data);
            }
        }
        [Fact]
        public void SimpleTypesRegular()
        {
            using (var proxy = GetProxy())
            {
                Assert.Equal("27", proxy.Service.SimpleTypesRegular(27));
            }
        }

        [Fact]
        public void SimpleTypesProto()
        {
            using (var proxy = GetProxy())
            {
                Assert.Equal("27", proxy.Service.SimpleTypesProto(27));
            }
        }

        [Fact]
        public void WcfRegularNullToNull()
        {
            using (var proxy = GetProxy())
            {
                Assert.Null(proxy.Service.RegularWcfItem(null));
            }
        }

        [Fact]
        public void WcfProtoNullToNull()
        {
            using (var proxy = GetProxy())
            {
                Assert.Null(proxy.Service.UsingProtoItem(null));
            }
        }

        static List<MyData> GetShortList()
        {
            List<MyData> list = new List<MyData>();
            for (int j = 0; j < 5; j++)
            {
                MyData data = new MyData();
                for (int i = 0; i < 50; i++)
                {
                    data.SubData.Add(new MySubData { Number = i, Name = "item " + i.ToString() });
                }
                list.Add(data);
            }
            return list;
        }
        static void CheckListResult(MyData[] arr)
        {
            Assert.NotNull(arr, "Null");
            Assert.Equal(5, arr.Length, "Length");
            for (int j = 0; j < arr.Length; j++)
            {
                MyData item = arr[j];
                Assert.NotNull(item, "Null: " + j.ToString());
                Assert.NotNull(item.SubData, "SubData Null: " + j.ToString());
                Assert.Equal(50, item.SubData.Count, "SubData Count: " + j.ToString());
                for (int i = 0; i < item.SubData.Count; i++)
                {
                    var subItem = item.SubData[i];
                    Assert.NotNull(subItem, i.ToString());
                    Assert.Equal(i, subItem.Number, "Number: " + i.ToString());
                    Assert.Equal("item " + i.ToString(), subItem.Name, "Name: " + i.ToString());
                }
            }
        }
        [Fact]
        public void RegularWcfList()
        {
            using (var proxy = GetProxy())
            {
                MyData[] arr = proxy.Service.RegularWcfList(GetShortList());
                CheckListResult(arr);
            }
        }
        [Fact]
        public void ProtoList()
        {
            using (var proxy = GetProxy())
            {
                MyData[] arr = proxy.Service.UsingProtoList(GetShortList());
                CheckListResult(arr);
            }
        }

        [Fact]
        public void ProtoListEmpty()
        {
            using (var proxy = GetProxy())
            {
                var emptyList = new List<MyData>();
                MyData[] arr = proxy.Service.UsingProtoList(emptyList);
                Assert.NotNull(arr);
                Assert.Equal(0, arr.Length);
            }
        }

        [Fact]
        public void ProtoItemEmpty()
        {
            using (var proxy = GetProxy())
            {
                var emptyItem = new MyData();
                MyData result = proxy.Service.UsingProtoItem(emptyItem);
                Assert.NotNull(result);
                Assert.Equal(0, result.SubData.Count);
            }
        }

        [Fact]
        public void RunWcfTest()
        {
            // generate some data:
            MyData data = new MyData();
            for (int i = 0; i < 5000; i++)
            {
                data.SubData.Add(new MySubData { Number = i, Name = "item " + i.ToString() });
            }

            using (var proxy = GetProxy())
            {
                // for JIT etc
                proxy.Service.RegularWcfItem(data);
                proxy.Service.UsingProtoItem(data);

                Stopwatch watchProto = Stopwatch.StartNew();
                MyData dataProto = proxy.Service.UsingProtoItem(data);
                watchProto.Stop();
                Console.WriteLine("WCF: Proto took: {0}", watchProto.ElapsedMilliseconds);

                Stopwatch watchRegular = Stopwatch.StartNew();
                MyData dataRegular = proxy.Service.RegularWcfItem(data);
                watchRegular.Stop();
                Console.WriteLine("WCF: Regular took: {0}", watchRegular.ElapsedMilliseconds);

                Assert.Equal(data.SubData.Count, dataProto.SubData.Count, "Proto count");
                Assert.Equal(data.SubData.Count, dataRegular.SubData.Count, "Regular count");
                for (int i = 0; i < data.SubData.Count; i++)
                {
                    Assert.Equal(data.SubData[i].Name, dataProto.SubData[i].Name, "Proto name");
                    Assert.Equal(data.SubData[i].Number, dataProto.SubData[i].Number, "Proto number");
                    Assert.Equal(data.SubData[i].Name, dataRegular.SubData[i].Name, "Regular name");
                    Assert.Equal(data.SubData[i].Number, dataRegular.SubData[i].Number, "Regular number");
                }
                Console.WriteLine(string.Format("Validated: {0}", data.SubData.Count));

                Assert.True(watchProto.ElapsedMilliseconds < watchRegular.ElapsedMilliseconds); //, "Proto should be quicker");
            }
        }

        [Fact]
        public void TestComplexPermutations()
        {
            MyData trivial = new MyData(), nonTrivial = new MyData { SubData = { new MySubData { Number = 12345 } } };
            List<MyData> empty = new List<MyData>(0);
            List<MyData> fourItems = new List<MyData> {trivial, nonTrivial, trivial, nonTrivial};
 
            using (var proxy = GetProxy())
            {
                int i = 0;
                try
                {
                    Assert.Equal(0, proxy.Service.ComplexMethod(null, null, null, null));
                    i++;
                    Assert.Equal(10, proxy.Service.ComplexMethod(fourItems, nonTrivial, nonTrivial, fourItems));
                    i++;
                    Assert.Equal(2, proxy.Service.ComplexMethod(null, trivial, nonTrivial, null));
                    i++;
                    Assert.Equal(1, proxy.Service.ComplexMethod(null, trivial, null, empty));
                    i++;
                    Assert.Equal(5, proxy.Service.ComplexMethod(fourItems, trivial, null, empty));
                    i++;
                    Assert.Equal(9, proxy.Service.ComplexMethod(fourItems, trivial, null, fourItems));
                    i++;
                    Assert.Equal(8, proxy.Service.ComplexMethod(fourItems, null, null, fourItems));
                } catch
                {
                    Debug.WriteLine(i);
                    throw;
                }
            }
        }

    }


    public sealed class WcfProxy<T> : ClientBase<T>, IDisposable
    where T : class
    {
        public T Service { get { return base.Channel; } }
        public void Dispose()
        {
            try
            {
                switch (State)
                {
                    case CommunicationState.Closed:
                        break; // nothing to do
                    case CommunicationState.Faulted:
                        Abort();
                        break;
                    case CommunicationState.Opened:
                        Close();
                        break;
                }
            }
            catch { } // best efforts...
        }
    }
}
#endif