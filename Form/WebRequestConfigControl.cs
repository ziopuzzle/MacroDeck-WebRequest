using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.Variables;
using System;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using ziopuzzle.WebRequest.Action;
using ziopuzzle.WebRequest.Model;

namespace ziopuzzle.WebRequest.Form
{
    public class WebRequestConfigControl : ActionConfigControl
    {
        private readonly WebRequestAction _action;
        private Control? _lastFocusedControl;

        private System.Windows.Forms.ComboBox? _cbMethod;
        private TextBox? _txtUrl;
        private TextBox? _txtBody;
        private TextBox? _txtVarName;
        private FlowLayoutPanel? _headersContainer;
        
        public WebRequestConfigControl(WebRequestAction action, ActionConfigurator actionConfigurator)
        {
            _action = action;
            InitializeUI();
            LoadConfiguration();
        }

        private void Control_Enter(object? sender, EventArgs? e)
        {
            if (sender is TextBox || sender is System.Windows.Forms.ComboBox)
            {
                _lastFocusedControl = (Control)sender;
            }
        }

        private void InitializeUI()
        {
            var lblMethod = new Label { Text = "Method", Location = new Point(0, 0), AutoSize = true, ForeColor = Color.White };
            _cbMethod = new System.Windows.Forms.ComboBox { Location = new Point(0, 24), Width = 96, DropDownStyle = ComboBoxStyle.DropDown };
            _cbMethod.Items.AddRange(["GET", "POST", "PUT", "DELETE", "PATCH"]);
            _cbMethod.Enter += Control_Enter;

            var lblUrl = new Label { Text = "URL", Location = new Point(128, 0), AutoSize = true, ForeColor = Color.White };
            _txtUrl = new TextBox { Location = new Point(128, 24), Width = 448 };
            _txtUrl.Enter += Control_Enter;

            var lblBody = new Label { Text = "Body", Location = new Point(0, 60), AutoSize = true, ForeColor = Color.White };
            var btnInsertVar = new Button { Text = "{x}", Location = new Point(64, 60), Width = 50, ForeColor = Color.White, UseCompatibleTextRendering = true };
            btnInsertVar.Click += BtnInsertVar_Click;
            _txtBody = new TextBox { Location = new Point(0, 84), Width = 576, Height = 144, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false };
            _txtBody.Enter += Control_Enter;

            var lblHeaders = new Label { Text = "Headers", Location = new Point(0, 230), AutoSize = true, ForeColor = Color.White };
            var btnAddHeader = new Button { Text = "+", Location = new Point(104, 230), Width = 40, ForeColor = Color.White, UseCompatibleTextRendering = true };
            btnAddHeader.Click += (s, e) => AddHeaderRow("", "");

            _headersContainer = new FlowLayoutPanel { Location = new Point(0, 254), Width = 576, Height = 112, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, BorderStyle = BorderStyle.FixedSingle };

            var lblVarName = new Label { Text = "Variable Name", Location = new Point(0, 376), AutoSize = true, ForeColor = Color.White };
            _txtVarName = new TextBox { Location = new Point(156, 376), Width = 422, PlaceholderText = "wr_responce => wr_responce_status(body)" };
            _txtBody.Enter += Control_Enter;

            this.Controls.Add(lblMethod);
            this.Controls.Add(_cbMethod);
            this.Controls.Add(lblUrl);
            this.Controls.Add(_txtUrl);
            this.Controls.Add(lblBody);
            this.Controls.Add(btnInsertVar);
            this.Controls.Add(_txtBody);
            this.Controls.Add(lblHeaders);
            this.Controls.Add(btnAddHeader);
            this.Controls.Add(_headersContainer);
            this.Controls.Add(lblVarName);
            this.Controls.Add(_txtVarName);
        }

        private void BtnInsertVar_Click(object? sender, EventArgs? e)
        {
            var contextMenu = new ContextMenuStrip();
            var variables = VariableManager.Variables;

            if (variables != null && variables.Length > 0)
            {
                string? myPluginName = Main.Instance?.GetType()?.Namespace;

                var groupedVariables = variables.GroupBy(v => v.Creator).OrderBy(g =>
                {
                    string creator = g.Key ?? "";
                    
                    if (creator == "User")
                    {
                        return 0;
                    }
                    if (!string.IsNullOrEmpty(myPluginName) && creator.Equals(myPluginName, StringComparison.OrdinalIgnoreCase))
                    {
                        return 1;
                    }
                    if (string.IsNullOrWhiteSpace(creator))
                    {
                        return 2;
                    }
                    return 3;
                }).ThenBy(g => g.Key);

                foreach (var g in groupedVariables)
                {
                    string creatorName = string.IsNullOrWhiteSpace(g.Key) ? "other" : g.Key;
                    var creatorMenuItem = new ToolStripMenuItem(creatorName);

                    foreach (var v in g.OrderBy(v => v.Name))
                    {
                        var variableMenuItem = new ToolStripMenuItem(v.Name);
                        variableMenuItem.Click += (s, ev) =>
                        {
                            if (_lastFocusedControl == null) return;

                            string insertText = "{{" + v.Name + "}}";
                            if (_lastFocusedControl is TextBox tb)
                            {
                                int selectionIndex = tb.SelectionStart;
                                tb.Text = tb.Text.Insert(selectionIndex, insertText);

                                tb.SelectionStart = selectionIndex + insertText.Length;
                                tb.Focus();
                            }
                            else if (_lastFocusedControl is System.Windows.Forms.ComboBox cb)
                            {
                                int selectionIndex = cb.SelectionStart;
                                cb.Text = cb.Text.Insert(selectionIndex, insertText);
                                cb.SelectionStart = selectionIndex + insertText.Length;
                                cb.Focus();
                            }
                        };

                        creatorMenuItem.DropDownItems.Add(variableMenuItem);
                    }

                    contextMenu.Items.Add(creatorMenuItem);
                }
            }
            else
            {
                contextMenu.Items.Add(new ToolStripMenuItem("Variables not found") { Enabled = false });
            }

            if (sender is Button btn)
            {
                contextMenu.Show(btn, new Point(0, btn.Height));
            }
        }

        private void AddHeaderRow(string key, string value)
        {
            var rowPanel = new Panel { Width = 480, Height = 30, Margin = new Padding(0, 2, 0, 2) };
            var txtKey = new TextBox { Width = 150, Location = new Point(0, 0), Text = key, PlaceholderText = "Name" };
            txtKey.Enter += Control_Enter;
            var txtValue = new TextBox { Width = 280, Location = new Point(160, 0), Text = value, PlaceholderText = "Value" };
            txtValue.Enter += Control_Enter;
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

            _cbMethod?.Text = config.Method;
            _txtUrl?.Text = config.Url;
            _txtBody?.Text = config.Body;
            _txtVarName?.Text = config.ResponseVariableName;

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
                Method = string.IsNullOrWhiteSpace(_cbMethod?.Text) ? "GET" : _cbMethod.Text,
                Url = _txtUrl?.Text ?? "",
                Body = _txtBody?.Text ?? "",
                ResponseVariableName = _txtVarName?.Text ?? "",
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
