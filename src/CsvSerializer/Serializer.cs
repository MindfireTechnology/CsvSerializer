using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CsvSerializer.Csv;
using Newtonsoft.Json;

namespace CsvSerializer
{
	public class Serializer : ISerializer
	{
		public CsvSettings Settings { get; set; }

		public Stack<ICsvCustomSerializer> Serializers { get; protected set; }

		public Serializer()
		{
			Settings = new CsvSettings();
			Serializers =
				new Stack<ICsvCustomSerializer> 
				{
					//new DefaultCsvSerializer()
				};
		}

		public void Serialize(Stream output, object value, bool leaveStreamOpen = true)
		{
			if (output == null)
				throw new ArgumentNullException("output");

			if (value == null)
				throw new ArgumentNullException("value");


			var columnList = GetColumnList(value);

			using (var csv = new CsvBuilder(Settings, output, leaveStreamOpen))
			{
				// Setup Columns
				csv.AddColumns(columnList);

				// Write out row data
				foreach (object rowObject in EnumerateRows(value))
				{
					var row = csv.AddRow();
					PopulateRowData(row, rowObject);
				}
			}
		}

		private void PopulateRowData(Row row, object rowObject)
		{
			foreach (var cell in row.Values)
			{
				cell.Value = GetValue(cell, rowObject);
			}
		}

		private string GetValue(Cell cell, object rowObject)
		{
			//cell.Column.Info.GetValue(rowObject, null).ToString();
			object o = ResolveObjectPathOrNull(cell.Column.ObjectPath, rowObject, cell.Column.ObjectIndex);
			if (o == null)
				return string.Empty;
			else
				return o.ToString();
		}

		private object ResolveObjectPathOrNull(string path, object value, int? rownum = null)
		{
			string[] heirarchy = path.Split('.');
			foreach (string part in heirarchy)
			{
				var prop = value.GetType().GetProperty(part);
				value = prop.GetValue(value, null);

				if (value is IEnumerable && !(value is string) && rownum != null)
					value = ((IEnumerable)value).GetObjectAtIndex(rownum.Value);

				if (value == null)
					return null;
			}

			return value;
		}

		protected IEnumerable<PropertyData> GetProperties(object value, bool recursive, string prefix = "", int? colnum = null)
		{
			if (prefix == null)
				prefix = string.Empty;

			foreach (var prop in value.Properties())
			{
				string name = (prefix + "." + prop.Name).TrimStart('.');
				string simpleName = (!Settings.ShowFullNamePath ? string.Empty : 
					prefix + (colnum == null ? string.Empty : colnum.ToString()) + Settings.NamePathDelimeter) + prop.Name;
				if (simpleName.StartsWith(Settings.NamePathDelimeter))
					simpleName = simpleName.Substring(Settings.NamePathDelimeter.Length);

				var attributes = prop.GetCustomAttributes(true);

				// Check for ignore
				if ((attributes.Any(n => n.TypeName() == "CsvIgnoreAttribute")) ||
					(Settings.UseSerializerAttributes && attributes.Any(n => n.TypeName() == "NonSerializedAttribute")) ||
					(Settings.UseXmlAttributes && attributes.Any(n => n.TypeName() == "XmlIgnoreAttribute")) ||
					(Settings.UseJsonAttributes && attributes.Any(n => n.TypeName() == "JsonIgnoreAttribute")))
					continue;

				// Check for JsonPropertyAttribute
				if (Settings.UseJsonAttributes && attributes.Any(n => n.TypeName() == "JsonPropertyAttribute"))
				{
					var attribute = (JsonPropertyAttribute)attributes.First(n => n.TypeName() == "JsonPropertyAttribute");
					simpleName = attribute.PropertyName;
				}

				// Check for collection
				object propValue = prop.GetValue(value, null);
				if (Settings.ConvertChildCollectionsToRows && propValue is IEnumerable && !(propValue is string))
				{
					// The row names are index based Address1, Address2, etc. 
					int rownum = 1;
					foreach (object child in EnumerateRows(propValue))
						foreach (var pd in GetProperties(child, true, name, rownum++))
							yield return pd;
				}
				else if (!IsClrType(prop.PropertyType) && recursive && propValue != null)
				{
					foreach (var pd in GetProperties(propValue, true, name))
						yield return pd;
				}
				else
				{
					yield return new PropertyData
					{
						Info = prop,
						Name = simpleName,
						ObjectPath = name,
						ObjectIndex = colnum,
						PropertyValue = propValue
					};
				}
			}
		}

		protected class PropertyData
		{
			public PropertyInfo Info { get; set; }
			public string Name { get; set; }
			public string ObjectPath { get; set; }
			public int? ObjectIndex { get; set; }
			public object PropertyValue { get; set; }
		}

		protected List<Column> GetColumnList(object value)
		{
			var result = new List<Column>();

			// Is the base collection Enumerable? If so, ignore it and use a child as the prototype.
			if (value is IEnumerable)
			{
				var enumerator = ((IEnumerable)value).GetEnumerator();
				if (enumerator.MoveNext() && enumerator.Current != null)
					value = enumerator.Current;
			}

			return GetProperties(value, true).Select(n => new Column 
			{ 
				Name = n.Name, 
				ObjectPath = n.ObjectPath, 
				ObjectIndex = n.ObjectIndex,
				Info = n.Info 
			}).ToList();
		}

		private bool IsClrType(Type type)
		{
			if (type == typeof(string) ||
				type == typeof(decimal) ||
				type == typeof(decimal?) ||
				type == typeof(Array) ||
				type == typeof(DateTime) ||
				type == typeof(DateTime?) ||
				type == typeof(char) ||
				type == typeof(char?) ||
				type == typeof(byte) ||
				type == typeof(byte?) ||
				type == typeof(short) ||
				type == typeof(short?) ||
				type == typeof(ushort) ||
				type == typeof(ushort?) ||
				type == typeof(int) ||
				type == typeof(int?) ||
				type == typeof(uint) ||
				type == typeof(uint?) ||
				type == typeof(long) ||
				type == typeof(long?) ||
				type == typeof(ulong) ||
				type == typeof(ulong?) ||
				type == typeof(float) ||
				type == typeof(float?) ||
				type == typeof(double) ||
				type == typeof(double?))
				return true;
			else
				return false;
		}

		private ICsvCustomSerializer GetSerializer(PropertyInfo prop)
		{
			return null;
		}

		private IOutputFormatter GetFormatter(PropertyInfo prop)
		{
			return null;
		}

		private IEnumerable<object> EnumerateRows(object value)
		{
			if (value is IEnumerable)
				return (IEnumerable<object>)value;
			else
				return EmumerateOne(value);
		}

		private IEnumerable<object> EmumerateOne(object value)
		{
			yield return value;
		}
	}
}
