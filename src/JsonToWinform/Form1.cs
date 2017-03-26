using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JsonToWinform
{
  public partial class Form1 : Form
  {
    private ILogger _logger;

    private Control _lastControl;

    public Form1()
    {
      InitializeComponent();
    }

    public void CreateConfigControls(string path)
    {
      var text = System.IO.File.ReadAllText(path);
      dynamic config = JsonConvert.DeserializeObject(text);

      JToken node = JToken.Parse(text);
      WalkNode(node, JObjectAction, JPropertyAction, ValueAction);
    }

    public void JObjectAction(JObject obj)
    {
      _logger.Warn("JObject");
      var p = new FlowLayoutPanel();
      _lastControl.Controls.Add(p);
      _lastControl = p;
    }

    public void JPropertyAction(JProperty property)
    {
      _logger.Warn($"{property.Name} -- ");
      
      var gb = new GroupBox() { Text = property.Name };
      _lastControl.Controls.Add(gb);
      _lastControl = gb;      
    }

    public void ValueAction(object value)
    {
      _logger.Warn(value.ToString());
      var p = new FlowLayoutPanel { Dock = DockStyle.Fill };
      var tb = new TextBox();
      tb.Text = value.ToString();
      p.Controls.Add(tb);      
      _lastControl.Controls.Add(p);
    }

    public void WalkNode(
      JToken node, 
      Action<JObject> objectAction, 
      Action<JProperty> propertyAction,
      Action<object> valueAction)
    {
      if (node.Type == JTokenType.Object)
      {
        objectAction((JObject)node);

        foreach (JToken child in node.Children<JToken>())
        {
          WalkNode(child, objectAction, propertyAction, valueAction);
        }
      }
      else if (node.Type == JTokenType.Array)
      {
        foreach (JToken child in node.Children())
        {
          WalkNode(child, objectAction, propertyAction, valueAction);
        }
      }
      else if (node.Type == JTokenType.Property)
      {
        propertyAction((JProperty)node);
        foreach (JToken child in node.Children())
        {
          WalkNode(child, objectAction, propertyAction, valueAction);
        }
      }
      else
      {
        valueAction((object)node);
      }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      _logger = LogManager.GetCurrentClassLogger();
      _lastControl = this.FlowLayoutPanel;
      CreateConfigControls("D:/temp/config.json");
    }
  }
}
