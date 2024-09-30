using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kitware.VTK;

namespace PointCloud
{
    public partial class Form1 : Form
    {
        private string selectedFilePath = ""; // 添加一个成员变量来存储选定的文件路径        
        RenderWindowControl renderWindowControl;// 创建渲染窗口控件
        public Form1()
        {
            InitializeComponent();
            this.Controls.Add(this.renderWindowControl=new RenderWindowControl());
            this.renderWindowControl.Dock = DockStyle.Fill;
            
            //将 Form1_Load 方法订阅为窗体的 Load 事件的处理程序
            this.Load += new System.EventHandler(this.Form1_Load);
            //将 Form1_Resize 方法订阅为窗体的 Resize 事件的处理程序
            this.Resize += new System.EventHandler(this.Form1_Resize);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            // 在窗体大小调整时更新按钮宽度
            SetButtonWidths();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            // 在窗体加载时设置按钮宽度
            SetButtonWidths();
        }
        private void SetButtonWidths()
        {
            int flowLayoutPanelWidth = this.flowLayoutPanel1.ClientSize.Width;
            this.button1.Width = flowLayoutPanelWidth / 2 - this.button1.Margin.Horizontal;
            this.button2.Width = flowLayoutPanelWidth / 2 - this.button2.Margin.Horizontal;
        }
       
        private void btnDisplay_Click(object sender, EventArgs e)
        {
            renderWindowControl.Dock = DockStyle.Fill; // 填充窗体
            //this.flowLayoutPanel1.Controls.Clear(); // 清除 flowLayoutPanel 上的所有控件
            //this.Controls.Add(renderWindowControl); // 添加到窗体的控件集合中

            // 检查是否已选择文件
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                MessageBox.Show("请先选择一个点云文件！");
                return;
            }

            // 读取点云文件并获取 vtkPoints
            vtkPoints points = ReadPointCloudFromFile(selectedFilePath);

            // 创建PolyData
            vtkPolyData polydata = vtkPolyData.New();
            polydata.SetPoints(points);

            // 创建顶点Glyph滤波器
            vtkVertexGlyphFilter glyphFilter = vtkVertexGlyphFilter.New();
            glyphFilter.SetInputConnection(polydata.GetProducerPort());

            // 创建映射器
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(glyphFilter.GetOutputPort());

            // 创建演员
            vtkActor actor = vtkActor.New();
            actor.SetMapper(mapper);
            actor.GetProperty().SetPointSize(5); // 设置点的大小

            // 获取渲染器并添加演员
            vtkRenderer renderer = renderWindowControl.RenderWindow.GetRenderers().GetFirstRenderer();
            renderer.AddActor(actor);
            renderer.SetBackground(0.1,0.1,0.1); // 设置背景颜色
            renderWindowControl.RenderWindow.Render(); // 执行渲染
        }

        private vtkPoints ReadPointCloudFromFile(string selectedFilePath)
        {
            vtkPoints points = vtkPoints.New();
            try
            {
                // 读取文件中的每行数据
                var lines = System.IO.File.ReadAllLines(selectedFilePath);
                foreach (var line in lines)
                {
                    // 使用空格分割每行数据
                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3) // 确保有足够的数据代表一个点
                    {
                        // 解析点的坐标
                        //double x = double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                        //double y = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);
                        //double z = double.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);


                        // 将点添加到vtkPoints对象
                        //points.InsertNextPoint(x, y, z);

                        double x, y, z;
                        if (double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out x) &&
                            double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out y) &&
                            double.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out z))
                        {
                            // 将点添加到vtkPoints对象
                            points.InsertNextPoint(x, y, z);
                        }
                        else
                        {
                            // 处理无法解析为浮点数的情况
                            Console.WriteLine($"无法解析点：{line}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("读点云文件发生错误: " + ex.Message);
            }
            return points;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();//创建打开文件对象
            openFileDialog.Title = "选择一个点云文件";
            openFileDialog.Filter = "所有支持的文件|*.pcd;*.ply;*.xyz|PCD文件|*.pcd|PLY文件|*.ply|XYZ文件|*.xyz";
            DialogResult result = openFileDialog.ShowDialog();//cancel ok
            if (result == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName))
            {
                selectedFilePath = openFileDialog.FileName; // 保存选定的文件路径
                //MessageBox.Show(selectedFilePath);
            }
        }
    }
}
