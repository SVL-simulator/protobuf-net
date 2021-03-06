using Xunit;
using ProtoBuf;
using System;

namespace Examples.Issues
{
    
    public class SO12475521
    {
        [Fact]
        public void Execute()
        {
            var obj = new HazType { X = 1, Type = typeof(string), AnotherType = typeof(ProtoReader) };

            var clone = Serializer.DeepClone(obj);

            Assert.Equal(1, clone.X);
            Assert.Equal(typeof(string), clone.Type);
            Assert.Equal(typeof(ProtoReader), clone.AnotherType);
        }

        [ProtoContract]
        public class HazType
        {
            [ProtoMember(1)]
            public int X { get; set; }

            [ProtoMember(2)]
            public Type Type { get; set; }

            [ProtoMember(3)]
            public Type AnotherType { get; set; }
        }

    }
}
