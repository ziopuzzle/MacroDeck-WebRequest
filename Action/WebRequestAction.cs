using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Plugins;
using SuchByte.MacroDeck.Variables;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ziopuzzle.WebRequest.Form;
using ziopuzzle.WebRequest.Model;

namespace ziopuzzle.WebRequest.Action
{
    public class WebRequestAction : PluginAction
    {
        public override string Name => "Web Request";
        public override string Description => "Send web request with any method, headers and body.";
        public override bool CanConfigure => true;

        public override ActionConfigControl GetActionConfigControl(ActionConfigurator actionConfigurator)
        {
            return new WebRequestConfigControl(this, actionConfigurator);
        }

        public override void Trigger(string clientId, ActionButton actionButton)
        {
            if (string.IsNullOrWhiteSpace(this.Configuration)) return;

            Task.Run(async () =>
            {
                try
                {
                    var config = JsonSerializer.Deserialize<HttpRequestModel>(this.Configuration);
                    if (config == null || string.IsNullOrWhiteSpace(config.Url)) return;

                    await ExecuteHttpRequestAsync(config);
                }
                catch (Exception)
                {
                }
            });
        }

        private async Task ExecuteHttpRequestAsync(HttpRequestModel config)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage(new HttpMethod(config.Method.ToUpper()), config.Url);

            client.DefaultRequestHeaders.UserAgent.ParseAdd("MacroDeck-WebRequest/1.0");

            foreach (var header in config.Headers)
            {
                if (!string.IsNullOrWhiteSpace(header.Key))
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(config.Body) && config.Method != "GET" && config.Method != "DELETE")
            {
                request.Content = new StringContent(config.Body, Encoding.UTF8, config.ContentType);
            }

            try
            {
                var response = await client.SendAsync(request);

                int statusCode = (int)response.StatusCode;
                string responseBody = await response.Content.ReadAsStringAsync();

                VariableManager.SetValue(
                    $"wr_responce_status",
                    statusCode,
                    VariableType.Integer,
                    Main.Instance,
                    []
                );

                VariableManager.SetValue(
                    $"wr_responce_body",
                    responseBody,
                    VariableType.String,
                    Main.Instance,
                    []
                );
            } catch (Exception)
            {

            }
        }
    }
}
