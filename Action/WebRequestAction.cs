using SuchByte.MacroDeck.ActionButton;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Plugins;
using SuchByte.MacroDeck.Variables;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ziopuzzle.WebRequest.Form;
using ziopuzzle.WebRequest.Model;
using ziopuzzle.WebRequest.Util;

namespace ziopuzzle.WebRequest.Action
{
    public class WebRequestAction : PluginAction
    {
        public override string Name => "Web Request";
        public override string Description => "Send web request with any method, headers and body.\nTo use a variable, write it as {{variable_name}}.\nVariables can be used with any textbox.";
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
            string url = VariableParser.Parse(config.Url);
            string method = VariableParser.Parse(config.Method) ?? "GET";
            string varName = VariableParser.Parse(config.ResponseVariableName);

            try
            {
                using var client = new HttpClient();
                var request = new HttpRequestMessage(new HttpMethod(method.ToUpper()), url);
                string contentType = "application/json";

                client.DefaultRequestHeaders.UserAgent.ParseAdd("MacroDeck-WebRequest/1.0");

                foreach (var header in config.Headers)
                {
                    if (!string.IsNullOrWhiteSpace(header.Key))
                    {
                        string headerValue = VariableParser.Parse(header.Value);
                        request.Headers.TryAddWithoutValidation(header.Key, headerValue);
                        if (header.Key == "Content-Type")
                        {
                            contentType = headerValue;
                        }
                    }
                }

                string body = VariableParser.Parse(config.Body);
                if (!string.IsNullOrWhiteSpace(body) && config.Method != "GET" && config.Method != "DELETE")
                {
                    request.Content = new StringContent(body, Encoding.UTF8, contentType);
                }

                if (request == null)
                {
                    throw new Exception("variable 'request' is null");
                }
                var response = await client.SendAsync(request);

                int statusCode = (int)response.StatusCode;
                string responseBody = await response.Content.ReadAsStringAsync();

                VariableManager.SetValue(
                    $"wr_response_status",
                    statusCode,
                    VariableType.Integer,
                    Main.Instance,
                    []
                );

                VariableManager.SetValue(
                    $"wr_response_body",
                    responseBody,
                    VariableType.String,
                    Main.Instance,
                    []
                );

                if (!string.IsNullOrWhiteSpace(varName))
                {
                    VariableManager.SetValue(
                        $"{varName}_status",
                        statusCode,
                        VariableType.Integer,
                        Main.Instance,
                        []
                    );

                    VariableManager.SetValue(
                        $"{varName}_body",
                        responseBody,
                        VariableType.String,
                        Main.Instance,
                        []
                    );
                }
            } catch (Exception ex)
            {
                VariableManager.SetValue(
                    $"wr_response_status",
                    0,
                    VariableType.Integer,
                    Main.Instance,
                    []
                );

                VariableManager.SetValue(
                    $"wr_response_body",
                    ex.Message,
                    VariableType.String,
                    Main.Instance,
                    []
                );
            }
        }
    }
}
