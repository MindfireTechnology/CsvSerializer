using System;
using System.IO;
using System.Text;
using CsvSerializer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvSerializerTests
{
	[TestClass]
	public class BasicFunctionality
	{
		[TestMethod]
		public void IntegrationTestSimpleObject()
		{
			// Arrange
			var serializer = new Serializer();
			var person = new Person { FirstName = "Nate \"D\"", LastName = "Zaugg" };
			var ms = new MemoryStream();
			string expected = "﻿FirstName,LastName\r\n\"Nate \"\"D\"\"\",Zaugg\r\n";
			string actual;

			// Act
			serializer.Serialize(ms, person);
			actual = Encoding.UTF8.GetString(ms.ToArray());

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void IntegrationTestSimpleObjectArray()
		{
			// Arrange
			var serializer = new Serializer();
			var people = new[]
			{
				new Person{ FirstName = "Nate \"D\"", LastName = "Zaugg" },
				new Person{ FirstName = "James\r\nTheKid", LastName = "King" },
				new Person{ FirstName = "Tiffany", LastName = "Zaugg" },
			};
			var ms = new MemoryStream();
			string expected = "﻿FirstName,LastName\r\n\"Nate \"\"D\"\"\",Zaugg\r\nJames TheKid,King\r\nTiffany,Zaugg\r\n";
			string actual;

			// Act
			serializer.Serialize(ms, people);
			actual = Encoding.UTF8.GetString(ms.ToArray());

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void AssertLinesEndWithCorrectLineEnding()
		{
			// Arrange
			var serializer = new Serializer { Settings = new CsvSettings { NewLineDelimeter = "\n" } };
			var person = new Person { FirstName = "Nate \"D\"", LastName = "Zaugg" };
			var ms = new MemoryStream();
			string expected = "﻿FirstName,LastName\n\"Nate \"\"D\"\"\",Zaugg\n";
			string actual;

			// Act
			serializer.Serialize(ms, person);
			actual = Encoding.UTF8.GetString(ms.ToArray());

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void AssertHeaderPrintsOnlyWhenRequested()
		{
			// Arrange
			var serializer = new Serializer { Settings = new CsvSettings { WriteHeaders = false } };
			var person = new Person { FirstName = "Nate \"D\"", LastName = "Zaugg" };
			var ms = new MemoryStream();
			string expected = "﻿\"Nate \"\"D\"\"\",Zaugg\r\n";
			string actual;

			// Act
			serializer.Serialize(ms, person);
			actual = Encoding.UTF8.GetString(ms.ToArray());

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void AssertValuesWithQuotesReceiveQuotes()
		{
			// Arrange
			var serializer = new Serializer();
			var person = new Person { FirstName = "Nate \"D\"", LastName = "Zaugg" };
			var ms = new MemoryStream();
			string expected = "﻿FirstName,LastName\r\n\"Nate \"\"D\"\"\",Zaugg\r\n";
			string actual;

			// Act
			serializer.Serialize(ms, person);
			actual = Encoding.UTF8.GetString(ms.ToArray());

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void AssertValuesWithCamasReceiveQuotes()
		{
			// Arrange
			var serializer = new Serializer();
			var person = new Person { FirstName = "Nate", LastName = "Dr, Zaugg" };
			var ms = new MemoryStream();
			string expected = "﻿FirstName,LastName\r\nNate,\"Dr, Zaugg\"\r\n";

			string actual;

			// Act
			serializer.Serialize(ms, person);
			actual = Encoding.UTF8.GetString(ms.ToArray());

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void AssertValuesWithLineEndingsDoTheRightThing()
		{
			// Arrange
			var serializer = new Serializer { Settings = new CsvSettings { RemoveLineBreaksInFields = false } };
			var person = new Person { FirstName = "Nate", LastName = "Dr\r\n Zaugg" };
			var ms = new MemoryStream();
			string expected = "﻿FirstName,LastName\r\nNate,\"Dr\r\n Zaugg\"\r\n";

			string actual;

			// Act
			serializer.Serialize(ms, person);
			actual = Encoding.UTF8.GetString(ms.ToArray());

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void FlattenObjectAsColumns()
		{
			// Arrange
			var serializer = new Serializer();
			var order = new Order
			{
				Id = "Order/12",
				OrderDate = new DateTime(2015, 06, 01),
				Customer = new Person { FirstName = "Nate", LastName = "Zaugg" }
			};
			var ms = new MemoryStream();
			string expected = "﻿Id,OrderDate,Customer.FirstName,Customer.LastName,Subtotal,Tax,Total\r\nOrder/12,6/1/2015 12:00:00 AM,Nate,Zaugg,0,0,0\r\n";

			string actual;

			// Act
			serializer.Serialize(ms, order);
			actual = Encoding.UTF8.GetString(ms.ToArray());

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void FlattenCollectionsAsColumns()
		{
			// Arrange
			var serializer = new Serializer();
			var order = new Order
			{
				Id = "Order/12",
				OrderDate = new DateTime(2015, 06, 01),
				Customer = new Person { FirstName = "Nate", LastName = "Zaugg" },
				Subtotal = 300,
				Tax = 22,
				Total = 322
			};
			order.Add(new OrderItem { Id = "1", Name = "Galaxy S5", ShortDescription = "My phone is nice!", Qty = 1, PricePerQty = 200, LineTotal = 200 });
			order.Add(new OrderItem { Id = "2", Name = "Xoom Tablet", ShortDescription = "I like Xoom tab", Qty = 1, PricePerQty = 100, LineTotal = 100 });

			var ms = new MemoryStream();
			string expected = "﻿Id,OrderDate,Customer.FirstName,Customer.LastName,Subtotal,Tax,Total,Items1.Name,Items1.ShortDescription,Items1.PricePerQty,Items1.Qty,Items1.LineTotal,Items1.TimeShipped,Items1.DiscountAmount,Items2.Name,Items2.ShortDescription,Items2.PricePerQty,Items2.Qty,Items2.LineTotal,Items2.TimeShipped,Items2.DiscountAmount\r\nOrder/12,6/1/2015 12:00:00 AM,Nate,Zaugg,300,22,322,Galaxy S5,My phone is nice!,200,1,200,,,Xoom Tablet,I like Xoom tab,100,1,100,,\r\n";

			string actual;

			// Act
			serializer.Serialize(ms, order);
			actual = Encoding.UTF8.GetString(ms.ToArray());

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		public void AssertNumberOfRowsDoesNotChangeWithType()
		{
			// Note: We need to serialize base on the declairing type rather than the reflected type.
			// Example: Two classes: Person -> Parent. If we have IEnumerable<Person> and one is a parent,
			//          then we need to serialize the same way (Person) for all objects.
		}

		[TestMethod]
		public void AssertOutputFormatterWorkingCorrectly()
		{
		}

		[TestMethod]
		public void AssertChildObjectsSerialize()
		{
		}

		[TestMethod]
		public void AssertChildObjectsNamedCorrectly()
		{
		}

		[TestMethod]
		public void TestSingleObjectEnumeration()
		{
		}

		[TestMethod]
		public void TestEnumerableObjectEnumeration()
		{
		}

		[TestMethod]
		public void TestEnumValuesSerializeCorrectly()
		{
		}

		[TestMethod]
		public void TestInfinateRecursionNotHappening()
		{
			// Note: Parent <---> Child is always a bad idea! But we should make sure we don't be stupid because someone else is
		}

		[TestMethod]
		public void AssertNullableValuesSerialize()
		{
			// Arrange
			var serializer = new Serializer();
			var order = new Order
			{
				Id = "Order/13",
				OrderDate = new DateTime(2017, 04, 07),
				Customer = new Person { FirstName = "Zach", LastName = "Thurston" },
				Subtotal = 935,
				Tax = 21,
				Total = 956
			};
			order.Add(new OrderItem { Id = "1", Name = "iPhone 7", ShortDescription = "128 GB Jet Black", Qty = 1, PricePerQty = 850, LineTotal = 775, DiscountAmount = 75, TimeShipped = new DateTime(2017, 04, 07, 12, 3, 00) });
			order.Add(new OrderItem { Id = "2", Name = "AirPods", ShortDescription = "Wireless earbuds", Qty = 1, PricePerQty = 160, LineTotal = 160 });

			var ms = new MemoryStream();
			var sb = new StringBuilder();
			sb.Append("﻿Id,OrderDate,Customer.FirstName,Customer.LastName,Subtotal,Tax,Total,");
			sb.Append("Items1.Name,Items1.ShortDescription,Items1.PricePerQty,Items1.Qty,Items1.LineTotal,Items1.TimeShipped,Items1.DiscountAmount,");
			sb.Append("Items2.Name,Items2.ShortDescription,Items2.PricePerQty,Items2.Qty,Items2.LineTotal,Items2.TimeShipped,Items2.DiscountAmount\r\n");
			sb.Append("Order/13,4/7/2017 12:00:00 AM,Zach,Thurston,935,21,956,");
			sb.Append("iPhone 7,128 GB Jet Black,850,1,775,4/7/2017 12:03:00 PM,75,");
			sb.Append("AirPods,Wireless earbuds,160,1,160,,\r\n");

			string actual;
			string expected = sb.ToString();

			// Act
			serializer.Serialize(ms, order);
			actual = Encoding.UTF8.GetString(ms.ToArray());

			// Assert
			Assert.AreEqual(expected, actual);
		}
	}

}
