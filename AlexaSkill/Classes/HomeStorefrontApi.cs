using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using AlexaSkill.Models.Commerce;
using Build.Labs.BotFramework.Models.Responce;
using Newtonsoft.Json;
#pragma warning disable 1587
/// <summary>
/// Use postman
//  1.	For recent order of the user : http://homestorefront.northeurope.cloudapp.azure.com/api/BabyBot/Cartbot/GetRecentOrders ( Post Request ) 
//  2.	Get Top Product : http://homestorefront.northeurope.cloudapp.azure.com/api/BabyBot/Cartbot/GetPromotedProductsJsonResult ( Post Request)
//              currentCatalogItemId : {803E72C3-5ADC-EBE3-7AEE-5956712C95C8}
//              relationshipFieldId: {A96D901B-BF4B-4960-9F55-65485F2C2FE7}
//  3.	Get Mini cart : http://homestorefront.northeurope.cloudapp.azure.com/api/BabyBot/Cartbot/GetMinicart ( Get Request )
//4.	Add cart : http://homestorefront.northeurope.cloudapp.azure.com/api/BabyBot/Cartbot/AddCartLine ( post )
//              addToCart_CatalogName : Habitat_Master
//              addToCart_ProductId : anything(example Mira laptop ) 6042185
///              quantity: 20
/// </summary>
#pragma warning restore 1587
namespace AlexaSkill.Classes
{
    public class HomeStorefrontApi
    {
        private string _baseUrl = "http://homestorefront.northeurope.cloudapp.azure.com/api/BabyBot";
        public async Task<MiniCart> MiniCart()
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(_baseUrl + "/Cartbot/GetMinicart");
                var contentResp = await response.Result.Content.ReadAsStringAsync();
                
                var result = JsonConvert.DeserializeObject<MiniCart>(contentResp);
                return result;
            }
        }
        public async Task<IList<TopProduct>> TopProduct()
        {
            using (var client = new HttpClient())
            {


                var values = new Dictionary<string, string>
                {
                    {"relationshipFieldId","{A96D901B-BF4B-4960-9F55-65485F2C2FE7}"},
                    {"currentCatalogItemId","{803E72C3-5ADC-EBE3-7AEE-5956712C95C8}"}
                };

                var content = new FormUrlEncodedContent(values);

                var response =   client.PostAsync(_baseUrl + "/Cartbot/GetPromotedProductsJsonResult", content);

                string contentResp = await response.Result.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<IList<TopProduct>>(contentResp);
                return result;
            }
        }
        async Task<string> SendGetRequest(string fullUrl,
            Dictionary<string, string> requestParameters)
        {

            using (var http = new HttpClient())
            {

                var httpResp = await http.GetAsync(fullUrl + "?"
                                                           + requestParameters.ToWebString());
                var respBody = await httpResp.Content.ReadAsStringAsync();

                return respBody;
            }
        }
    }
}