using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace System
{
  static class SplitCamelCaseExtension
	{
		public static string SplitCamelCase(this string str)
		{
			return Regex.Replace(Regex.Replace(str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2"), @"(\p{Ll})(\P{Ll})", "$1 $2");
		}

		public static IEnumerable<string> RemoveQuoteAndSplit(this string? str, string separator) 
		{
			if (string.IsNullOrWhiteSpace(str)) return Array.Empty<string>();
			var pattern = @$"{separator}(?=(?:[^""]*""[^""]*"")*[^""]*$)";			
      var values = Regex.Split(str, pattern);
			return values.Select(i => i.Replace("\"", string.Empty));
    }

		public static async IAsyncEnumerable<string[]> ReadRecordAsync(this StreamReader reader, string separator, int fieldCount, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			bool eof = false;
			var line = await reader.ReadLineAsync();
			while (line != null)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (eof)
				{
          var fields = line.RemoveQuoteAndSplit(separator).ToArray();
          yield return fields;
					break;
        }

        var quoteCount = line.Count(c => c == '"');
				if (quoteCount % 2 == 0)
				{
					var fields = line.RemoveQuoteAndSplit(separator).ToArray();
					if (fields.Length >= fieldCount)
					{
						yield return fields;
						line = await reader.ReadLineAsync();
					}
					else
					{
						var newline = await reader.ReadLineAsync();
						eof = newline == null;
						line += "\n" + newline;
					}
				}
				else
				{
          var newline = await reader.ReadLineAsync();
          eof = newline == null;
          line += "\n" + newline;
        }
      }
		}
  }
}
