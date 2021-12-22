using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text.Json;
namespace QCS
{
    public class WhitelistApproved : WhiteListRequest
    {
        private static readonly HttpClient client = new HttpClient();
        public String uuid { get; set; }
        public WhitelistApproved(String username, String email, String studentNumber) : base(username, email, studentNumber) { uuid = ""; }
        public WhitelistApproved(WhiteListRequest wlr) : base(wlr.Username, wlr.Email, wlr.studentNumber) { uuid = ""; }

        public async Task<String> GetJSON()
        {
            if (String.IsNullOrEmpty(uuid)) await getUUID();
            if (String.IsNullOrEmpty(uuid)) return "";
            return String.Format("{{\n\t\"uuid\":\"{0}\",\n\t\"name\":\"{1}\"\n}}", this.uuid, this.Username);
        }

        private async Task getUUID()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var stream = client.GetStreamAsync(String.Format("https://playerdb.co/api/player/minecraft/{0}", this.Username));

            PlayerDB? playerInfo = (await JsonSerializer.DeserializeAsync<PlayerDB>(await stream));

            if(playerInfo!=null && playerInfo.code=="player.found"){
                //Console.WriteLine(String.Format("UUID {0} found for {1}", playerInfo.data.player.id, this.Username));
                this.uuid = playerInfo.data.player.id;
            }
            else {
                Console.WriteLine("Error getting UUID for " + this.Username);
            }

          
        }
    }
}