using System.Text.Json;

namespace SimpleComControl.Core.Helpers
{
    public static class JsonHelpers
    {

        public static string? ToJson<T>(this T x, bool makePretty = true)
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = makePretty
            };
            return x.ToJson(options);
        }
        public static string? ToJson<T>(this T x, JsonSerializerOptions? options)
        {
            if (x == null) { return null; }

            return JsonSerializer.Serialize<T>(x, options);
        }

        public static T? FromJson<T>(this string x, JsonSerializerOptions? options = null)
        {
            if (!string.IsNullOrWhiteSpace(x))
            {
                return JsonSerializer.Deserialize<T>(x, options);
            }
            return default;
        }


        public static bool ToJsonFile<T>(this T x, string filePath, bool makePretty = true)
        {
            JsonSerializerOptions options = new() 
            {
                WriteIndented = makePretty
            };
            return x.ToJsonFile(filePath, options);
        }
        public static bool ToJsonFile<T>(this T x, string filePath, JsonSerializerOptions? options)
        {
            if (x != null && !string.IsNullOrWhiteSpace(filePath))
            {
                string? json = x.ToJson(options);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    File.WriteAllText(filePath, json);
                    return true;
                }
            }
            return false;
        }

        public static T? FromJsonFile<T>(string filePath, JsonSerializerOptions? options = null)
        {
            T? result = default;
            if(!string.IsNullOrWhiteSpace(filePath))
            {
                string json = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    result = json.FromJson<T>(options);
                }
            }
            return result;
        }
    }
}
