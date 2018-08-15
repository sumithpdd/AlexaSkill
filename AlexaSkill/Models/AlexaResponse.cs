using Newtonsoft.Json;

namespace AlexaSkill.Models
{
    [JsonObject]
    public class AlexaResponse
    {
        public AlexaResponse()
        {
            Version = "1.0";
            Session = new SessionAttributes();
            Response = new ResponseAttributes();
        }

        public AlexaResponse(string outputSpeechText)
            : this()
        {
            Response.OutputSpeech.Text = outputSpeechText;
            Response.Card.Content = outputSpeechText;
        }

        public AlexaResponse(string outputSpeechText, bool shouldEndSession)
            : this()
        {
            Response.OutputSpeech.Text = outputSpeechText;
            Response.ShouldEndSession = shouldEndSession;

            if (shouldEndSession)
                Response.Card.Content = outputSpeechText;
            else
                Response.Card = null;
        }

        public AlexaResponse(string outputSpeechText, string ssmlVersion, bool isSsml)
            : this()
        {
            Response.OutputSpeech.Text = outputSpeechText;
            Response.Card.Content = outputSpeechText;
            Response.OutputSpeech.Type = "SSML";
            Response.OutputSpeech.Ssml = "<speak>" + outputSpeechText + "</speak>";
        }

        public AlexaResponse(string outputSpeechText, string cardContent)
            : this()
        {
            Response.OutputSpeech.Text = outputSpeechText;
            Response.Card.Content = cardContent;
        }

        [JsonProperty("version")] public string Version { get; set; }

        [JsonProperty("sessionAttributes")] public SessionAttributes Session { get; set; }

        [JsonProperty("response")] public ResponseAttributes Response { get; set; }

        [JsonObject("sessionAttributes")]
        public class SessionAttributes
        {
            [JsonProperty("memberId")] public int MemberId { get; set; }
        }

        [JsonObject("response")]
        public class ResponseAttributes
        {
            public ResponseAttributes()
            {
                ShouldEndSession = true;
                OutputSpeech = new OutputSpeechAttributes();
                Card = new CardAttributes();
                Reprompt = new RepromptAttributes();
            }

            [JsonProperty("shouldEndSession")] public bool ShouldEndSession { get; set; }

            [JsonProperty("outputSpeech")] public OutputSpeechAttributes OutputSpeech { get; set; }

            [JsonProperty("card")] public CardAttributes Card { get; set; }

            [JsonProperty("reprompt")] public RepromptAttributes Reprompt { get; set; }

            [JsonObject("outputSpeech")]
            public class OutputSpeechAttributes
            {
                public OutputSpeechAttributes()
                {
                    Type = "PlainText";
                }

                [JsonProperty("type")] public string Type { get; set; }

                [JsonProperty("text")] public string Text { get; set; }

                [JsonProperty("ssml")] public string Ssml { get; set; }
            }

            [JsonObject("card")]
            public class CardAttributes
            {
                public CardAttributes()
                {
                    Type = "Simple";
                }

                [JsonProperty("type")] public string Type { get; set; }

                [JsonProperty("title")] public string Title { get; set; }

                [JsonProperty("content")] public string Content { get; set; }
            }

            [JsonObject("reprompt")]
            public class RepromptAttributes
            {
                public RepromptAttributes()
                {
                    OutputSpeech = new OutputSpeechAttributes();
                }

                [JsonProperty("outputSpeech")] public OutputSpeechAttributes OutputSpeech { get; set; }
            }
        }
    }
}