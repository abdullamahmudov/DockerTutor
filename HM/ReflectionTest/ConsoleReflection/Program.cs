using System.Diagnostics;
using System.Security.AccessControl;
using ConsoleReflection.ExampleTypes;
using ConsoleReflection.Implementation;
using ConsoleReflection.Interfaces;

var list = new List<int> { 100, 1_000, 10_000, 100_000 };
var stopWatch = new Stopwatch();
var tableBuilder = new TableBuilder();
var info = new SystemInfo();
Console.WriteLine(info.GetInfo());
var obj = new F { i1 = 1, i2 = 2, i3 = 3, i4 = 4, i5 = 5 };
var testCustomSerializer = new TestSerializer(new CustomCSVSerializer(), tableBuilder, stopWatch);
var testJsonSerializer = new TestSerializer(new NewtonsoftJsonSerializer(), tableBuilder, stopWatch);
var testCustomDeserializer = new TestDeserializer<F>(new CustomCSVDeserializer(), tableBuilder, stopWatch);
var testJsonDeserializer = new TestDeserializer<F>(new NewtonsoftJsonDeserializer(), tableBuilder, stopWatch);

testCustomSerializer.Test(obj, list);
testJsonSerializer.Test(obj, list);
testCustomDeserializer.Test(testCustomSerializer.Result, list);
testJsonDeserializer.Test(testJsonSerializer.Result, list);

testCustomSerializer.Write();
testJsonSerializer.Write();
testCustomDeserializer.Write();
testJsonDeserializer.Write();



