using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace ConsoleApp1
{
    public class Sample3
    {
        public class Account
        {
            public string Email { get; set; }
            public bool Active { get; set; }
            public DateTime CreatedDate { get; set; }
            public IList<string> Roles { get; set; }
        }



        public void Des()
        {

            string json = @"{
                         'Email': 'jame's@example.com',
                         'Active': true,
                         'CreatedDate': '2013-01-20T00:00:00Z',
                         'Roles': [
                           'User',
                           'Admin'
                            ]
                        }";

            var settings = new JsonSerializerSettings
            {
                StringEscapeHandling = StringEscapeHandling.Default,
            };
            

            
            //Console.WriteLine(serialized);

            var account = JsonConvert.DeserializeObject(json);

            //Console.WriteLine(account.Email);

        }
    }
}