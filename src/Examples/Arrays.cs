using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using ProtoBuf;
using ProtoBuf.Meta;
using System.Linq;
namespace Examples
{
    [ProtoContract]
    class Node
    {
        [ProtoMember(1)]
        public int Key { get; set; }

        [ProtoMember(2)]
        public Node[] Nodes { get; set; }        
    }

    [ProtoContract]
    class Prim
    {
        [ProtoMember(1)]
        public string[] Values { get; set; }
    }

    [ProtoContract]
    class ArrayArray
    {
        [ProtoMember(1)]
        public string[][] Values { get; set; }
    }

    [ProtoContract]
    class ArrayList
    {
        [ProtoMember(1)]
        public List<string>[] Values { get; set; }
    }
    [ProtoContract]
    class ListArray
    {
        [ProtoMember(1)]
        public List<string[]> Values { get; set; }
    }
    [ProtoContract]
    class ListList
    {
        [ProtoMember(1)]
        public List<List<string>> Values { get; set; }
    }

    [ProtoContract]
    class MultiDim
    {
        [ProtoMember(1)]
        public int[,] Values { get; set; }
    }

    [ProtoContract(SkipConstructor=false)]
    public class WithAndWithoutOverwrite
    {
        [ProtoMember(1, OverwriteList=false)]
        public int[] Append = { 1, 2, 3 };

        [ProtoMember(2, OverwriteList=true)]
        public int[] Overwrite = { 4, 5, 6 };
    }
    [ProtoContract(SkipConstructor=true)]
    public class WithSkipConstructor
    {
        [ProtoMember(1)]
        public int[] Values = { 1, 2, 3 };
    }

    
    public class ArrayTests
    {
        [ProtoContract]
        class Foo { }
        [Fact]
        public void DeserializeNakedArray()
        {
            var arr = new Foo[0];
            var model = TypeModel.Create();
            Foo[] foo = (Foo[])model.DeepClone(arr);
            Assert.Empty(foo);
        }
        [Fact]
        public void DeserializeBusyArray()
        {
            var arr = new Foo[3] { new Foo(), new Foo(), new Foo() };
            var model = TypeModel.Create();
            Foo[] foo = (Foo[])model.DeepClone(arr);
            Assert.Equal(3, foo.Length);
        }
        [Fact]
        public void TestOverwriteVersusAppend()
        {
            var orig = new WithAndWithoutOverwrite { Append = new[] {7,8}, Overwrite = new[] { 9,10}};
            var model = TypeModel.Create();
            model.AutoCompile = false;
            model.Add(typeof(WithAndWithoutOverwrite), true);

            var clone = (WithAndWithoutOverwrite)model.DeepClone(orig);
            Assert.True(clone.Overwrite.SequenceEqual(new[] { 9, 10 }), "Overwrite, Runtime");
            Assert.True(clone.Append.SequenceEqual(new[] { 1, 2, 3, 7, 8 }), "Append, Runtime");

            model.CompileInPlace();
            clone = (WithAndWithoutOverwrite)model.DeepClone(orig);
            Assert.True(clone.Overwrite.SequenceEqual(new[] { 9, 10 }), "Overwrite, CompileInPlace");
            Assert.True(clone.Append.SequenceEqual(new[] { 1, 2, 3, 7, 8 }), "Append, CompileInPlace");

            clone = (WithAndWithoutOverwrite)model.Compile("TestOverwriteVersusAppendTypeModel", "TestOverwriteVersusAppend.dll").DeepClone(orig);
            Assert.True(clone.Overwrite.SequenceEqual(new[] { 9, 10 }), "Overwrite, CompileToFile");
            Assert.True(clone.Append.SequenceEqual(new[] { 1, 2, 3, 7, 8 }), "Append, CompileToFile");

            clone = (WithAndWithoutOverwrite)(model.Compile()).DeepClone(orig);
            Assert.True(clone.Overwrite.SequenceEqual(new[] { 9, 10 }), "Overwrite, Compile");
            Assert.True(clone.Append.SequenceEqual(new[] { 1, 2, 3, 7, 8 }), "Append, Compile");
        }

        [Fact]
        public void TestDirectSkipConstructor()
        {
            var obj = new SkipCtorType();
            Assert.Equal(42, obj.Value);

            obj = (SkipCtorType)BclHelpers.GetUninitializedObject(typeof(SkipCtorType));
            Assert.Equal(0, obj.Value);
        }
        public class SkipCtorType
        {
            public int Value { get; set; } = 42;
        }

        [Fact]
        public void TestSkipConstructor()
        {
            var orig = new WithSkipConstructor { Values = new[] { 4, 5 } };
            var model = TypeModel.Create();
            model.AutoCompile = false;
            model.Add(typeof(WithSkipConstructor), true);

            var clone = (WithSkipConstructor)model.DeepClone(orig);
            Assert.True(clone.Values.SequenceEqual(new[] { 4, 5 }), "Runtime");

            model.CompileInPlace();
            clone = (WithSkipConstructor)model.DeepClone(orig);
            Assert.True(clone.Values.SequenceEqual(new[] { 4, 5 }), "CompileInPlace");

            clone = (WithSkipConstructor)(model.Compile()).DeepClone(orig);
            Assert.True(clone.Values.SequenceEqual(new[] { 4, 5 }), "Compile");
        }

        [Fact]
        public void TestPrimativeArray()
        {
            Prim p = new Prim { Values = new[] { "abc", "def", "ghi", "jkl" } },
                clone = Serializer.DeepClone(p);

            string[] oldArr = p.Values, newArr = clone.Values;
            Assert.Equal(oldArr.Length, newArr.Length);
            for (int i = 0; i < oldArr.Length; i++)
            {
                Assert.Equal(oldArr[i], newArr[i]); //, "Item " + i.ToString());
            }
        }

        [Fact]
        public void TestMultidimArray()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                MultiDim md = new MultiDim { Values = new int[1, 2] { { 3, 4 } } };
                Serializer.DeepClone(md);
            });
        }

        [Fact]
        public void TestArrayArray()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                Serializer.DeepClone(new ArrayArray());
            });
        }
        [Fact]
        public void TestArrayList()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                Serializer.DeepClone(new ArrayList());
            });
        }
        [Fact]
        public void TestListArray()
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                Serializer.DeepClone(new ListArray());
            });
        }
        [Fact]
        public void TestListList()
        {
            Assert.Throws<NotSupportedException>(() => {
                Serializer.DeepClone(new ListList());
            });
        }


        [Fact]
        public void TestObjectArray()
        {
            Node node = new Node
            {
                Key = 27,
                Nodes = new[] {
                    new Node { Key = 1 },
                    new Node { Key = 3 } }
            };
            VerifyNodeTree(node);
        }

        [Fact(Skip = "known variation")]
        public void TestEmptyArray()
        {
            Node node = new Node
            {
                Key = 27,
                Nodes = new Node[0]
            };
            VerifyNodeTree(node);
        }

        [Fact]
        public void TestNullArray()
        {
            Node node = new Node
            {
                Key = 27,
                Nodes = null
            };
            VerifyNodeTree(node);
        }

        [Fact]
        public void TestDuplicateNonRecursive()
        {
            Node child = new Node { Key = 17 };
            Node parent = new Node { Nodes = new[] { child, child, child } };
            VerifyNodeTree(parent);
        }

        [Fact]
        public void TestDuplicateRecursive()
        {
            Assert.Throws<ProtoException>(() =>
            {
                Node child = new Node { Key = 17 };
                Node parent = new Node { Nodes = new[] { child, child, child } };
                child.Nodes = new[] { parent };
                VerifyNodeTree(parent);
            });
        }

        [Fact]
        public void TestNestedArray()
        {
            Node node = new Node
            {
                Key = 27,
                Nodes = new[] {
                    new Node {
                        Key = 19,
                        Nodes = new[] {
                            new Node {Key = 1},
                            new Node {Key = 14},
                        },
                    },
                    new Node {
                        Key = 3
                    },
                    new Node {
                        Key = 3,
                        Nodes = new[] {
                            new Node {Key = 234}
                        }
                    }
                }
            };
            VerifyNodeTree(node);
        }

        [Fact]
        public void TestStringArray()
        {
            var foo = new List<string> { "abc", "def", "ghi" };

            var clone = Serializer.DeepClone(foo);
                
        }

        [ProtoContract]
        internal class Tst
        {
            [ProtoMember(1)]
            public int ValInt
            {
                get;
                set;
            }

            [ProtoMember(2)]
            public byte[] ArrayData
            {
                get;
                set;
            }

            [ProtoMember(3)]
            public string Str1
            {
                get;
                set;
            }
        }
        [Fact]
        public void TestEmptyArrays()
        {
            Tst t = new Tst();
            t.ValInt = 128;
            t.Str1 = "SOme string text value ttt";
            t.ArrayData = new byte[] { };

            MemoryStream stm = new MemoryStream();
            Serializer.Serialize(stm, t);
            Console.WriteLine(stm.Length);
        }
        static void VerifyNodeTree(Node node) {
            Node clone = Serializer.DeepClone(node);
            string msg;
            bool eq = AreEqual(node, clone, out msg);
            Assert.True(eq, msg);
        }

        static bool AreEqual(Node x, Node y, out string msg)
        {
            // compare core
            if (ReferenceEquals(x, y)) { msg = ""; return true; }
            if (x == null || y == null) { msg = "1 node null"; return false; }
            if (x.Key != y.Key) { msg = "key"; return false; }

            Node[] xNodes = x.Nodes, yNodes = y.Nodes;
            if (ReferenceEquals(xNodes,yNodes))
            { // trivial
            }
            else
            {
                if (xNodes == null || yNodes == null) { msg = "1 Nodes null"; return false; }
                if (xNodes.Length != yNodes.Length) { msg = "Nodes length"; return false; }
                for (int i = 0; i < xNodes.Length; i++)
                {
                    bool eq = AreEqual(xNodes[i], yNodes[i], out msg);
                    if (!eq)
                    {
                        msg = i.ToString() + "; " + msg;
                        return false;
                    }
                }
            }
            // run out of things to be different!
            msg = "";
            return true;

        }
    }
}
