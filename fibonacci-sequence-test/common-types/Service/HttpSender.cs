using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using common_types.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;

namespace common_types.Services
{
    public class HttpSender : INumberSender
    {
        private string _uri;

        public HttpSender(string uri)
        {
            _uri = uri;
        }

        public void Send(FibonacciNumber number)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, _uri))
                {
                    string content = JsonConvert.SerializeObject(number);
                    msg.Content = new StringContent(content, Encoding.UTF8, "application/json");
                    client.SendAsync(msg).Wait();
                    //client.PostAsync(_uri, new StringContent(JsonConvert.SerializeObject(new { value = number }), Encoding.UTF8, "application/json")).Wait();
                }
            }
        }
    }
}
