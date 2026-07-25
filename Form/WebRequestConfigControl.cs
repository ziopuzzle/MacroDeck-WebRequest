using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using System.Drawing;
using System.Text.Json;
using System.Windows.Forms;
using ziopuzzle.WebRequest.Action;
using ziopuzzle.WebRequest.Model;

namespace ziopuzzle.WebRequest.Form
{
    public class WebRequestConfigControl : ActionConfigControl
    {
        private readonly WebRequestAction _action;

        private System.Windows.Forms.ComboBox? _cbMethod;
        private TextBox? _txtUrl;
        private TextBox? _txtBody;
        private FlowLayoutPanel? _headersContainer;

        public WebRequestConfigControl(WebRequestAction action, ActionConfigurator actionConfigurator)
        {
            _action = action;
            InitializeUI();
            LoadConfiguration();
        }

        private void InitializeUI()
        {
            var lblMethod = new Label { Text = "Method", Location = new Point(0, 0), AutoSize = true, ForeColor = Color.White };
            _cbMethod = new System.Windows.Forms.ComboBox { Location = new Point(0, 24), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
            _cbMethod.Items.AddRange(["GET", "POST", "PUT", "DELETE", "PATCH"]);
            _cbMethod.SelectedIndex = 0;

            var lblUrl = new Label { Text = "URL", Location = new Point(128, 0), AutoSize = true, ForeColor = Color.White };
            _txtUrl = new TextBox { Location = new Point(128, 24), Width = 400 };

            var lblBody = new Label { Text = "Body", Location = new Point(0, 64), AutoSize = true, ForeColor = Color.White };
            _txtBody = new TextBox { Location = new Point(0, 88), Width = 510, Height = 150, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false };

            var lblHeaders = new Label { Text = "Headers", Location = new Point(0, 242), AutoSize = true, ForeColor = Color.White };
            var btnAddHeader = new Button { Text = "+", Location = new Point(104, 242), Width = 40, ForeColor = Color.White, UseCompatibleTextRendering = true };
            btnAddHeader.Click += (s, e) => AddHeaderRow("", "");

            _headersContainer = new FlowLayoutPanel
            {
                Location = new Point(0, 266),
                Width = 510,
                Height = 150,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BorderStyle = BorderStyle.FixedSingle
            };

            this.Controls.Add(lblMethod);
            this.Controls.Add(_cbMethod);
            this.Controls.Add(lblUrl);
            this.Controls.Add(_txtUrl);
            this.Controls.Add(lblBody);
            this.Controls.Add(_txtBody);
            this.Controls.Add(lblHeaders);
            this.Controls.Add(btnAddHeader);
            this.Controls.Add(_headersContainer);
        }

        private void AddHeaderRow(string key, string value)
        {
            var rowPanel = new Panel { Width = 480, Height = 30, Margin = new Padding(0, 2, 0, 2) };
            var txtKey = new TextBox { Width = 150, Location = new Point(0, 0), Text = key, PlaceholderText = "Name" };
            var txtValue = new TextBox { Width = 280, Location = new Point(160, 0), Text = value, PlaceholderText = "Value" };
            var btnRemove = new Button { Text = "x", Width = 30, Location = new Point(450, 0), ForeColor = Color.White, UseCompatibleTextRendering = true };
            btnRemove.Click += (s, e) => _headersContainer?.Controls.Remove(rowPanel);

            rowPanel.Controls.Add(txtKey);
            rowPanel.Controls.Add(txtValue);
            rowPanel.Controls.Add(btnRemove);

            _headersContainer?.Controls.Add(rowPanel);
        }

        private void LoadConfiguration()
        {
            if (string.IsNullOrWhiteSpace(_action.Configuration)) return;

            var config = JsonSerializer.Deserialize<HttpRequestModel>(_action.Configuration);
            if (config == null) return;

            _cbMethod?.SelectedItem = config.Method;
            _txtUrl?.Text = config.Url;
            _txtBody?.Text = config.Body;

            _headersContainer?.Controls.Clear();
            if (config.Headers != null)
            {
                foreach (var header in config.Headers)
                {
                    AddHeaderRow(header.Key, header.Value);
                }
            }
        }

        public override bool OnActionSave()
        {
            var config = new HttpRequestModel
            {
                Method = _cbMethod?.SelectedItem?.ToString() ?? "GET",
                Url = _txtUrl?.Text ?? "",
                Body = _txtBody?.Text ?? "",
                Headers = []
            };

            if (_headersContainer != null)
            {
                foreach (Control control in _headersContainer.Controls)
                {
                    if (control is Panel row)
                    {
                        TextBox? txtKey = row.Controls[0] as TextBox;
                        TextBox? txtVal = row.Controls[1] as TextBox;

                        if (txtKey != null && txtVal != null && !string.IsNullOrWhiteSpace(txtKey.Text))
                        {
                            config.Headers[txtKey.Text] = txtVal.Text;
                        }
                    }
                }
            }

            _action.Configuration = JsonSerializer.Serialize(config);
            return true;
        }
    }
}
