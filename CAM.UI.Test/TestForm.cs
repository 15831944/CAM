﻿//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.Runtime;
using CAM.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CAM.UI.Test
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();

            var techProcessService = new CamManager(new AcadGatewayMock(listBox));
            techProcessView.Init(techProcessService);
            techProcessService.ProgramGenerated += (object sender, ProgramEventArgs e) => programView.ShowProgram(e.Program);
        }
	}
}
