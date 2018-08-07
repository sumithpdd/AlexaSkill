using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AlexaSkill.Adapters.Twitter;
using AlexaSkill.Classes;
using AlexaSkill.Data;
using AlexaSkill.Models;
using Microsoft.Ajax.Utilities;

namespace AlexaSkill.Controllers
{
    public class AlexaController : ApiController
    {
        [HttpPost, Route("api/alexa/HabitatHome")]
        public dynamic HabitatHome(AlexaRequest alexaRequest)
        {
            var request = new Requests().Create(new Data.Request
            {
                MemberId = alexaRequest.Session.Attributes?.MemberId ?? 0,
                TimeStamp = alexaRequest.Request.Timestamp,
                Intent = (alexaRequest.Request.Intent == null) ? "" : alexaRequest.Request.Intent.Name,
                AppId = alexaRequest.Session.Application.ApplicationId,
                RequestId = alexaRequest.Request.RequestId,
                SessionId = alexaRequest.Session.SessionId,
                UserId = alexaRequest.Session.User.UserId,
                IsNew = alexaRequest.Session.New,
                Version = alexaRequest.Version,
                Type = alexaRequest.Request.Type,
                Reason = alexaRequest.Request.Reason,
                SlotsList = alexaRequest.Request.Intent?.GetSlots(),
                DateCreated = DateTime.UtcNow
            });

            AlexaResponse response = null;


            switch (request.Type)
            {
                case "LaunchRequest":
                    response = LaunchRequestHandler(request);
                    break;
                case "IntentRequest":
                    response = IntentRequestHandler(request);
                    break;
                case "SessionEndedRequest":
                    response = SessionEndedRequestHandler(request);
                    break;
            }

            return response;
        }

        private AlexaResponse LaunchRequestHandler(Request request)
        {
            var response = new AlexaResponse("Welcome to Habitat Home. What would you like to hear, the Top Products or New products?");
            response.Session.MemberId = request.MemberId;
            response.Response.Card.Title = "Habitat home";
            response.Response.Card.Content = "Hello\ncruel world!";
            response.Response.Reprompt.OutputSpeech.Text = "Please pick one, Top products or New products?";
            response.Response.ShouldEndSession = false;

            return response;
        }

        private AlexaResponse IntentRequestHandler(Request request)
        {
            AlexaResponse response = null;

            switch (request.Intent)
            {
                case AlexaIntents.Showbiointent:
                    response = ShowBioIntentHandler(request);
                    break;
                case AlexaIntents.Newproductsintent:
                    response = NewProductsIntentHandler(request);
                    break;
                case AlexaIntents.Topproductsintent:
                    response = TopProductsIntentHandler(request);
                    break;
                case AlexaIntents.AmazonCancelintent:
                case AlexaIntents.AmazonStopintent:
                    response = CancelOrStopIntentHandler(request);
                    break;
                case AlexaIntents.AmazonHelpintent:
                    response = HelpIntent(request);
                    break;
                case AlexaIntents.Whereistheorderintent:
                    response = WhereIsTheOrderIntentHandler(request);
                    break;
                case AlexaIntents.PickAWinnerintent:
 
                    response =PickAWinnerIntentHandler(request)  ; 
                    break;

            }

            return response;
        }

        private AlexaResponse ShowBioIntentHandler(Request request)
        {
            var text =
                "Sumith Parambat Damodaran ! working as Director of engineering at <emphasis level=\"strong\">THINK</emphasis>, <break time=\"1s\"/> Previously he was working with <amazon:effect name=\"whispered\">Sitecore UK </amazon:effect> Leading the Services team.";
            var response = new AlexaResponse(text, text, true);
            return response;
        }

        private AlexaResponse WhereIsTheOrderIntentHandler(Request request)
        {
            var text =
                "Your order is in route with delivery.";
            var response = new AlexaResponse(text, text, true);
            return response;
        }
        private   AlexaResponse PickAWinnerIntentHandler(Request request)
        {
            TwitterAdapter.ConsumerKey = TwitterConfiguration.ConsumerKeyApiKey;
            TwitterAdapter.ConsumerSecret = TwitterConfiguration.ConsumerSecretApiSecret;
            var query =TwitterConfiguration.DefaultQuery;
            var results =  TwitterAdapter.SearchAsync(query);

            var model = new TwitterSearch() { Query = query, TwitterResult = results };
            var result = model.TwitterResult[new Random().Next(model.TwitterResult.Count)];
 
             using (var db = new alexaskilldemoEntities())
            {
                var competitionWinner = db.CompetitionWinners.FirstOrDefault();

                if (competitionWinner == null)
                {
                    db.CompetitionWinners.Add(new CompetitionWinner()
                    {
                        Name = result.ScreenNameResponse,
                        Tweet = result.Text,
                        CreatedDate = result.CreatedAt,
                        ProfileImageUrl = result.ProfileImageUrl,
                        UpdatedDate = DateTime.UtcNow
                    });
                }
                else
                {
                    competitionWinner.Name = result.ScreenNameResponse;
                    competitionWinner.Tweet = result.Text;
                    competitionWinner.CreatedDate = result.CreatedAt;
                    competitionWinner.ProfileImageUrl = result.ProfileImageUrl;
                    competitionWinner.UpdatedDate = DateTime.UtcNow;
                    }

                db.SaveChanges();
            }
            var text ="I have selected " + result.ScreenNameResponse + " - Tweet " + result.Text;
            var response = new AlexaResponse(text, text, true);
            return response;
        }
        private AlexaResponse HelpIntent(Request request)
        {
            var response = new AlexaResponse("To use the Habitat home skill, you can say, Alexa, ask Habitat home for top products, to retrieve the top products or say, Alexa, ask Habitat home for the new products, to retrieve the latest new products. You can also say, Alexa, stop or Alexa, cancel, at any time to exit the Habitat home skill. For now, do you want to hear the Top products or New products?", false);
            response.Response.Reprompt.OutputSpeech.Text = "Please select one, top products or new products?";
            return response;
        }

        private AlexaResponse CancelOrStopIntentHandler(Request request)
        {
            return new AlexaResponse("Thanks for listening, let's talk again soon.", true);
        }

        private AlexaResponse NewProductsIntentHandler(Request request)
        {
            var output = new StringBuilder("Here are the latest products. ");

            using (var db = new alexaskilldemoEntities())
            {
                db.Products.Take(10).OrderByDescending(c => c.CreatedDate).ToList()
                    .ForEach(c => output.AppendFormat("{0} cost {1}. ", c.ProductName, c.Price));
            }

            return new AlexaResponse(output.ToString());
        }

        private AlexaResponse TopProductsIntentHandler(Request request)
        {
            int limit = 10;
            var criteria = string.Empty;

            if (request.SlotsList.Any())
            {
                int maxLimit = 10;
                var limitValue = request.SlotsList.FirstOrDefault(s => s.Key == "Limit").Value;

                if (!string.IsNullOrWhiteSpace(limitValue) && int.TryParse(limitValue, out limit) && !(limit >= 1 && limit <= maxLimit))
                {
                    limit = maxLimit;
                }

                criteria = request.SlotsList.FirstOrDefault(s => s.Key == "Criteria").Value;
            }

            var output = new StringBuilder();
            output.AppendFormat("Here are the top {0} {1}. ", limit, string.IsNullOrWhiteSpace(criteria) ? "products" : criteria);

            using (var db = new alexaskilldemoEntities())
            {
                if (criteria == "make")
                    db.Products.Take(limit).OrderByDescending(c => c.Votes).ToList()
                        .ForEach(c => output.AppendFormat("{0}. ", c.Description));
                else
                    db.Products.Take(limit).OrderByDescending(c => c.Votes).ToList()
                        .ForEach(c => output.AppendFormat("{0} and price {1}. ", c.ProductName, c.Price));
            }

            return new AlexaResponse(output.ToString());
        }

        private AlexaResponse SessionEndedRequestHandler(Request request)
        {
            return null;
        }
    }
}
