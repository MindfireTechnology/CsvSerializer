# CsvSerializer
Convert an object graph to CSV.

## Basic Usage
	var target = new[]
	{
		new Person { Name = "Nate Zaugg", Department = "Management", Charisma = 100, Address = new Address { Street1 = "123 Fake Street", Address2 = "C/O Homer", City = "Springfield" } },
		new Person { Name = "Dan Beus", Department = "IT", Charisma = 110, Address = new Address { Street1 = "872 Heratigte Park Blvd", Address2 = "Suite 200", City = "Layton" } },
		new Person { Name = "Phil Gilmore", Department = "AV", Charisma = 110, Address = new Address { Street1 = "53 Broad Street", Address2 = "#600", City = "Salt Lake City" } },
	} 

	using (var fs = File.Open(@"C:\Temp\Output.csv"))
	{
		ISerializer serializer = new CsvSerializer(); // Obviously do this via DI
		serializer.Serialize(fs, target);
	}

	// The file output would be:
	Name,Department,Charisma,Address.Street1,Address.Street2,Address.City
	"Nate Zaugg",Management,100,"123 Fake Street","C/O Homer",Springfield
	"Dan Beus",IT,110,"872 Heratigte Park Blvd","Suite 200",Layton
	"Phil Gilmore",AV,110,"53 Broad Street",#600,"Salt Lake City"


## Settings

The settings can be modified to change the output. The default settings are fully compatible with Microsoft Excel, but there may be times where
you want something else to be output.

	serializer.Settings.WriteHeaders = false;

### WriteHeaders
Specifies if a header should be generated. Default: true (bool)

### QuoteAllValues
Specifies if all fields should be quoted, if it's required or not. Default: false (bool)

### FieldDelimeter
The delimeter between each field in a row. Default: "," (string)

### QuoteDelimeter
The delimeter used to quote fields in a row. Default: "\"" (string)

### NewLineDelimeter
The new line string used at the end of a row. Default: "CRLF" (string)

### RemoveLineBreaksInFields
Specifies if CR or LF values should be stripped from field output and replaced with whitespace. Default: True (bool)

### TextEncoding
Specifies the text encoder that should be used to write values. Default: System.Text.UTF8 (System.Text.Encoding)

### ShowFullNamePath
Specifies if sub-classes should show their parents type as part of their name. E.g. "Person.Address.Line1". Default: true (bool)

### NamePathDelimeter
The delimeter between each part of a path. E.g. "Person.Address.Line1". Default: "." (string)

### UseXmlAttributes
Specifies if attribute tags like [XmlIgnore] or [XmlElement(Name="value")] should be observed. Default: true (bool)

### UseJsonAttributes
Specifies if attribute tags like [JsonIgnore] or [JsonProperty("value")] should be observed. Default: true (bool)

### UseSerializerAttributes
Specifies if attribute tags like [NonSerialized] or [DataMember(Name="value")]. Default: true (bool)

### ConvertChildCollectionsToRows
Indicates if a collection is detected as a property on the object to be serialized, the collection will be converted to rows. 
E.g. If there is a Person with 3 addresses, there will be columns for Person.Address1.City, Person.Address2.City, and Person.Address3.City.
Alternativly, the rows in a child collection will cause the parent column values to be repeated or omitted based on the value of the
FlattenHeirarchicalStructuresWithEmptyRows property. Default: true (bool)

### FlattenHeirarchicalStructuresWithEmptyRows
Indicates if a heirarchy of objects should use the parental row values for each of the child rows or if the child rows are surrounded by 
empty values. Default: true (bool)

Note: This property is only active if ConvertChildCollectionsToRows is set to true.

