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
    private JsonConfigNode _rootNode;
    private readonly Font _defaultFont = new Font(new FontFamily("Consolas"), 12, FontStyle.Regular, GraphicsUnit.Pixel);
    private readonly Font _propertyFont = new Font(new FontFamily("Consolas"), 12, FontStyle.Bold, GraphicsUnit.Pixel);

    public Form1()
    {
      InitializeComponent();
      InitializeData();
      FillTree();
    }

    private void FillTree()
    {
      this.treeListView1.ClearObjects();
      this.treeListView1.CanExpandGetter = x => ((JsonConfigNode)x).Children.Count > 0;
      this.treeListView1.ChildrenGetter = x => (x as JsonConfigNode)?.Children;
      var keyCol = new BrightIdeasSoftware.OLVColumn("Key", "Key")
      {
        AspectGetter = x => (x as JsonConfigNode)?.Key,
        IsEditable = false,
      };
      var valueCol = new BrightIdeasSoftware.OLVColumn("Value", "Value")
      {
        AspectGetter = x => (x as JsonConfigNode)?.Value,
        IsEditable = true,
        CellEditUseWholeCell = true
      };

      // add the columns to the tree
      this.treeListView1.Columns.Add(keyCol);
      this.treeListView1.Columns.Add(valueCol);
      this.treeListView1.Roots = _rootNode.Children;
      this.treeListView1.ExpandAll();

      this.treeListView1.Font = _defaultFont;
    }

    private void InitializeData()
    {
      var text = System.IO.File.ReadAllText("D:/temp/transform.json");

      var node = JToken.Parse(text);
      var root = new JsonConfigNode("root", null, node);
      WalkNode(node, root);
      _rootNode = root;
    }

    public void WalkNode(JToken node, JsonConfigNode currentNode)
    {
      if (node.Type == JTokenType.Object)
      {
        foreach (var child in node.Children<JToken>())
        {
          WalkNode(child, currentNode);
        }
      }
      else if (node.Type == JTokenType.Array)
      {
        var i = 0;
        foreach (var child in node.Children())
        {
          var item = new JsonConfigNode($"[{i}]", null, child);
          currentNode.Children.Add(item);
          WalkNode(child, item);
          i++;
        }
      }
      else if (node.Type == JTokenType.Property)
      {
        var prop = new JsonConfigNode(((JProperty)node).Name, null, node);
        currentNode.Children.Add(prop);
        foreach (var child in node.Children())
        {
          WalkNode(child, prop);
        }
      }
      else
      {
        currentNode.Value = (((Newtonsoft.Json.Linq.JValue)node).Value);
      }
    }

    private void Form1_Load(object sender, EventArgs e)
    {

      foreach (ColumnHeader c in this.treeListView1.Columns)
      {
        c.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
      }
    }

    private void treeListView1_CellRightClick(object sender, BrightIdeasSoftware.CellRightClickEventArgs e)
    {
      //e.MenuStrip = new ContextMenuStrip();
      //MessageBox.Show("HI");
    }

    private void treeListView1_FormatCell(object sender, BrightIdeasSoftware.FormatCellEventArgs e)
    {
      if (e.ColumnIndex == 0)
      {
        e.SubItem.Font = _propertyFont;
        return;
      }
      var node = (JsonConfigNode)e.Model;

      if (node.Value == null)
      {
        return;
      }

      var type = node.Value.GetType();
      if (type == typeof(string))
      {
        e.SubItem.ForeColor = Color.DarkGreen;
      }
      else if (type == typeof(int) || type == typeof(long) || type == typeof(float))
      {
        e.SubItem.ForeColor = Color.DarkBlue;
      }
      else if (type == typeof(bool))
      {
        e.SubItem.ForeColor = Color.Blue;
      }
      //if (node.)
      e.SubItem.Font = _defaultFont;
    }
  }

  public class JsonConfigNode
  {
    public string Key { get; set; }
    public object Value { get; set; }
    public JToken Token { get; set; }
    public List<JsonConfigNode> Children { get; set; }

    public JsonConfigNode(string key, object value, JToken token)
    {
      this.Token = token;
      this.Key = key;
      this.Value = value;
      this.Children = new List<JsonConfigNode>();
    }
  }
}
