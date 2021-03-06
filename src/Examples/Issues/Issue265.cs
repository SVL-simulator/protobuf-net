
using System;
using Xunit;
using ProtoBuf;
using ProtoBuf.Meta;
using System.Linq;

namespace Examples.Issues
{
    
    public class Issue265
    {
        public enum E
        {
            [ProtoEnum(Value = 3)]
            V0 = 0,
            [ProtoEnum(Value = 4)]
            V1 = 1,
            [ProtoEnum(Value = 5)]
            V2 = 2,
        }
        [ProtoContract]
        public class Foo
        {
            [ProtoMember(3)]
            public E[] Bar { get; set; }
        }

        [Fact]
        public void ShouldSerializeEnumArrayMember()
        {
            var model = TypeModel.Create();
            model.AutoCompile = false;
            TestMember(model);
            model.Compile("ShouldSerializeEnumArrayMember", "ShouldSerializeEnumArrayMember.dll");
            PEVerify.AssertValid("ShouldSerializeEnumArrayMember.dll");
            model.CompileInPlace();
            TestMember(model);
            TestMember(model.Compile());
        }

        private static void TestMember(TypeModel model)
        {
            var value = new Foo {Bar = new E[] {E.V0, E.V1, E.V2}};

            Assert.True(Program.CheckBytes(value, model, 0x18, 0x03, 0x18, 0x04, 0x18, 0x05));
            var clone = (Foo) model.DeepClone(value);
            Assert.Equal("V0,V1,V2", string.Join(",", clone.Bar)); //, "clone");
        }

        [Fact]
        public void VerifyThatIntsAreHandledAppropriatelyForComparison()
        {
            var model = TypeModel.Create();
            model.AutoCompile = false;
            var orig = new int[] {3, 4, 5};
            Assert.True(Program.CheckBytes(orig, model, 0x08, 0x03, 0x08, 0x04, 0x08, 0x05));
            var clone = (int[])model.DeepClone(orig);
            Assert.Equal("3,4,5", string.Join(",", clone)); //, "clone");
        }

        [Fact]
        public void ShouldSerializeIndividualEnum()
        {
            var model = TypeModel.Create();
            model.AutoCompile = false;
            TestIndividual(model);
            model.Compile("ShouldSerializeIndividualEnum", "ShouldSerializeIndividualEnum.dll");
            PEVerify.AssertValid("ShouldSerializeIndividualEnum.dll");
            model.CompileInPlace();
            TestIndividual(model);
            TestIndividual(model.Compile());
        }

        private static void TestIndividual(TypeModel model)
        {
            var value = E.V1;
            Assert.True(Program.CheckBytes(value, model, 0x08, 0x04));
            Assert.Equal(value, model.DeepClone(value));
        }

        [Fact]
        public void ShouldSerializeArrayOfEnums()
        {
            var model = TypeModel.Create();
            model.AutoCompile = false;
            TestArray(model);
            model.Compile("ShouldSerializeArrayOfEnums", "ShouldSerializeArrayOfEnums.dll");
            PEVerify.AssertValid("ShouldSerializeArrayOfEnums.dll");
            model.CompileInPlace();
            TestArray(model);
            TestArray(model.Compile());
        }

        private static void TestArray(TypeModel model)
        {
            var value = new[] {E.V0, E.V1, E.V2};
            Assert.Equal("V0,V1,V2", string.Join(",", value)); //, "original");
            Assert.True(Program.CheckBytes(value, model, 0x08, 0x03, 0x08, 0x04, 0x08, 0x05));
            var clone = (E[]) model.DeepClone(value);
            Assert.Equal("V0,V1,V2", string.Join(",", clone)); //, "clone");
            value.SequenceEqual(clone);
        }
    }
}
