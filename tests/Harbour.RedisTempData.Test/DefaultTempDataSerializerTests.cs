﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Should;

namespace Harbour.RedisTempData.Test
{
    public class DefaultTempDataSerializerTests
    {
        [Fact]
        void can_serialize_an_object()
        {
            var sut = new DefaultTempDataSerializer();

            var value = sut.Serialize(new FakeItem() { Name = "John Doe" });
            var result = sut.Deserialize(value) as FakeItem;

            result.ShouldNotBeNull();
            result.Name.ShouldEqual("John Doe");
        }

        [Fact]
        void can_serialize_a_string()
        {
            var sut = new DefaultTempDataSerializer();

            var serialized = sut.Serialize("Hello World");
            var deserialized = sut.Deserialize(serialized) as string;

            deserialized.ShouldEqual("Hello World");
        }

        [Fact]
        void deserializing_null_input_returns_null()
        {
            var sut = new DefaultTempDataSerializer();

            var result = sut.Deserialize(null);

            result.ShouldBeNull();
        }
    }
}
