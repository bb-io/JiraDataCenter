using Apps.Jira.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Apps.JiraDataCenter.Utils
{
    public class DescriptionConverter : JsonConverter<Description?>
    {
        public override Description? ReadJson(JsonReader reader, Type objectType, Description? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.String)
            {
                var text = (string?)reader.Value ?? string.Empty;

                return new Description
                {
                    Type = "doc",
                    Version = 1,
                    Content =
                    [
                        new ContentElement
                    {
                        Type = "paragraph",
                        Content =
                        [
                            new ContentElement
                            {
                                Type = "text",
                                Text = text
                            }
                        ]
                    }
                    ]
                };
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                var obj = JObject.Load(reader);
                return obj.ToObject<Description>(serializer);
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, Description? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
