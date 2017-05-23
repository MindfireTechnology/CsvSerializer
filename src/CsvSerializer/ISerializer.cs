using System.Collections.Generic;
using System.IO;

namespace CsvSerializer
{
	public interface ISerializer
	{
		/// <summary>Stack of available serializers. This list will be interrogated in LIFO order for the most compatable serializer</summary>
		/// <remarks>Note: There is a default serializer added upon class creation. Removing all serializers will result in a runtime exception.</remarks>
		Stack<ICsvCustomSerializer> Serializers { get; }

		/// <summary>The settings that are in effect for this instance of the Serializer.</summary>
		CsvSettings Settings { get; set; }

		/// <summary>
		/// Serialize an object to CSV
		/// </summary>
		/// <param name="output">The stream we use to write the output</param>
		/// <param name="value">The object to serialize</param>
		/// <param name="leaveOpen">true to leave the stream open after running this method otherwise, false</param>
		void Serialize(Stream output, object value, bool leaveStreamOpen = true);
	}
}